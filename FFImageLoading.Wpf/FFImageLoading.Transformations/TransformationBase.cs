using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public abstract class TransformationBase : ITransformation
	{
		public abstract string Key
		{
			get;
		}

		public IBitmap Transform(IBitmap bitmapHolder, string path, ImageSource source, bool isPlaceholder, string key)
		{
			BitmapHolder bitmapHolder2 = bitmapHolder.ToNative();
			return Transform(bitmapHolder2, path, source, isPlaceholder, key);
		}

		protected virtual BitmapHolder Transform(BitmapHolder bitmapHolder, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return bitmapHolder;
		}
	}
}
