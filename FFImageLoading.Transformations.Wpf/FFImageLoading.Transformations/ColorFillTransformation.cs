using FFImageLoading.Extensions;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class ColorFillTransformation : TransformationBase
	{
		public string HexColor
		{
			get;
			set;
		}

		public override string Key => $"ColorFillTransformation,hexColor={HexColor}";

		public ColorFillTransformation()
			: this("#000000")
		{
		}

		public ColorFillTransformation(string hexColor)
		{
			HexColor = hexColor;
		}

		public static ColorHolder BlendColor(ColorHolder color, ColorHolder backColor)
		{
			float num = (float)(int)color.A / 255f;
			byte r = (byte)((float)(int)color.R * num + (float)(int)backColor.R * (1f - num));
			byte g = (byte)((float)(int)color.G * num + (float)(int)backColor.G * (1f - num));
			byte b = (byte)((float)(int)color.B * num + (float)(int)backColor.B * (1f - num));
			return new ColorHolder(r, g, b);
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			int pixelCount = bitmapSource.PixelCount;
			ColorHolder backColor = HexColor.ToColorFromHex();
			for (int i = 0; i < pixelCount; i++)
			{
				ColorHolder pixel = bitmapSource.GetPixel(i);
				bitmapSource.SetPixel(i, BlendColor(pixel, backColor));
			}
			return bitmapSource;
		}
	}
}
