using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FFImageLoading.Work;
using FFImageLoading.Wpf;

namespace FFImageLoading.Extensions
{
	public static class ImageExtensions
	{
		private static WriteableBitmap NewBitmap(int pixelWidth, int pixelHeight, Action<WriteableBitmap> action = null)
		{
			WriteableBitmap writeableBitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96.0, 96.0, PixelFormats.Pbgra32, null);
			action?.Invoke(writeableBitmap);
			writeableBitmap.Freeze();
			return writeableBitmap;
		}

		private static async Task<WriteableBitmap> NewBitmapAsync(int pixelWidth, int pixelHeight, Func<WriteableBitmap, Task> func = null)
		{
			WriteableBitmap ret = new WriteableBitmap(pixelWidth, pixelHeight, 96.0, 96.0, PixelFormats.Pbgra32, null);
			if (func != null)
			{
				await func(ret);
			}
			ret.Freeze();
			return ret;
		}

		public static WriteableBitmap ToBitmapImageAsync(this BitmapHolder holder)
		{
			if (holder == null || holder.PixelData == null)
			{
				return null;
			}
			return holder.ToWriteableBitmap();
		}

		private static WriteableBitmap ToWriteableBitmap(this BitmapHolder holder)
		{
			return NewBitmap(holder.Width, holder.Height, delegate (WriteableBitmap b)
			{
				int stride = b.GetStride();
				b.WritePixels(new Int32Rect(0, 0, holder.Width, holder.Height), holder.PixelData, stride, 0);
			});
		}

		private static int GetStride(this BitmapSource bitmap)
		{
			return (bitmap as WriteableBitmap)?.BackBufferStride ?? (bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8));
		}

		private static async Task WriteBytesFromStreamAsync(this WriteableBitmap bitmap, Stream source)
		{
			MemoryStream dstStream = new MemoryStream();
			try
			{
				await source.CopyToAsync(dstStream);
				await Task.Run(delegate
				{
					bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), dstStream.ToArray(), bitmap.GetStride(), 0);
				});
			}
			finally
			{
				if (dstStream != null)
				{
					((IDisposable)dstStream).Dispose();
				}
			}
		}

		private static void WriteBytesFromStream(this WriteableBitmap bitmap, Stream source)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				source.CopyTo(memoryStream);
				bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), memoryStream.ToArray(), bitmap.GetStride(), 0);
			}
		}

		private static WriteableBitmap ToWritableBitmap(this Stream source, int pixelWidth, int pixelHeight)
		{
			MemoryStream dstStream = new MemoryStream();
			try
			{
				source.CopyTo(dstStream);
				return NewBitmap(pixelWidth, pixelHeight, delegate(WriteableBitmap b)
				{
					b.WritePixels(new Int32Rect(0, 0, b.PixelWidth, b.PixelHeight), dstStream.ToArray(), b.GetStride(), 0);
				});
			}
			finally
			{
				if (dstStream != null)
				{
					((IDisposable)dstStream).Dispose();
				}
			}
		}

		public static async Task<WriteableBitmap> ToBitmapImageAsync(this Stream imageStream, Tuple<int, int> downscale, bool downscaleDipUnits, InterpolationMode mode, bool allowUpscale, ImageInformation imageInformation = null)
		{
			if (imageStream == null)
			{
				return null;
			}
			using (imageStream)
			{
				if(downscale == null)
				{
					downscale = new Tuple<int, int>(1, 1);
				}

				if (downscale != null && (downscale.Item1 > 0 || downscale.Item2 > 0))
				{
					return imageStream.ResizeImage(downscale.Item1, downscale.Item2, mode, downscaleDipUnits, allowUpscale, imageInformation);
				}
				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.None;
				image.StreamSource = imageStream;
				image.EndInit();
				image.Freeze();
				imageStream.Seek(0L, SeekOrigin.Begin);

				WriteableBitmap bitmap = null;
				if (imageInformation != null)
				{
					imageInformation.SetCurrentSize(image.PixelWidth, image.PixelHeight);
					imageInformation.SetOriginalSize(image.PixelWidth, image.PixelHeight);
				}
				await ImageService.Instance.Config.MainThreadDispatcher.PostAsync(delegate
				{
					bitmap = new WriteableBitmap(image);
					bitmap.Freeze();
				}).ConfigureAwait(continueOnCapturedContext: false);
				return bitmap;
			}
		}

		private static BitmapDecoder NewDecoder(Stream downscaledImage)
		{
			return BitmapDecoder.Create(downscaledImage, BitmapCreateOptions.None, BitmapCacheOption.None);
		}

		public static async Task<BitmapHolder> ToBitmapHolderAsync(this Stream imageStream, Tuple<int, int> downscale, bool downscaleDipUnits, InterpolationMode mode, bool allowUpscale, ImageInformation imageInformation = null)
		{
			if (imageStream == null)
			{
				return null;
			}
			using (imageStream)
			{
				if (downscale != null && (downscale.Item1 > 0 || downscale.Item2 > 0))
				{
					WriteableBitmap downscaledImage = imageStream.ResizeImage(downscale.Item1, downscale.Item2, mode, downscaleDipUnits, allowUpscale, imageInformation);
					return new BitmapHolder(await downscaledImage.ToBytesAsync(), downscaledImage.PixelWidth, downscaledImage.PixelHeight);
				}
				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.None;
				image.StreamSource = imageStream;
				image.EndInit();
				if (imageInformation != null)
				{
					imageInformation.SetCurrentSize(image.PixelWidth, image.PixelHeight);
					imageInformation.SetOriginalSize(image.PixelWidth, image.PixelHeight);
				}

				return new BitmapHolder(await image.ToBytesAsync(), image.PixelWidth, image.PixelHeight);
			}
		}

		private static async Task<byte[]> ToBytesAsync(this BitmapSource bitmap)
		{
			if (bitmap.Format.BitsPerPixel != 32)
			{
				bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0.0);
				bitmap.Freeze();
			}
			if(!bitmap.IsFrozen)
			{
				bitmap.Freeze();
			}
			int stride = bitmap.GetStride();
			byte[] bytes = new byte[stride * bitmap.PixelHeight];
			await Task.Run(delegate
			{
				bitmap.CopyPixels(bytes, stride, 0);
			});
			return bytes;
		}

		public static WriteableBitmap ResizeImage(this Stream imageStream, int width, int height, InterpolationMode interpolationMode, bool useDipUnits, bool allowUpscale, ImageInformation imageInformation = null)
		{
			if (useDipUnits)
			{
				width = width.DpToPixels();
				height = height.DpToPixels();
			}
			WriteableBitmap writeableBitmap = null;
			BitmapDecoder bitmapDecoder = NewDecoder(imageStream);
			BitmapFrame bitmapFrame = bitmapDecoder.Frames.First();

			if(width == 1 && height == 1)
			{
				width = bitmapFrame.PixelWidth;
				height = bitmapFrame.PixelHeight;
			}

			if ((height > 0 && bitmapFrame.PixelHeight > height) || (width > 0 && bitmapFrame.PixelWidth > width) || allowUpscale)
			{
				using (imageStream)
				{
					double num = (double)width / (double)bitmapFrame.PixelWidth;
					double num2 = (double)height / (double)bitmapFrame.PixelHeight;
					double num3 = Math.Min(num, num2);
					if (width == 0)
					{
						num3 = num2;
					}
					if (height == 0)
					{
						num3 = num;
					}
					uint height2 = (uint)Math.Floor((double)bitmapFrame.PixelHeight * num3);
					uint width2 = (uint)Math.Floor((double)bitmapFrame.PixelWidth * num3);
					TransformedBitmap source = new TransformedBitmap(bitmapFrame, new ScaleTransform(num3, num3));
					if (imageInformation != null)
					{
						imageInformation.SetOriginalSize(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight);
						imageInformation.SetCurrentSize((int)width2, (int)height2);
					}
					writeableBitmap = new WriteableBitmap(source);
				}
			}
			else
			{
				writeableBitmap = new WriteableBitmap(bitmapFrame);
			}
			if (!writeableBitmap.IsFrozen)
			{
				writeableBitmap.Freeze();
			}
			return writeableBitmap;
		}
	}
}
