using System;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class SepiaTransformation : TransformationBase
	{
		public override string Key => "SepiaTransformation";

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			ToSepia(bitmapSource);
			return bitmapSource;
		}

		public static void ToSepia(BitmapHolder bmp)
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
				int num = (int)Math.Min(0.393 * (double)r + 0.769 * (double)g + 0.189 * (double)b, 255.0);
				int num2 = (int)Math.Min(0.349 * (double)r + 0.686 * (double)g + 0.168 * (double)b, 255.0);
				int num3 = (int)Math.Min(0.272 * (double)r + 0.534 * (double)g + 0.131 * (double)b, 255.0);
				if (num > 255)
				{
					num = 255;
				}
				if (num2 > 255)
				{
					num2 = 255;
				}
				if (num3 > 255)
				{
					num3 = 255;
				}
				bmp.SetPixel(i, new ColorHolder(a, num, num2, num3));
			}
		}
	}
}
