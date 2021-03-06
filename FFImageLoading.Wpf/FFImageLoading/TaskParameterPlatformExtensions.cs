using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FFImageLoading.Extensions;
using FFImageLoading.Targets;
using FFImageLoading.Work;
using FFImageLoading.Wpf;

namespace FFImageLoading
{
	public static class TaskParameterPlatformExtensions
	{
		public static async Task<Stream> AsPNGStreamAsync(this TaskParameter parameters)
		{
			return await (await parameters.AsWriteableBitmapAsync().ConfigureAwait(continueOnCapturedContext: false)).AsPngStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public static async Task<Stream> AsJPGStreamAsync(this TaskParameter parameters, int quality = 80)
		{
			return await (await parameters.AsWriteableBitmapAsync().ConfigureAwait(continueOnCapturedContext: false)).AsJpegStreamAsync(quality).ConfigureAwait(continueOnCapturedContext: false);
		}

		public static Task<WriteableBitmap> AsWriteableBitmapAsync(this TaskParameter parameters)
		{
			BitmapTarget target = new BitmapTarget();
			Action<Exception> userErrorCallback = parameters.OnError;
			Action<IScheduledWork> finishCallback = parameters.OnFinish;
			TaskCompletionSource<WriteableBitmap> tcs = new TaskCompletionSource<WriteableBitmap>();
			parameters.Error(delegate(Exception ex)
			{
				tcs.TrySetException(ex);
				userErrorCallback?.Invoke(ex);
			}).Finish(delegate(IScheduledWork scheduledWork)
			{
				finishCallback?.Invoke(scheduledWork);
				tcs.TrySetResult(target.BitmapSource as WriteableBitmap);
			});
			if (parameters.Source != ImageSource.Stream && string.IsNullOrWhiteSpace(parameters.Path))
			{
				target.SetAsEmpty(null);
				parameters.TryDispose();
				return null;
			}
			IImageLoaderTask task = ImageService.CreateTask(parameters, target);
			ImageService.Instance.LoadImage(task);
			return tcs.Task;
		}

		public static IScheduledWork Into(this TaskParameter parameters, Image imageView)
		{
			ImageTarget target = new ImageTarget(imageView);
			return parameters.Into(target);
		}

		public static Task<IScheduledWork> IntoAsync(this TaskParameter parameters, Image imageView)
		{
			return parameters.IntoAsync(delegate(TaskParameter param)
			{
				param.Into(imageView);
			});
		}

		public static IScheduledWork Into<TImageView>(this TaskParameter parameters, ITarget<BitmapSource, TImageView> target) where TImageView : class
		{
			if (parameters.Source != ImageSource.Stream && string.IsNullOrWhiteSpace(parameters.Path))
			{
				target.SetAsEmpty(null);
				parameters.TryDispose();
				return null;
			}
			IImageLoaderTask imageLoaderTask = ImageService.CreateTask(parameters, target);
			ImageService.Instance.LoadImage(imageLoaderTask);
			return imageLoaderTask;
		}

		public static Task<IScheduledWork> IntoAsync<TImageView>(this TaskParameter parameters, ITarget<BitmapSource, TImageView> target) where TImageView : class
		{
			return parameters.IntoAsync(delegate(TaskParameter param)
			{
				param.Into(target);
			});
		}

		private static Task<IScheduledWork> IntoAsync(this TaskParameter parameters, Action<TaskParameter> into)
		{
			Action<Exception> userErrorCallback = parameters.OnError;
			Action<IScheduledWork> finishCallback = parameters.OnFinish;
			TaskCompletionSource<IScheduledWork> tcs = new TaskCompletionSource<IScheduledWork>();
			parameters.Error(delegate(Exception ex)
			{
				tcs.TrySetException(ex);
				userErrorCallback?.Invoke(ex);
			}).Finish(delegate(IScheduledWork scheduledWork)
			{
				finishCallback?.Invoke(scheduledWork);
				tcs.TrySetResult(scheduledWork);
			});
			into(parameters);
			return tcs.Task;
		}
	}
}
