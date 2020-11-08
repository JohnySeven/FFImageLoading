using System;

namespace FFImageLoading
{
	public readonly struct ColorHolder
	{
		public readonly byte A;

		public readonly byte R;

		public readonly byte G;

		public readonly byte B;

		public static readonly ColorHolder Transparent = new ColorHolder(0, 0, 0, 0);

		public ColorHolder(int a, int r, int g, int b)
		{
			A = Convert.ToByte(Math.Min(Math.Max(0, a), 255));
			if (a > 0)
			{
				R = Convert.ToByte(Math.Min(Math.Max(0, r), 255));
				G = Convert.ToByte(Math.Min(Math.Max(0, g), 255));
				B = Convert.ToByte(Math.Min(Math.Max(0, b), 255));
			}
			else
			{
				R = 0;
				G = 0;
				B = 0;
			}
		}

		public ColorHolder(int r, int g, int b)
		{
			A = byte.MaxValue;
			R = Convert.ToByte(Math.Min(Math.Max(0, r), 255));
			G = Convert.ToByte(Math.Min(Math.Max(0, g), 255));
			B = Convert.ToByte(Math.Min(Math.Max(0, b), 255));
		}
	}
}
