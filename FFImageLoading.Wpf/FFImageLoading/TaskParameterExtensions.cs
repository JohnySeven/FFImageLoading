using System;
using System.Threading.Tasks;
using FFImageLoading.Cache;
using FFImageLoading.Work;

namespace FFImageLoading
{
	public static class TaskParameterExtensions
	{
		public static async Task InvalidateAsync(this TaskParameter parameters, CacheType cacheType)
		{
			using (IImageLoaderTask task = ImageService.CreateTask(parameters))
			{
				string key = task.Key;
				await ImageService.Instance.InvalidateCacheEntryAsync(key, cacheType).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public static IImageLoaderTask Preload(this TaskParameter parameters)
		{
			if (!parameters.Priority.HasValue)
			{
				parameters.WithPriority(LoadingPriority.Low);
			}
			parameters.Preload = true;
			IImageLoaderTask imageLoaderTask = ImageService.CreateTask(parameters);
			ImageService.Instance.LoadImage(imageLoaderTask);
			return imageLoaderTask;
		}

		public static Task PreloadAsync(this TaskParameter parameters)
		{
			TaskCompletionSource<IScheduledWork> tcs = new TaskCompletionSource<IScheduledWork>();
			if (!parameters.Priority.HasValue)
			{
				parameters.WithPriority(LoadingPriority.Low);
			}
			parameters.Preload = true;
			IImageLoaderTask task = ImageService.CreateTask(parameters);
			Action<Exception> userErrorCallback = parameters.OnError;
			Action<IScheduledWork> finishCallback = parameters.OnFinish;
			parameters.Error(delegate(Exception ex)
			{
				tcs.TrySetException(ex);
				userErrorCallback?.Invoke(ex);
			}).Finish(delegate(IScheduledWork scheduledWork)
			{
				finishCallback?.Invoke(scheduledWork);
				tcs.TrySetResult(scheduledWork);
			});
			ImageService.Instance.LoadImage(task);
			return tcs.Task;
		}

		public static IImageLoaderTask DownloadOnly(this TaskParameter parameters)
		{
			if (parameters.Source == ImageSource.Url)
			{
				return parameters.WithCache(CacheType.Disk).Preload();
			}
			return null;
		}

		public static async Task DownloadOnlyAsync(this TaskParameter parameters)
		{
			if (parameters.Source == ImageSource.Url)
			{
				await parameters.WithCache(CacheType.Disk).PreloadAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}
}
