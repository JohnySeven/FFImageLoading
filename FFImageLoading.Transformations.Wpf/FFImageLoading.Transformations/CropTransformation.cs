using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class CropTransformation : TransformationBase
	{
		public double ZoomFactor
		{
			get;
			set;
		}

		public double XOffset
		{
			get;
			set;
		}

		public double YOffset
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

		public override string Key => $"CropTransformation,zoomFactor={ZoomFactor},xOffset={XOffset},yOffset={YOffset},cropWidthRatio={CropWidthRatio},cropHeightRatio={CropHeightRatio}";

		public CropTransformation()
			: this(1.0, 0.0, 0.0)
		{
		}

		public CropTransformation(double zoomFactor, double xOffset, double yOffset)
			: this(zoomFactor, xOffset, yOffset, 1.0, 1.0)
		{
		}

		public CropTransformation(double zoomFactor, double xOffset, double yOffset, double cropWidthRatio, double cropHeightRatio)
		{
			ZoomFactor = zoomFactor;
			XOffset = xOffset;
			YOffset = yOffset;
			CropWidthRatio = cropWidthRatio;
			CropHeightRatio = cropHeightRatio;
			if (ZoomFactor < 1.0)
			{
				ZoomFactor = 1.0;
			}
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return ToCropped(bitmapSource, ZoomFactor, XOffset, YOffset, CropWidthRatio, CropHeightRatio);
		}

		public static BitmapHolder ToCropped(BitmapHolder source, double zoomFactor, double xOffset, double yOffset, double cropWidthRatio, double cropHeightRatio)
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
			xOffset *= num3;
			yOffset *= num4;
			num3 /= zoomFactor;
			num4 /= zoomFactor;
			float num7 = (float)((num - num3) / 2.0 + xOffset);
			float num8 = (float)((num2 - num4) / 2.0 + yOffset);
			if (num7 < 0f)
			{
				num7 = 0f;
			}
			if (num8 < 0f)
			{
				num8 = 0f;
			}
			if ((double)num7 + num3 > num)
			{
				num7 = (float)(num - num3);
			}
			if ((double)num8 + num4 > num2)
			{
				num8 = (float)(num2 - num4);
			}
			int num9 = (int)num3;
			int num10 = (int)num4;
			byte[] array = new byte[num9 * num10 * 4];
			for (int i = 0; i < num10; i++)
			{
				int srcOffset = (((int)num8 + i) * source.Width + (int)num7) * 4;
				int destOffset = i * num9 * 4;
				Helpers.BlockCopy(source.PixelData, srcOffset, array, destOffset, num9 * 4);
			}
			return new BitmapHolder(array, num9, num10);
		}

		public static BitmapHolder ToCropped(BitmapHolder source, int x, int y, int width, int height)
		{
			int width2 = source.Width;
			int height2 = source.Height;
			if (x < 0)
			{
				x = 0;
			}
			if (x + width > width2)
			{
				width = width2 - x;
			}
			if (y < 0)
			{
				y = 0;
			}
			if (y + height > height2)
			{
				height = height2 - y;
			}
			byte[] array = new byte[width * height * 4];
			for (int i = 0; i < height; i++)
			{
				int srcOffset = ((y + i) * width2 + x) * 4;
				int destOffset = i * width * 4;
				Helpers.BlockCopy(source.PixelData, srcOffset, array, destOffset, width * 4);
			}
			return new BitmapHolder(array, width, height);
		}
	}
}
