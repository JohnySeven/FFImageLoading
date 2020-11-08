using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FFImageLoading.Cache;
using FFImageLoading.Decoders;
using FFImageLoading.Extensions;
using FFImageLoading.Helpers;

namespace FFImageLoading.Work
{
	public class PlatformImageLoaderTask<TImageView> : ImageLoaderTask<BitmapHolder, BitmapSource, TImageView> where TImageView : class
	{
		public PlatformImageLoaderTask(ITarget<BitmapSource, TImageView> target, TaskParameter parameters, IImageService imageService)
			: base((IMemoryCache<BitmapSource>)ImageCache.Instance, target, parameters, imageService)
		{
		}

		public override async Task Init()
		{
			await ScaleHelper.InitAsync().ConfigureAwait(continueOnCapturedContext: false);
			await base.Init().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async Task SetTargetAsync(BitmapSource image, bool animated)
		{
			if (base.Target != null)
			{
				await base.MainThreadDispatcher.PostAsync(delegate
				{
					ThrowIfCancellationRequested();
					base.PlatformTarget.Set(this, image, animated);
				}).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		protected override int DpiToPixels(int size)
		{
			return size.DpToPixels();
		}

		protected override IDecoder<BitmapHolder> ResolveDecoder(ImageInformation.ImageType type)
		{
			if (type == ImageInformation.ImageType.GIF || type == ImageInformation.ImageType.WEBP)
			{
				throw new NotImplementedException();
			}
			return new BaseDecoder();
		}

		protected override async Task<BitmapHolder> TransformAsync(BitmapHolder bitmap, IList<ITransformation> transformations, string path, ImageSource source, bool isPlaceholder)
		{
			await StaticLocks.DecodingLock.WaitAsync(base.CancellationTokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
			ThrowIfCancellationRequested();
			try
			{
				foreach (ITransformation transformation in transformations)
				{
					ThrowIfCancellationRequested();
					BitmapHolder old = bitmap;
					try
					{
						IBitmap bitmapHolder = transformation.Transform(bitmap, path, source, isPlaceholder, base.Key);
						bitmap = bitmapHolder.ToNative();
					}
					catch (Exception ex2)
					{
						Exception ex = ex2;
						base.Logger.Error($"Transformation failed: {transformation.Key}", ex);
						throw;
					}
					finally
					{
						if (old != null && old != bitmap && old.PixelData != bitmap.PixelData)
						{
							old.FreePixels();
						}
					}
				}
			}
			finally
			{
				StaticLocks.DecodingLock.Release();
			}
			return bitmap;
		}

		protected override async Task<BitmapSource> GenerateImageFromDecoderContainerAsync(IDecodedImage<BitmapHolder> decoded, ImageInformation imageInformation, bool isPlaceholder)
		{
			if (decoded.IsAnimated)
			{
				throw new NotImplementedException();
			}
			try
			{
				if (decoded.Image.HasWriteableBitmap)
				{
					return decoded.Image.WriteableBitmap;
				}
				return decoded.Image.ToBitmapImageAsync();
			}
			finally
			{
				decoded.Image.FreePixels();
				decoded.Image = null;
			}
		}
	}
}
