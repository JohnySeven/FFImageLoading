using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class GrayscaleTransformation : TransformationBase
	{
		public override string Key => "GrayscaleTransformation";

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			ToGrayscale(bitmapSource);
			return bitmapSource;
		}

		public static void ToGrayscale(BitmapHolder bmp)
		{
			int width = bmp.Width;
			int height = bmp.Height;
			int pixelCount = bmp.PixelCount;
			for (int i = 0; i < pixelCount; i++)
			{
				ColorHolder pixel = bmp.GetPixel(i);
				int a = pixel.A;
				int r = pixel.R;
				int g = pixel.G;
				int b = pixel.B;
				int num = r * 6966 + g * 23436 + b * 2366 >> 15;
				r = (g = (b = num));
				bmp.SetPixel(i, new ColorHolder(a, r, g, b));
			}
		}
	}
}
