using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class CornersTransformation : TransformationBase
	{
		private enum Corner
		{
			TopLeftCorner,
			TopRightCorner,
			BottomRightCorner,
			BottomLeftCorner
		}

		public double TopLeftCornerSize
		{
			get;
			set;
		}

		public double TopRightCornerSize
		{
			get;
			set;
		}

		public double BottomLeftCornerSize
		{
			get;
			set;
		}

		public double BottomRightCornerSize
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

		public CornerTransformType CornersTransformType
		{
			get;
			set;
		}

		public override string Key => $"CornersTransformation,cornersSizes={TopLeftCornerSize},{TopRightCornerSize},{BottomRightCornerSize},{BottomLeftCornerSize},cornersTransformType={CornersTransformType},cropWidthRatio={CropWidthRatio},cropHeightRatio={CropHeightRatio},";

		public CornersTransformation()
			: this(20.0, CornerTransformType.TopRightRounded)
		{
		}

		public CornersTransformation(double cornersSize, CornerTransformType cornersTransformType)
			: this(cornersSize, cornersSize, cornersSize, cornersSize, cornersTransformType, 1.0, 1.0)
		{
		}

		public CornersTransformation(double topLeftCornerSize, double topRightCornerSize, double bottomLeftCornerSize, double bottomRightCornerSize, CornerTransformType cornersTransformType)
			: this(topLeftCornerSize, topRightCornerSize, bottomLeftCornerSize, bottomRightCornerSize, cornersTransformType, 1.0, 1.0)
		{
		}

		public CornersTransformation(double cornersSize, CornerTransformType cornersTransformType, double cropWidthRatio, double cropHeightRatio)
			: this(cornersSize, cornersSize, cornersSize, cornersSize, cornersTransformType, cropWidthRatio, cropHeightRatio)
		{
		}

		public CornersTransformation(double topLeftCornerSize, double topRightCornerSize, double bottomLeftCornerSize, double bottomRightCornerSize, CornerTransformType cornersTransformType, double cropWidthRatio, double cropHeightRatio)
		{
			TopLeftCornerSize = topLeftCornerSize;
			TopRightCornerSize = topRightCornerSize;
			BottomLeftCornerSize = bottomLeftCornerSize;
			BottomRightCornerSize = bottomRightCornerSize;
			CornersTransformType = cornersTransformType;
			CropWidthRatio = cropWidthRatio;
			CropHeightRatio = cropHeightRatio;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return ToTransformedCorners(bitmapSource, TopLeftCornerSize, TopRightCornerSize, BottomLeftCornerSize, BottomRightCornerSize, CornersTransformType, CropWidthRatio, CropHeightRatio);
		}

		public static BitmapHolder ToTransformedCorners(BitmapHolder source, double topLeftCornerSize, double topRightCornerSize, double bottomLeftCornerSize, double bottomRightCornerSize, CornerTransformType cornersTransformType, double cropWidthRatio, double cropHeightRatio)
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
			topLeftCornerSize = topLeftCornerSize * (num3 + num4) / 2.0 / 100.0;
			topRightCornerSize = topRightCornerSize * (num3 + num4) / 2.0 / 100.0;
			bottomLeftCornerSize = bottomLeftCornerSize * (num3 + num4) / 2.0 / 100.0;
			bottomRightCornerSize = bottomRightCornerSize * (num3 + num4) / 2.0 / 100.0;
			int num9 = (int)topLeftCornerSize;
			int num10 = (int)topRightCornerSize;
			int num11 = (int)bottomLeftCornerSize;
			int num12 = (int)bottomRightCornerSize;
			int width = bitmapHolder.Width;
			int height = bitmapHolder.Height;
			ColorHolder transparent = ColorHolder.Transparent;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (j <= num9 && i <= num9)
					{
						if (!CheckCorner(num9, num9, num9, cornersTransformType, Corner.TopLeftCorner, j, i))
						{
							bitmapHolder.SetPixel(i * width + j, transparent);
						}
					}
					else if (j >= width - num10 && i <= num10 && num10 > 0)
					{
						if (!CheckCorner(width - num10, num10, num10, cornersTransformType, Corner.TopRightCorner, j, i))
						{
							bitmapHolder.SetPixel(i * width + j, transparent);
						}
					}
					else if (j >= width - num12 && i >= height - num12 && num12 > 0)
					{
						if (!CheckCorner(width - num12, height - num12, num12, cornersTransformType, Corner.BottomRightCorner, j, i))
						{
							bitmapHolder.SetPixel(i * width + j, transparent);
						}
					}
					else if (j <= num11 && i >= height - num11 && num11 > 0 && !CheckCorner(num11, height - num11, num11, cornersTransformType, Corner.BottomLeftCorner, j, i))
					{
						bitmapHolder.SetPixel(i * width + j, transparent);
					}
				}
			}
			return bitmapHolder;
		}

		private static bool HasFlag(CornerTransformType flags, CornerTransformType flag)
		{
			return (flags & flag) != 0;
		}

		private static bool CheckCorner(int w, int h, int size, CornerTransformType flags, Corner which, int xC, int yC)
		{
			if ((HasFlag(flags, CornerTransformType.TopLeftCut) && which == Corner.TopLeftCorner) || (HasFlag(flags, CornerTransformType.TopRightCut) && which == Corner.TopRightCorner) || (HasFlag(flags, CornerTransformType.BottomRightCut) && which == Corner.BottomRightCorner) || (HasFlag(flags, CornerTransformType.BottomLeftCut) && which == Corner.BottomLeftCorner))
			{
				return CheckCutCorner(w, h, size, which, xC, yC);
			}
			if ((HasFlag(flags, CornerTransformType.TopLeftRounded) && which == Corner.TopLeftCorner) || (HasFlag(flags, CornerTransformType.TopRightRounded) && which == Corner.TopRightCorner) || (HasFlag(flags, CornerTransformType.BottomRightRounded) && which == Corner.BottomRightCorner) || (HasFlag(flags, CornerTransformType.BottomLeftRounded) && which == Corner.BottomLeftCorner))
			{
				return CheckRoundedCorner(w, h, size, which, xC, yC);
			}
			return true;
		}

		private static bool CheckCutCorner(int w, int h, int size, Corner which, int xC, int yC)
		{
			switch(which)
			{
				case Corner.TopLeftCorner:
					return Slope(size, 0.0, xC - 1, yC) < Slope(size, 0.0, 0.0, size);
				case Corner.TopRightCorner:
					return Slope(w, 0.0, xC, yC) > Slope(w, 0.0, w + size, size);
				case Corner.BottomRightCorner:
					return Slope(h + size, h, xC, yC) > Slope(h + size, h, w, h + size);
				case Corner.BottomLeftCorner:
					return Slope(0.0, h, xC, yC) < Slope(0.0, h, size, h + size);
				default:
					return true;
			};
		}

		private static double Slope(double x1, double y1, double x2, double y2)
		{
			return (y2 - y1) / (x2 - x1);
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
	}
}
