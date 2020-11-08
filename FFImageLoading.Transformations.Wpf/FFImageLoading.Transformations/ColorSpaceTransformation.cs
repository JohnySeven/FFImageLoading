using System;
using System.Linq;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class ColorSpaceTransformation : TransformationBase
	{
		private float[][] _rgbawMatrix;

		public float[][] RGBAWMatrix
		{
			get
			{
				return _rgbawMatrix;
			}
			set
			{
				if (value.Length != 5 || value.Any((float[] v) => v.Length != 5))
				{
					throw new ArgumentException("Wrong size of RGBAW color matrix");
				}
				_rgbawMatrix = value;
			}
		}

		public override string Key => string.Format("ColorSpaceTransformation,rgbawMatrix={0}", string.Join(",", _rgbawMatrix.Select((float[] x) => string.Join(",", x)).ToArray()));

		public ColorSpaceTransformation()
			: this(FFColorMatrix.InvertColorMatrix)
		{
		}

		public ColorSpaceTransformation(float[][] rgbawMatrix)
		{
			if (rgbawMatrix.Length != 5 || rgbawMatrix.Any((float[] v) => v.Length != 5))
			{
				throw new ArgumentException("Wrong size of RGBAW color matrix");
			}
			RGBAWMatrix = rgbawMatrix;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			ToColorSpace(bitmapSource, _rgbawMatrix);
			return bitmapSource;
		}

		public static void ToColorSpace(BitmapHolder bmp, float[][] rgbawMatrix)
		{
			float num = rgbawMatrix[0][0];
			float num2 = rgbawMatrix[0][1];
			float num3 = rgbawMatrix[0][2];
			float num4 = rgbawMatrix[0][3];
			float num5 = rgbawMatrix[1][0];
			float num6 = rgbawMatrix[1][1];
			float num7 = rgbawMatrix[1][2];
			float num8 = rgbawMatrix[1][3];
			float num9 = rgbawMatrix[2][0];
			float num10 = rgbawMatrix[2][1];
			float num11 = rgbawMatrix[2][2];
			float num12 = rgbawMatrix[2][3];
			float num13 = rgbawMatrix[3][0];
			float num14 = rgbawMatrix[3][1];
			float num15 = rgbawMatrix[3][2];
			float num16 = rgbawMatrix[3][3];
			float num17 = rgbawMatrix[4][0];
			float num18 = rgbawMatrix[4][1];
			float num19 = rgbawMatrix[4][2];
			float num20 = rgbawMatrix[4][3];
			int width = bmp.Width;
			int height = bmp.Height;
			int pixelCount = bmp.PixelCount;
			for (int i = 0; i < pixelCount; i++)
			{
				ColorHolder pixel = bmp.GetPixel(i);
				byte a = pixel.A;
				byte r = pixel.R;
				byte g = pixel.G;
				byte b = pixel.B;
				int r2 = (int)((float)(int)r * num + (float)(int)g * num5 + (float)(int)b * num9 + (float)(int)a * num13 + num17);
				int g2 = (int)((float)(int)r * num2 + (float)(int)g * num6 + (float)(int)b * num10 + (float)(int)a * num14 + num18);
				int b2 = (int)((float)(int)r * num3 + (float)(int)g * num7 + (float)(int)b * num11 + (float)(int)a * num15 + num19);
				int a2 = (int)((float)(int)r * num4 + (float)(int)g * num8 + (float)(int)b * num12 + (float)(int)a * num16 + num20);
				bmp.SetPixel(i, new ColorHolder(a2, r2, g2, b2));
			}
		}
	}
}
