using FFImageLoading.Wpf;

namespace FFImageLoading.Extensions
{
	public static class UnitsExtensions
	{
		public static int DpToPixels(this int dp)
		{
			return ImageService.Instance.DpToPixels(dp);
		}

		public static int DpToPixels(this double dp)
		{
			return ImageService.Instance.DpToPixels(dp);
		}

		public static double PixelsToDp(this int px)
		{
			return ImageService.Instance.PixelsToDp(px);
		}

		public static double PixelsToDp(this double px)
		{
			return ImageService.Instance.PixelsToDp(px);
		}
	}
}
