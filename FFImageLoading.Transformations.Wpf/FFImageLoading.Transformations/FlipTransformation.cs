using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class FlipTransformation : TransformationBase
	{
		public override string Key => $"FlipTransformation,Type={FlipType}";

		public FlipType FlipType
		{
			get;
			set;
		}

		public FlipTransformation()
			: this(FlipType.Horizontal)
		{
		}

		public FlipTransformation(FlipType flipType)
		{
			FlipType = flipType;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return ToFlipped(bitmapSource, FlipType);
		}

		public static BitmapHolder ToFlipped(BitmapHolder bmp, FlipType flipMode)
		{
			int width = bmp.Width;
			int height = bmp.Height;
			int num = 0;
			BitmapHolder bitmapHolder = new BitmapHolder(new byte[bmp.PixelData.Length], width, height);
			if (flipMode == FlipType.Vertical)
			{
				byte[] pixelData = bitmapHolder.PixelData;
				for (int num2 = height - 1; num2 >= 0; num2--)
				{
					for (int i = 0; i < width; i++)
					{
						int pos = num2 * width + i;
						bitmapHolder.SetPixel(num, bmp.GetPixel(pos));
						num++;
					}
				}
			}
			else
			{
				byte[] pixelData2 = bitmapHolder.PixelData;
				for (int j = 0; j < height; j++)
				{
					for (int num3 = width - 1; num3 >= 0; num3--)
					{
						int pos2 = j * width + num3;
						bitmapHolder.SetPixel(num, bmp.GetPixel(pos2));
						num++;
					}
				}
			}
			return bitmapHolder;
		}
	}
}
