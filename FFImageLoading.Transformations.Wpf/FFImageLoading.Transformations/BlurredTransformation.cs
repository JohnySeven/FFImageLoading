using System;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class BlurredTransformation : TransformationBase
	{
		public double Radius
		{
			get;
			set;
		}

		public override string Key => $"BlurredTransformation,radius={Radius}";

		public BlurredTransformation()
		{
			Radius = 20.0;
		}

		public BlurredTransformation(double radius)
		{
			Radius = radius;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			ToLegacyBlurred(bitmapSource, (int)Radius);
			return bitmapSource;
		}

		public static void ToLegacyBlurred(BitmapHolder source, int radius)
		{
			int width = source.Width;
			int height = source.Height;
			int num = width - 1;
			int val = height - 1;
			int num2 = width * height;
			int num3 = radius + radius + 1;
			int[] array = new int[num2];
			int[] array2 = new int[num2];
			int[] array3 = new int[num2];
			int[] array4 = new int[Math.Max(width, height)];
			int[] array5 = new int[Math.Max(width, height)];
			int[] array6 = new int[256 * num3];
			for (int i = 0; i < 256 * num3; i++)
			{
				array6[i] = i / num3;
			}
			int num4;
			int num5 = (num4 = 0);
			for (int j = 0; j < height; j++)
			{
				int num7;
				int num6;
				int num8 = (num7 = (num6 = 0));
				for (int i = -radius; i <= radius; i++)
				{
					ColorHolder pixel = source.GetPixel(num4 + Math.Min(num, Math.Max(i, 0)));
					num8 += pixel.R;
					num7 += pixel.G;
					num6 += pixel.B;
				}
				for (int k = 0; k < width; k++)
				{
					array[num4] = array6[num8];
					array2[num4] = array6[num7];
					array3[num4] = array6[num6];
					if (j == 0)
					{
						array4[k] = Math.Min(k + radius + 1, num);
						array5[k] = Math.Max(k - radius, 0);
					}
					ColorHolder pixel2 = source.GetPixel(num5 + array4[k]);
					ColorHolder pixel3 = source.GetPixel(num5 + array5[k]);
					num8 += pixel2.R - pixel3.R;
					num7 += pixel2.G - pixel3.G;
					num6 += pixel2.B - pixel3.B;
					num4++;
				}
				num5 += width;
			}
			for (int k = 0; k < width; k++)
			{
				int num7;
				int num6;
				int num8 = (num7 = (num6 = 0));
				int num9 = -radius * width;
				for (int i = -radius; i <= radius; i++)
				{
					num4 = Math.Max(0, num9) + k;
					num8 += array[num4];
					num7 += array2[num4];
					num6 += array3[num4];
					num9 += width;
				}
				num4 = k;
				for (int j = 0; j < height; j++)
				{
					ColorHolder color = new ColorHolder(source.GetPixel(num4).A, array6[num8], array6[num7], array6[num6]);
					source.SetPixel(num4, color);
					if (k == 0)
					{
						array4[j] = Math.Min(j + radius + 1, val) * width;
						array5[j] = Math.Max(j - radius, 0) * width;
					}
					int num10 = k + array4[j];
					int num11 = k + array5[j];
					num8 += array[num10] - array[num11];
					num7 += array2[num10] - array2[num11];
					num6 += array3[num10] - array3[num11];
					num4 += width;
				}
			}
		}
	}
}
