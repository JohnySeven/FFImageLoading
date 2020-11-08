using System.Windows.Media.Imaging;

namespace FFImageLoading.Work
{
	public class BitmapHolder : IBitmap
	{
		public bool HasWriteableBitmap => WriteableBitmap != null;

		public WriteableBitmap WriteableBitmap
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public byte[] PixelData
		{
			get;
			private set;
		}

		public int PixelCount => PixelData.Length / 4;

		public BitmapHolder(WriteableBitmap bitmap)
		{
			WriteableBitmap = bitmap;
			Width = bitmap.PixelWidth;
			Height = bitmap.PixelHeight;
		}

		public BitmapHolder(byte[] pixels, int width, int height)
		{
			PixelData = pixels;
			Width = width;
			Height = height;
		}

		public void SetPixel(int x, int y, ColorHolder color)
		{
			int pos = y * Width + x;
			SetPixel(pos, color);
		}

		public void SetPixel(int pos, ColorHolder color)
		{
			int num = pos * 4;
			PixelData[num] = color.B;
			PixelData[num + 1] = color.G;
			PixelData[num + 2] = color.R;
			PixelData[num + 3] = color.A;
		}

		public ColorHolder GetPixel(int pos)
		{
			int num = pos * 4;
			byte b = PixelData[num];
			byte g = PixelData[num + 1];
			byte r = PixelData[num + 2];
			byte a = PixelData[num + 3];
			return new ColorHolder(a, r, g, b);
		}

		public ColorHolder GetPixel(int x, int y)
		{
			int pos = y * Width + x;
			return GetPixel(pos);
		}

		public void FreePixels()
		{
			PixelData = null;
			WriteableBitmap = null;
		}
	}
}
