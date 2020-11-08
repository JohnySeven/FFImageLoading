using System;

namespace FFImageLoading.Extensions
{
	public static class ColorExtensions
	{
		public const int SizeOfArgb = 4;

		public static ColorHolder ToColorFromHex(this string hexColor)
		{
			if (string.IsNullOrWhiteSpace(hexColor))
			{
				throw new ArgumentException("Invalid color string.", "hexColor");
			}
			if (!hexColor.StartsWith("#", StringComparison.Ordinal))
			{
				hexColor.Insert(0, "#");
			}
			switch (hexColor.Length)
			{
			case 9:
			{
				uint num4 = Convert.ToUInt32(hexColor.Substring(1), 16);
				byte a = (byte)(num4 >> 24);
				byte r2 = (byte)((num4 >> 16) & 0xFFu);
				byte g2 = (byte)((num4 >> 8) & 0xFFu);
				byte b9 = (byte)(num4 & 0xFFu);
				return new ColorHolder(a, r2, g2, b9);
			}
			case 7:
			{
				uint num3 = Convert.ToUInt32(hexColor.Substring(1), 16);
				byte r = (byte)((num3 >> 16) & 0xFFu);
				byte g = (byte)((num3 >> 8) & 0xFFu);
				byte b8 = (byte)(num3 & 0xFFu);
				return new ColorHolder(255, r, g, b8);
			}
			case 5:
			{
				ushort num2 = Convert.ToUInt16(hexColor.Substring(1), 16);
				byte b4 = (byte)(num2 >> 12);
				byte b5 = (byte)((uint)(num2 >> 8) & 0xFu);
				byte b6 = (byte)((uint)(num2 >> 4) & 0xFu);
				byte b7 = (byte)(num2 & 0xFu);
				b4 = (byte)((b4 << 4) | b4);
				b5 = (byte)((b5 << 4) | b5);
				b6 = (byte)((b6 << 4) | b6);
				b7 = (byte)((b7 << 4) | b7);
				return new ColorHolder(b4, b5, b6, b7);
			}
			case 4:
			{
				ushort num = Convert.ToUInt16(hexColor.Substring(1), 16);
				byte b = (byte)((uint)(num >> 8) & 0xFu);
				byte b2 = (byte)((uint)(num >> 4) & 0xFu);
				byte b3 = (byte)(num & 0xFu);
				b = (byte)((b << 4) | b);
				b2 = (byte)((b2 << 4) | b2);
				b3 = (byte)((b3 << 4) | b3);
				return new ColorHolder(255, b, b2, b3);
			}
			default:
				throw new FormatException($"The {hexColor} string is not a recognized HexColor format.");
			}
		}
	}
}
