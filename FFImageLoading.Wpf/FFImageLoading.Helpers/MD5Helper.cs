using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FFImageLoading.Helpers
{
	public class MD5Helper : IMD5Helper
	{
		public string MD5(Stream input)
		{
			using (MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider())
			{
				byte[] value = mD5CryptoServiceProvider.ComputeHash(input);
				return BitConverter.ToString(value)?.ToSanitizedKey();
			}
		}

		public string MD5(string input)
		{
			byte[] value = ComputeHash(Encoding.UTF8.GetBytes(input));
			return BitConverter.ToString(value)?.ToSanitizedKey();
		}

		public byte[] ComputeHash(byte[] input)
		{
			using (MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider())
			{
				return mD5CryptoServiceProvider.ComputeHash(input);
			}
		}

		public static byte[] StreamToByteArray(Stream stream)
		{
			if (stream is MemoryStream)
			{
				return ((MemoryStream)stream).ToArray();
			}
			return ReadFully(stream);
		}

		public static byte[] ReadFully(Stream input)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				input.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}
	}
}
