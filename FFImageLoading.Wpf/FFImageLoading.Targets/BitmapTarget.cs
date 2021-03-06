using System;
using System.Windows.Media.Imaging;
using FFImageLoading.Work;

namespace FFImageLoading.Targets
{
	public class BitmapTarget : Target<BitmapSource, WriteableBitmap>
	{
		private WeakReference<BitmapSource> _imageWeakReference = null;

		public BitmapSource BitmapSource
		{
			get
			{
				if (_imageWeakReference == null)
				{
					return null;
				}
				BitmapSource target = null;
				_imageWeakReference.TryGetTarget(out target);
				return target;
			}
		}

		public override void Set(IImageLoaderTask task, BitmapSource image, bool animated)
		{
			if (!(task?.IsCancelled ?? true))
			{
				if (_imageWeakReference == null)
				{
					_imageWeakReference = new WeakReference<BitmapSource>(image);
				}
				else
				{
					_imageWeakReference.SetTarget(image);
				}
			}
		}
	}
}
