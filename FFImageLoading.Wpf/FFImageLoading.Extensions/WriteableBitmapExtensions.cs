using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFImageLoading.Extensions
{
	public static class WriteableBitmapExtensions
	{
		public static async Task<byte[]> AsPngBytesAsync(this WriteableBitmap bitmap)
		{
			MemoryStream stream = await bitmap.AsPngStreamAsync();
			try
			{
				return await Task.Run(() => stream.ToArray());
			}
			finally
			{
				if (stream != null)
				{
					((IDisposable)stream).Dispose();
				}
			}
		}

		public static async Task<MemoryStream> AsPngStreamAsync(this WriteableBitmap bitmap)
		{
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			MemoryStream ret = new MemoryStream();
			await Task.Run(delegate
			{
				encoder.Save(ret);
			});
			ret.Seek(0L, SeekOrigin.Begin);
			return ret;
		}

		public static async Task<byte[]> AsJpegBytesAsync(this WriteableBitmap bitmap, int quality = 90)
		{
			MemoryStream stream = await bitmap.AsJpegStreamAsync(quality);
			try
			{
				return await Task.Run(() => stream.ToArray());
			}
			finally
			{
				if (stream != null)
				{
					((IDisposable)stream).Dispose();
				}
			}
		}

		public static async Task<MemoryStream> AsJpegStreamAsync(this WriteableBitmap bitmap, int quality = 90)
		{
			JpegBitmapEncoder encoder = new JpegBitmapEncoder
			{
				QualityLevel = quality
			};
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			MemoryStream ret = new MemoryStream();
			await Task.Run(delegate
			{
				encoder.Save(ret);
			});
			ret.Seek(0L, SeekOrigin.Begin);
			return ret;
		}
	}
}
