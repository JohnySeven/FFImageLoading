using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FFImageLoading.Cache;
using FFImageLoading.Config;
using FFImageLoading.DataResolvers;
using FFImageLoading.Helpers;
using FFImageLoading.Work;

namespace FFImageLoading
{
	[Preserve(AllMembers = true)]
	public class ImageService : ImageServiceBase<BitmapSource>
	{
		private static ConditionalWeakTable<object, IImageLoaderTask> _viewsReferences = new ConditionalWeakTable<object, IImageLoaderTask>();

		private static IImageService _instance;

		public static IImageService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ImageService();
				}
				return _instance;
			}
		}

		public static bool EnableMockImageService
		{
			get;
			set;
		}

		protected override IMemoryCache<BitmapSource> MemoryCache => ImageCache.Instance;

		protected override void PlatformSpecificConfiguration(Configuration configuration)
		{
			base.PlatformSpecificConfiguration(configuration);
			configuration.ClearMemoryCacheOnOutOfMemory = false;
			configuration.ExecuteCallbacksOnUIThread = true;
		}

		protected override IMD5Helper CreatePlatformMD5HelperInstance(Configuration configuration)
		{
			return new MD5Helper();
		}

		protected override IMiniLogger CreatePlatformLoggerInstance(Configuration configuration)
		{
			return new MiniLogger();
		}

		protected override IPlatformPerformance CreatePlatformPerformanceInstance(Configuration configuration)
		{
			return new PlatformPerformance();
		}

		protected override IMainThreadDispatcher CreateMainThreadDispatcherInstance(Configuration configuration)
		{
			return new MainThreadDispatcher();
		}

		protected override IDataResolverFactory CreateDataResolverFactoryInstance(Configuration configuration)
		{
			return new DataResolverFactory();
		}

		protected override IDiskCache CreatePlatformDiskCacheInstance(Configuration configuration)
		{
			DirectoryInfo directoryInfo = null;
			string text = null;
			if (string.IsNullOrWhiteSpace(configuration.DiskCachePath))
			{
				throw new ArgumentException("Configuration property DiskCachePath must be specified!");
			}
			string[] source = configuration.DiskCachePath.Split(new char[2]
			{
				'/',
				'\\'
			}, StringSplitOptions.RemoveEmptyEntries);
			text = source.Last();
			string path = configuration.DiskCachePath.Substring(0, configuration.DiskCachePath.LastIndexOf(text));
			directoryInfo = new DirectoryInfo(path);
			return new SimpleDiskCache(directoryInfo, text, base.Config);
		}

		internal static IImageLoaderTask CreateTask<TImageView>(TaskParameter parameters, ITarget<BitmapSource, TImageView> target) where TImageView : class
		{
			return new PlatformImageLoaderTask<TImageView>(target, parameters, Instance);
		}

		internal static IImageLoaderTask CreateTask(TaskParameter parameters)
		{
			return new PlatformImageLoaderTask<object>(null, parameters, Instance);
		}

		protected override void SetTaskForTarget(IImageLoaderTask currentTask)
		{
			object obj = currentTask?.Target?.TargetControl;
			if (!(obj is Image))
			{
				return;
			}
			lock (_viewsReferences)
			{
				if (_viewsReferences.TryGetValue(obj, out var value))
				{
					try
					{
						if (value != null && !value.IsCancelled && !value.IsCompleted)
						{
							value.Cancel();
						}
					}
					catch (ObjectDisposedException)
					{
					}
					_viewsReferences.Remove(obj);
				}
				_viewsReferences.Add(obj, currentTask);
			}
		}

		public override void CancelWorkForView(object view)
		{
			lock (_viewsReferences)
			{
				if (!_viewsReferences.TryGetValue(view, out var value))
				{
					return;
				}
				try
				{
					if (value != null && !value.IsCancelled && !value.IsCompleted)
					{
						value.Cancel();
					}
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}

		public override int DpToPixels(double dp)
		{
			return (int)Math.Floor(dp * ScaleHelper.Scale);
		}

		public override double PixelsToDp(double px)
		{
			if (Math.Abs(px) < double.Epsilon)
			{
				return 0.0;
			}
			return Math.Floor(px / ScaleHelper.Scale);
		}
	}
}
