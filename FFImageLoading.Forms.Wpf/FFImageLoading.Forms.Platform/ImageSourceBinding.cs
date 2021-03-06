using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFImageLoading.Work;
using XF = Xamarin.Forms;

namespace FFImageLoading.Forms.Platform
{
	public class ImageSourceBinding : IImageSourceBinding
	{
		public ImageSource ImageSource
		{
			get;
			private set;
		}

		public string Path
		{
			get;
			private set;
		}

		public Func<CancellationToken, Task<Stream>> Stream
		{
			get;
			private set;
		}

		public ImageSourceBinding(ImageSource imageSource, string path)
		{
			ImageSource = imageSource;
			Path = path;
		}

		public ImageSourceBinding(Func<CancellationToken, Task<Stream>> stream)
		{
			ImageSource = ImageSource.Stream;
			Stream = stream;
		}

		internal static async Task<ImageSourceBinding> GetImageSourceBinding(XF.ImageSource source, CachedImage element = null)
		{
			if (source == null)
			{
				return null;
			}
			var uriImageSource = source as XF.UriImageSource;
			if (uriImageSource != null)
			{
				string uri = uriImageSource.Uri.OriginalString;
				if (string.IsNullOrWhiteSpace(uri))
				{
					return null;
				}
				return new ImageSourceBinding(ImageSource.Url, uri);
			}
			var fileImageSource = source as XF.FileImageSource;
			if (fileImageSource != null)
			{
				if (string.IsNullOrWhiteSpace(fileImageSource.File))
				{
					return null;
				}
				try
				{
					if (System.IO.Path.IsPathRooted(fileImageSource.File))
					{
						return new ImageSourceBinding(ImageSource.Filepath, fileImageSource.File);
					}
					else
					{
						return new ImageSourceBinding(ImageSource.CompiledResource, fileImageSource.File);
					}
				}
				catch (Exception)
				{
				}
				return new ImageSourceBinding(ImageSource.ApplicationBundle, fileImageSource.File);
			}
			var streamImageSource = source as XF.StreamImageSource;
			if (streamImageSource != null)
			{
				return new ImageSourceBinding(streamImageSource.Stream);
			}
			EmbeddedResourceImageSource embeddedResoureSource = source as EmbeddedResourceImageSource;
			if (embeddedResoureSource != null)
			{
				string uri2 = embeddedResoureSource.Uri?.OriginalString;
				if (string.IsNullOrWhiteSpace(uri2))
				{
					return null;
				}
				return new ImageSourceBinding(ImageSource.EmbeddedResource, uri2);
			}
			DataUrlImageSource dataUrlSource = source as DataUrlImageSource;
			if (dataUrlSource != null)
			{
				if (string.IsNullOrWhiteSpace(dataUrlSource.DataUrl))
				{
					return null;
				}
				return new ImageSourceBinding(ImageSource.Url, dataUrlSource.DataUrl);
			}
			IVectorImageSource vectorSource = source as IVectorImageSource;
			if (vectorSource != null)
			{
				if (element != null && vectorSource.VectorHeight == 0 && vectorSource.VectorHeight == 0)
				{
					var visual = element as XF.VisualElement;
					if (visual.Height > 0.0 && !double.IsInfinity(visual.Height))
					{
						vectorSource.UseDipUnits = true;
						vectorSource.VectorHeight = (int)visual.Height;
					}
					else if (visual.Width > 0.0 && !double.IsInfinity(visual.Width))
					{
						vectorSource.UseDipUnits = true;
						vectorSource.VectorWidth = (int)visual.Width;
					}
					else if (visual.HeightRequest > 0.0 && !double.IsInfinity(visual.HeightRequest))
					{
						vectorSource.UseDipUnits = true;
						vectorSource.VectorHeight = (int)visual.HeightRequest;
					}
					else if (visual.WidthRequest > 0.0 && !double.IsInfinity(visual.WidthRequest))
					{
						vectorSource.UseDipUnits = true;
						vectorSource.VectorWidth = (int)visual.WidthRequest;
					}
					else if (visual.MinimumHeightRequest > 0.0 && !double.IsInfinity(visual.MinimumHeightRequest))
					{
						vectorSource.UseDipUnits = true;
						vectorSource.VectorHeight = (int)visual.MinimumHeightRequest;
					}
					else if (visual.MinimumWidthRequest > 0.0 && !double.IsInfinity(visual.MinimumWidthRequest))
					{
						vectorSource.UseDipUnits = true;
						vectorSource.VectorWidth = (int)visual.MinimumWidthRequest;
					}
				}
				return await GetImageSourceBinding(vectorSource.ImageSource, element).ConfigureAwait(continueOnCapturedContext: false);
			}
			throw new NotImplementedException("ImageSource type not supported");
		}

		public override bool Equals(object obj)
		{
			ImageSourceBinding imageSourceBinding = obj as ImageSourceBinding;
			if (imageSourceBinding == null)
			{
				return false;
			}
			return ImageSource == imageSourceBinding.ImageSource && Path == imageSourceBinding.Path && Stream == imageSourceBinding.Stream;
		}

		public override int GetHashCode()
		{
			int num = 17;
			num = num * 23 + ImageSource.GetHashCode();
			num = num * 23 + Path.GetHashCode();
			return num * 23 + Stream.GetHashCode();
		}
	}
}
