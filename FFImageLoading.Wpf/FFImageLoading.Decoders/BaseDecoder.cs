using System;
using System.IO;
using System.Threading.Tasks;
using FFImageLoading.Config;
using FFImageLoading.Extensions;
using FFImageLoading.Helpers;
using FFImageLoading.Work;
using FFImageLoading.Wpf;

namespace FFImageLoading.Decoders
{
	public class BaseDecoder : IDecoder<BitmapHolder>
	{
		public Configuration Configuration => ImageService.Instance.Config;

		public IMiniLogger Logger => ImageService.Instance.Config.Logger;

		public async Task<IDecodedImage<BitmapHolder>> DecodeAsync(Stream imageData, string path, ImageSource source, ImageInformation imageInformation, TaskParameter parameters)
		{
			if (imageData == null)
			{
				throw new ArgumentNullException("imageData");
			}
			bool allowUpscale = parameters.AllowUpscale ?? Configuration.AllowUpscale;
			BitmapHolder imageIn = ((parameters.Transformations != null && parameters.Transformations.Count != 0) ? (await imageData.ToBitmapHolderAsync(parameters.DownSampleSize, parameters.DownSampleUseDipUnits, parameters.DownSampleInterpolationMode, allowUpscale, imageInformation).ConfigureAwait(continueOnCapturedContext: false)) : new BitmapHolder(await imageData.ToBitmapImageAsync(parameters.DownSampleSize, parameters.DownSampleUseDipUnits, parameters.DownSampleInterpolationMode, allowUpscale, imageInformation).ConfigureAwait(continueOnCapturedContext: false)));
			return new DecodedImage<BitmapHolder>
			{
				Image = imageIn
			};
		}
	}
}
