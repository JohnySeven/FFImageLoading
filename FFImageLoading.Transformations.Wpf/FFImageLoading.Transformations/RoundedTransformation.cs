using System;
using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class RoundedTransformation : TransformationBase
	{
		private enum Corner
		{
			TopLeftCorner,
			TopRightCorner,
			BottomRightCorner,
			BottomLeftCorner
		}

		public double Radius
		{
			get;
			set;
		}

		public double CropWidthRatio
		{
			get;
			set;
		}

		public double CropHeightRatio
		{
			get;
			set;
		}

		public double BorderSize
		{
			get;
			set;
		}

		public string BorderHexColor
		{
			get;
			set;
		}

		public override string Key => $"RoundedTransformation,radius={Radius},cropWidthRatio={CropWidthRatio},cropHeightRatio={CropHeightRatio},borderSize={BorderSize},borderHexColor={BorderHexColor}";

		public RoundedTransformation()
			: this(30.0)
		{
		}

		public RoundedTransformation(double radius)
			: this(radius, 1.0, 1.0)
		{
		}

		public RoundedTransformation(double radius, double cropWidthRatio, double cropHeightRatio)
			: this(radius, cropWidthRatio, cropHeightRatio, 0.0, null)
		{
		}

		public RoundedTransformation(double radius, double cropWidthRatio, double cropHeightRatio, double borderSize, string borderHexColor)
		{
			Radius = radius;
			CropWidthRatio = cropWidthRatio;
			CropHeightRatio = cropHeightRatio;
			BorderSize = borderSize;
			BorderHexColor = borderHexColor;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return ToRounded(bitmapSource, (int)Radius, CropWidthRatio, CropHeightRatio, BorderSize, BorderHexColor);
		}

		public static BitmapHolder ToRounded(BitmapHolder source, int rad, double cropWidthRatio, double cropHeightRatio, double borderSize, string borderHexColor)
		{
			double num = source.Width;
			double num2 = source.Height;
			double num3 = num;
			double num4 = num2;
			double num5 = cropWidthRatio / cropHeightRatio;
			double num6 = num / num2;
			if (num6 > num5)
			{
				num3 = cropWidthRatio * num2 / cropHeightRatio;
			}
			else if (num6 < num5)
			{
				num4 = cropHeightRatio * num / cropWidthRatio;
			}
			double num7 = (num - num3) / 2.0;
			double num8 = (num2 - num4) / 2.0;
			BitmapHolder bitmapHolder = null;
			bitmapHolder = ((num7 == 0.0 && num8 == 0.0) ? new BitmapHolder(source.PixelData, source.Width, source.Height) : CropTransformation.ToCropped(source, (int)num7, (int)num8, (int)num3, (int)num4));
			rad = ((rad != 0) ? ((int)((double)rad * (num3 + num4) / 2.0 / 500.0)) : ((int)(Math.Min(num3, num4) / 2.0)));
			int num9 = (int)num3;
			int num10 = (int)num4;
			ColorHolder transparent = ColorHolder.Transparent;
			for (int i = 0; i < num10; i++)
			{
				for (int j = 0; j < num9; j++)
				{
					if (j <= rad && i <= rad)
					{
						if (!CheckRoundedCorner(rad, rad, rad, Corner.TopLeftCorner, j, i))
						{
							bitmapHolder.SetPixel(i * num9 + j, transparent);
						}
					}
					else if (j >= num9 - rad && i <= rad)
					{
						if (!CheckRoundedCorner(num9 - rad, rad, rad, Corner.TopRightCorner, j, i))
						{
							bitmapHolder.SetPixel(i * num9 + j, transparent);
						}
					}
					else if (j >= num9 - rad && i >= num10 - rad)
					{
						if (!CheckRoundedCorner(num9 - rad, num10 - rad, rad, Corner.BottomRightCorner, j, i))
						{
							bitmapHolder.SetPixel(i * num9 + j, transparent);
						}
					}
					else if (j <= rad && i >= num10 - rad && !CheckRoundedCorner(rad, num10 - rad, rad, Corner.BottomLeftCorner, j, i))
					{
						bitmapHolder.SetPixel(i * num9 + j, transparent);
					}
				}
			}
			return bitmapHolder;
		}

		private static bool CheckRoundedCorner(int h, int k, int r, Corner which, int xC, int yC)
		{
			int num = 0;
			int num2 = r;
			int num3 = 3 - 2 * r;
			do
			{
				switch (which)
				{
				case Corner.TopLeftCorner:
					if (xC <= h - num && yC <= k - num2)
					{
						return false;
					}
					if (xC <= h - num2 && yC <= k - num)
					{
						return false;
					}
					break;
				case Corner.TopRightCorner:
					if (xC >= h + num2 && yC <= k - num)
					{
						return false;
					}
					if (xC >= h + num && yC <= k - num2)
					{
						return false;
					}
					break;
				case Corner.BottomRightCorner:
					if (xC >= h + num && yC >= k + num2)
					{
						return false;
					}
					if (xC >= h + num2 && yC >= k + num)
					{
						return false;
					}
					break;
				case Corner.BottomLeftCorner:
					if (xC <= h - num2 && yC >= k + num)
					{
						return false;
					}
					if (xC <= h - num && yC >= k + num2)
					{
						return false;
					}
					break;
				}
				num++;
				if (num3 < 0)
				{
					num3 += 4 * num + 6;
					continue;
				}
				num2--;
				num3 += 4 * (num - num2) + 10;
			}
			while (num <= num2);
			return true;
		}

		private static void SetPixel4(BitmapHolder bitmap, int centerX, int centerY, int deltaX, int deltaY, ColorHolder color)
		{
			if (centerX + deltaX < bitmap.Width && centerY + deltaY < bitmap.Height)
			{
				bitmap.SetPixel(centerX + deltaX, centerY + deltaY, color);
			}
			if (centerX - deltaX >= 0 && centerY + deltaY < bitmap.Height)
			{
				bitmap.SetPixel(centerX - deltaX, centerY + deltaY, color);
			}
			if (centerX + deltaX < bitmap.Width && centerY - deltaY >= 0)
			{
				bitmap.SetPixel(centerX + deltaX, centerY - deltaY, color);
			}
			if (centerX - deltaX >= 0 && centerY - deltaY >= 0)
			{
				bitmap.SetPixel(centerX - deltaX, centerY - deltaY, color);
			}
		}

		private static void CircleAA(BitmapHolder bitmap, int size, ColorHolder color)
		{
			int centerX = bitmap.Width / 2;
			double num = (bitmap.Width - size) / 2;
			int centerY = bitmap.Height / 2;
			double num2 = (bitmap.Height - size) / 2;
			double num3 = num * num;
			double num4 = num2 * num2;
			int num5 = (int)Math.Round(num3 / Math.Sqrt(num3 + num4));
			for (int i = 0; i <= num5; i++)
			{
				double num6 = Math.Floor(num2 * Math.Sqrt(1.0 - (double)(i * i) / num3));
				double num7 = num6 - Math.Floor(num6);
				int num8 = (int)Math.Round(num7 * 255.0);
				SetPixel4(bitmap, centerX, centerY, i, (int)Math.Floor(num6), new ColorHolder(num8, color.R, color.G, color.B));
				SetPixel4(bitmap, centerX, centerY, i, (int)Math.Floor(num6) + 1, new ColorHolder(255 - num8, color.R, color.G, color.B));
			}
			num5 = (int)Math.Round(num4 / Math.Sqrt(num3 + num4));
			for (int j = 0; j <= num5; j++)
			{
				double num9 = Math.Floor(num * Math.Sqrt(1.0 - (double)(j * j) / num4));
				double num10 = num9 - Math.Floor(num9);
				int num11 = (int)Math.Round(num10 * 255.0);
				SetPixel4(bitmap, centerX, centerY, (int)Math.Floor(num9), j, new ColorHolder(num11, color.R, color.G, color.B));
				SetPixel4(bitmap, centerX, centerY, (int)Math.Floor(num9) + 1, j, new ColorHolder(255 - num11, color.R, color.G, color.B));
			}
		}
	}
}
