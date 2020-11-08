using System;
using FFImageLoading.Extensions;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class TintTransformation : ColorSpaceTransformation
	{
		private string _hexColor;

		public string HexColor
		{
			get
			{
				return _hexColor;
			}
			set
			{
				_hexColor = value;
				ColorHolder colorHolder = value.ToColorFromHex();
				A = colorHolder.A;
				R = colorHolder.R;
				G = colorHolder.G;
				B = colorHolder.B;
			}
		}

		public bool EnableSolidColor
		{
			get;
			set;
		}

		public int R
		{
			get;
			set;
		}

		public int G
		{
			get;
			set;
		}

		public int B
		{
			get;
			set;
		}

		public int A
		{
			get;
			set;
		}

		public override string Key => $"TintTransformation,R={R},G={G},B={B},A={A},HexColor={HexColor},EnableSolidColor={EnableSolidColor}";

		public TintTransformation()
			: this(0, 165, 0, 128)
		{
		}

		public TintTransformation(int r, int g, int b, int a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public TintTransformation(string hexColor)
		{
			HexColor = hexColor;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			if (EnableSolidColor)
			{
				ToReplacedColor(bitmapSource, R, G, B, A);
				return bitmapSource;
			}
			base.RGBAWMatrix = FFColorMatrix.ColorToTintMatrix(R, G, B, A);
			return base.Transform(bitmapSource, path, source, isPlaceholder, key);
		}

		public static void ToReplacedColor(BitmapHolder bmp, int r, int g, int b, int a)
		{
			int width = bmp.Width;
			int height = bmp.Height;
			int pixelCount = bmp.PixelCount;
			float num = (float)a / 255f;
			float num2 = 1f - num;
			int val = (int)((float)r - (float)r * num2);
			int val2 = (int)((float)g - (float)g * num2);
			int val3 = (int)((float)b - (float)b * num2);
			int val4 = (int)((float)r + (float)r * num2);
			int val5 = (int)((float)g + (float)g * num2);
			int val6 = (int)((float)b + (float)b * num2);
			for (int i = 0; i < pixelCount; i++)
			{
				ColorHolder pixel = bmp.GetPixel(i);
				int a2 = pixel.A;
				byte r2 = pixel.R;
				byte g2 = pixel.G;
				byte b2 = pixel.B;
				int val7 = (int)((float)(int)r2 + (float)(255 - r2) * (num * (float)r / 255f));
				int val8 = (int)((float)(int)g2 + (float)(255 - g2) * (num * (float)g / 255f));
				int val9 = (int)((float)(int)b2 + (float)(255 - b2) * (num * (float)b / 255f));
				val7 = Math.Min(Math.Max(val, val7), val4);
				val8 = Math.Min(Math.Max(val2, val8), val5);
				val9 = Math.Min(Math.Max(val3, val9), val6);
				bmp.SetPixel(i, new ColorHolder(pixel.A, val7, val8, val9));
			}
		}
	}
}
