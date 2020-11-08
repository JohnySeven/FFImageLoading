using System;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class RotateTransformation : TransformationBase
	{
		public double Degrees
		{
			get;
			set;
		}

		public bool CCW
		{
			get;
			set;
		}

		public bool Resize
		{
			get;
			set;
		}

		public override string Key => $"RotateTransformation,degrees={Degrees},ccw={CCW},resize={Resize}";

		public RotateTransformation()
			: this(30.0)
		{
		}

		public RotateTransformation(double degrees)
			: this(degrees, ccw: false, resize: false)
		{
		}

		public RotateTransformation(double degrees, bool ccw)
			: this(degrees, ccw, resize: false)
		{
		}

		public RotateTransformation(double degrees, bool ccw, bool resize)
		{
			Degrees = degrees;
			CCW = ccw;
			Resize = resize;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return ToRotated(bitmapSource, Degrees, CCW, Resize);
		}

		public static BitmapHolder ToRotated(BitmapHolder source, double degrees, bool ccw, bool resize)
		{
			if (degrees == 0.0 || degrees % 360.0 == 0.0)
			{
				return source;
			}
			if (ccw)
			{
				degrees = 360.0 - degrees;
			}
			double num = -Math.PI / 180.0 * degrees;
			int width = source.Width;
			int height = source.Height;
			int num2;
			int num3;
			if (!resize || degrees % 180.0 == 0.0)
			{
				num2 = width;
				num3 = height;
			}
			else
			{
				double num4 = degrees / (180.0 / Math.PI);
				num2 = (int)Math.Ceiling(Math.Abs(Math.Sin(num4) * (double)height) + Math.Abs(Math.Cos(num4) * (double)width));
				num3 = (int)Math.Ceiling(Math.Abs(Math.Sin(num4) * (double)width) + Math.Abs(Math.Cos(num4) * (double)height));
			}
			int num5 = width / 2;
			int num6 = height / 2;
			int num7 = num2 / 2;
			int num8 = num3 / 2;
			BitmapHolder bitmapHolder = new BitmapHolder(new byte[num2 * num3 * 4], num2, num3);
			int width2 = source.Width;
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num9 = j - num7;
					int num10 = num8 - i;
					double num11 = Math.Sqrt(num9 * num9 + num10 * num10);
					double num12;
					if (num9 == 0)
					{
						if (num10 == 0)
						{
							bitmapHolder.SetPixel(i * num2 + j, source.GetPixel(num6 * width2 + num5));
							continue;
						}
						num12 = ((num10 >= 0) ? (Math.PI / 2.0) : 4.71238898038469);
					}
					else
					{
						num12 = Math.Atan2(num10, num9);
					}
					num12 -= num;
					double num13 = num11 * Math.Cos(num12);
					double num14 = num11 * Math.Sin(num12);
					num13 += (double)num5;
					num14 = (double)num6 - num14;
					int num15 = (int)Math.Floor(num13);
					int num16 = (int)Math.Floor(num14);
					int num17 = (int)Math.Ceiling(num13);
					int num18 = (int)Math.Ceiling(num14);
					if (num15 >= 0 && num17 >= 0 && num15 < width && num17 < width && num16 >= 0 && num18 >= 0 && num16 < height && num18 < height)
					{
						double num19 = num13 - (double)num15;
						double num20 = num14 - (double)num16;
						ColorHolder pixel = source.GetPixel(num16 * width2 + num15);
						ColorHolder pixel2 = source.GetPixel(num16 * width2 + num17);
						ColorHolder pixel3 = source.GetPixel(num18 * width2 + num15);
						ColorHolder pixel4 = source.GetPixel(num18 * width2 + num17);
						double num21 = (1.0 - num19) * (double)(int)pixel.A + num19 * (double)(int)pixel2.A;
						double num22 = (1.0 - num19) * (double)(int)pixel.R + num19 * (double)(int)pixel2.R;
						double num23 = (1.0 - num19) * (double)(int)pixel.G + num19 * (double)(int)pixel2.G;
						double num24 = (1.0 - num19) * (double)(int)pixel.B + num19 * (double)(int)pixel2.B;
						double num25 = (1.0 - num19) * (double)(int)pixel3.A + num19 * (double)(int)pixel4.A;
						double num26 = (1.0 - num19) * (double)(int)pixel3.R + num19 * (double)(int)pixel4.R;
						double num27 = (1.0 - num19) * (double)(int)pixel3.G + num19 * (double)(int)pixel4.G;
						double num28 = (1.0 - num19) * (double)(int)pixel3.B + num19 * (double)(int)pixel4.B;
						int r = (int)Math.Round((1.0 - num20) * num22 + num20 * num26);
						int g = (int)Math.Round((1.0 - num20) * num23 + num20 * num27);
						int b = (int)Math.Round((1.0 - num20) * num24 + num20 * num28);
						int num29 = (int)Math.Round((1.0 - num20) * num21 + num20 * num25);
						int num30 = num29 + 1;
						bitmapHolder.SetPixel(i * num2 + j, new ColorHolder(num29, r, g, b));
					}
				}
			}
			return bitmapHolder;
		}
	}
}
