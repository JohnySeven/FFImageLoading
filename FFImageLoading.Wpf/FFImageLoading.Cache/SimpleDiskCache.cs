#define DEBUG
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFImageLoading.Config;
using FFImageLoading.Helpers;

namespace FFImageLoading.Cache
{
	public class SimpleDiskCache : IDiskCache
	{
		private readonly SemaphoreSlim fileWriteLock = new SemaphoreSlim(1, 1);

		private readonly SemaphoreSlim _currentWriteLock = new SemaphoreSlim(1, 1);

		private Task initTask = null;

		private string cacheFolderName;

		private DirectoryInfo rootFolder;

		private DirectoryInfo cacheFolder;

		private ConcurrentDictionary<string, byte> fileWritePendingTasks = new ConcurrentDictionary<string, byte>();

		private ConcurrentDictionary<string, CacheEntry> entries = new ConcurrentDictionary<string, CacheEntry>();

		private Task _currentWrite = Task.FromResult((byte)1);

		protected Configuration Configuration
		{
			get;
			private set;
		}

		protected IMiniLogger Logger => Configuration.Logger;

		public SimpleDiskCache(string cacheFolderName, Configuration configuration)
		{
			Configuration = configuration;
			this.cacheFolderName = cacheFolderName;
			initTask = Init();
		}

		public SimpleDiskCache(DirectoryInfo rootFolder, string cacheFolderName, Configuration configuration)
		{
			Configuration = configuration;
			this.rootFolder = rootFolder ?? throw new ArgumentNullException("rootFolder");
			this.cacheFolderName = cacheFolderName ?? throw new ArgumentNullException("cacheFolderName");
			initTask = Init();
		}

		protected virtual async Task Init()
		{
			try
			{
				cacheFolder = await Task.Run(delegate
				{
					if (!rootFolder.Exists)
					{
						rootFolder.Create();
					}
					DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(rootFolder.FullName, cacheFolderName));
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
					}
					return directoryInfo;
				});
				await InitializeEntries().ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				Logger.Debug($"SimpleDiskCache: Unable to create cache folder: {cacheFolder.FullName}, Exception: {ex}");
			}
			finally
			{
				CleanCallback();
			}
		}

		protected virtual async Task InitializeEntries()
		{
			FileInfo[] array = await Task.Run(() => cacheFolder.GetFiles());
			foreach (FileInfo file in array)
			{
				string key = Path.GetFileNameWithoutExtension(file.Name);
				TimeSpan duration = GetDuration(file.Extension);
				entries.TryAdd(key, new CacheEntry(file.CreationTimeUtc, duration, file.Name));
			}
		}

		protected virtual TimeSpan GetDuration(string text)
		{
			string text2 = text.Split(new char[1]
			{
				'.'
			}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
			if (string.IsNullOrWhiteSpace(text2))
			{
				return Configuration.DiskCacheDuration;
			}
			int result;
			return int.TryParse(text2, out result) ? TimeSpan.FromSeconds(result) : Configuration.DiskCacheDuration;
		}

		protected virtual async Task CleanCallback()
		{
			DateTime now = DateTime.UtcNow;
			KeyValuePair<string, CacheEntry>[] kvps = entries.Where((KeyValuePair<string, CacheEntry> kvp) => kvp.Value.Origin + kvp.Value.TimeToLive < now).ToArray();
			KeyValuePair<string, CacheEntry>[] array = kvps;
			foreach (KeyValuePair<string, CacheEntry> kvp2 in array)
			{
				if (!entries.TryRemove(kvp2.Key, out var oldCacheEntry))
				{
					continue;
				}
				try
				{
					Logger.Debug($"SimpleDiskCache: Removing expired file {oldCacheEntry.FileName}");
					await Task.Run(delegate
					{
						File.Delete(oldCacheEntry.FileName);
					});
				}
				catch
				{
				}
			}
		}

		public virtual async Task<string> GetFilePathAsync(string key)
		{
			await initTask.ConfigureAwait(continueOnCapturedContext: false);
			if (!entries.TryGetValue(key, out var entry))
			{
				return null;
			}
			return Path.Combine(cacheFolder.FullName, entry.FileName);
		}

		public virtual async Task<bool> ExistsAsync(string key)
		{
			await initTask.ConfigureAwait(continueOnCapturedContext: false);
			return entries.ContainsKey(key);
		}

		public virtual async Task AddToSavingQueueIfNotExistsAsync(string key, byte[] bytes, TimeSpan duration, Action writeFinished = null)
		{
			await initTask.ConfigureAwait(continueOnCapturedContext: false);
			if (!fileWritePendingTasks.TryAdd(key, 1))
			{
				return;
			}
			await _currentWriteLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				_currentWrite = _currentWrite.ContinueWith((Func<Task, Task>)async delegate
				{
					await Task.Yield();
					await initTask.ConfigureAwait(continueOnCapturedContext: false);
					try
					{
						await fileWriteLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
						string filename = Path.Combine(cacheFolder.FullName, key + "." + (long)duration.TotalSeconds);
						using (FileStream fs = new FileStream(filename, FileMode.Create))
						{
							await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(continueOnCapturedContext: false);
						}
						entries[key] = new CacheEntry(DateTime.UtcNow, duration, filename);
						writeFinished?.Invoke();
					}
					catch (Exception ex2)
					{
						Exception ex = ex2;
						Debug.WriteLine($"An error occured while caching to disk image '{key}'.");
						Debug.WriteLine(ex.ToString());
					}
					finally
					{
						fileWritePendingTasks.TryRemove(key, out var _);
						fileWriteLock.Release();
					}
				});
			}
			finally
			{
				_currentWriteLock.Release();
			}
		}

		public virtual async Task<Stream> TryGetStreamAsync(string key)
		{
			await initTask.ConfigureAwait(continueOnCapturedContext: false);
			await WaitForPendingWriteIfExists(key).ConfigureAwait(continueOnCapturedContext: false);
			Stream ret = null;
			try
			{
				if (!entries.TryGetValue(key, out var entry))
				{
					return null;
				}
				try
				{
					ret = await Task.Run(() => new FileStream(Path.Combine(cacheFolder.FullName, entry.FileName), FileMode.Open));
				}
				catch (IOException ex)
				{
					Logger.Error("Failed to load " + entry.FileName + "!", ex);
				}
				return ret;
			}
			catch
			{
				return null;
			}
		}

		public virtual async Task RemoveAsync(string key)
		{
			await initTask.ConfigureAwait(continueOnCapturedContext: false);
			await WaitForPendingWriteIfExists(key).ConfigureAwait(continueOnCapturedContext: false);
			if (!entries.TryRemove(key, out var oldCacheEntry))
			{
				return;
			}
			try
			{
				await Task.Run(delegate
				{
					File.Delete(Path.Combine(cacheFolder.FullName, oldCacheEntry.FileName));
				});
			}
			catch
			{
			}
		}

		public virtual async Task ClearAsync()
		{
			await initTask.ConfigureAwait(continueOnCapturedContext: false);
			while (fileWritePendingTasks.Count != 0)
			{
				await Task.Delay(20).ConfigureAwait(continueOnCapturedContext: false);
			}
			try
			{
				await fileWriteLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
				FileInfo[] array = await Task.Run(() => cacheFolder.GetFiles());
				foreach (FileInfo item in array)
				{
					try
					{
						await Task.Run(delegate
						{
							item.Delete();
						});
					}
					catch (IOException)
					{
					}
				}
			}
			catch (IOException)
			{
			}
			finally
			{
				entries.Clear();
				fileWriteLock.Release();
			}
		}

		protected virtual async Task WaitForPendingWriteIfExists(string key)
		{
			while (fileWritePendingTasks.ContainsKey(key))
			{
				await Task.Delay(20).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}
}
