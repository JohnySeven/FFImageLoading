namespace FFImageLoading.Work
{
	public static class IBitmapExtensions
	{
		public static BitmapHolder ToNative(this IBitmap bitmap)
		{
			return (BitmapHolder)bitmap;
		}
	}
}
