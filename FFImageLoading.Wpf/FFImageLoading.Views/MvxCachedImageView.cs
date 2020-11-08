using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FFImageLoading.Args;
using FFImageLoading.Cache;
using FFImageLoading.Work;

namespace FFImageLoading.Views
{
	public class MvxCachedImageView : ContentControl, ICachedImageView, IDisposable
	{
		public class ImageSourceBinding
		{
			public FFImageLoading.Work.ImageSource ImageSource
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

			public ImageSourceBinding(FFImageLoading.Work.ImageSource imageSource, string path)
			{
				ImageSource = imageSource;
				Path = path;
			}

			public ImageSourceBinding(Func<CancellationToken, Task<Stream>> stream)
			{
				ImageSource = FFImageLoading.Work.ImageSource.Stream;
				Stream = stream;
				Path = "Stream";
			}

			public override int GetHashCode()
			{
				int num = 17;
				num = num * 23 + ImageSource.GetHashCode();
				num = num * 23 + Path.GetHashCode();
				return num * 23 + Stream.GetHashCode();
			}
		}

		private Image _internalImage;

		protected IScheduledWork _scheduledWork;

		protected ImageSourceBinding _lastImageSource;

		protected bool _isDisposed;

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata(false));

		public static readonly DependencyProperty RetryCountProperty = DependencyProperty.Register("RetryCount", typeof(int), typeof(MvxCachedImageView), new PropertyMetadata(3));

		public static readonly DependencyProperty RetryDelayProperty = DependencyProperty.Register("RetryDelay", typeof(int), typeof(MvxCachedImageView), new PropertyMetadata(500));

		public static readonly DependencyProperty LoadingDelayProperty = DependencyProperty.Register("LoadingDelay", typeof(int), typeof(MvxCachedImageView), new PropertyMetadata(0));

		public static readonly DependencyProperty DownsampleWidthProperty = DependencyProperty.Register("DownsampleWidth", typeof(double), typeof(MvxCachedImageView), new PropertyMetadata(0.0));

		public static readonly DependencyProperty DownsampleHeightProperty = DependencyProperty.Register("DownsampleHeight", typeof(double), typeof(MvxCachedImageView), new PropertyMetadata(0.0));

		public static readonly DependencyProperty DownsampleUseDipUnitsProperty = DependencyProperty.Register("DownsampleUseDipUnits", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata(false));

		public static readonly DependencyProperty CacheDurationProperty = DependencyProperty.Register("CacheDuration", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty LoadingPriorityProperty = DependencyProperty.Register("LoadingPriority", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata(LoadingPriority.Normal));

		public static readonly DependencyProperty BitmapOptimizationsProperty = DependencyProperty.Register("BitmapOptimizations", typeof(bool?), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty FadeAnimationEnabledProperty = DependencyProperty.Register("FadeAnimationEnabled", typeof(bool?), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty FadeAnimationForCachedImagesProperty = DependencyProperty.Register("FadeAnimationForCachedImages", typeof(bool?), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty InvalidateLayoutAfterLoadedProperty = DependencyProperty.Register("InvalidateLayoutAfterLoaded", typeof(bool?), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty FadeAnimationDurationProperty = DependencyProperty.Register("FadeAnimationDuration", typeof(int?), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty TransformPlaceholdersProperty = DependencyProperty.Register("TransformPlaceholders", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty CacheTypeProperty = DependencyProperty.Register("CacheType", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty TransformationsProperty = DependencyProperty.Register("Transformations", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata(new List<ITransformation>(), OnTransformationsChanged));

		public static readonly DependencyProperty CustomDataResolverProperty = DependencyProperty.Register("CustomDataResolver", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty CustomLoadingPlaceholderDataResolverProperty = DependencyProperty.Register("CustomLoadingPlaceholderDataResolver", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty CustomErrorPlaceholderDataResolverProperty = DependencyProperty.Register("CustomErrorPlaceholderDataResolver", typeof(bool), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty LoadingPlaceholderImagePathProperty = DependencyProperty.Register("LoadingPlaceholderImagePath", typeof(string), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty ErrorPlaceholderImagePathProperty = DependencyProperty.Register("ErrorPlaceholderImagePath", typeof(string), typeof(MvxCachedImageView), new PropertyMetadata((object)null));

		public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImagePath", typeof(string), typeof(MvxCachedImageView), new PropertyMetadata(null, OnImageChanged));

		public static readonly DependencyProperty ImageStreamProperty = DependencyProperty.Register("ImageStream", typeof(Func<CancellationToken, Task<Stream>>), typeof(MvxCachedImageView), new PropertyMetadata(null, OnImageChanged));

		public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(MvxCachedImageView), new PropertyMetadata(Stretch.None, StretchPropertyChanged));

		public static readonly DependencyProperty HorizontalImageAlignmentProperty = DependencyProperty.Register("HorizontalImageAlignment", typeof(HorizontalAlignment), typeof(MvxCachedImageView), new PropertyMetadata(HorizontalAlignment.Stretch, HorizontalImageAlignmentPropertyChanged));

		public static readonly DependencyProperty VerticalImageAlignmentProperty = DependencyProperty.Register("VerticalImageAlignment", typeof(VerticalAlignment), typeof(MvxCachedImageView), new PropertyMetadata(VerticalAlignment.Stretch, VerticalImageAlignmentPropertyChanged));

		public Image Image
		{
			get
			{
				return _internalImage;
			}
			set
			{
				_internalImage = value;
				base.Content = _internalImage;
			}
		}

		private bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(this);

		public bool IsLoading
		{
			get
			{
				return (bool)GetValue(IsLoadingProperty);
			}
			set
			{
				SetValue(IsLoadingProperty, value);
			}
		}

		public int RetryCount
		{
			get
			{
				return (int)GetValue(RetryCountProperty);
			}
			set
			{
				SetValue(RetryCountProperty, value);
			}
		}

		public int RetryDelay
		{
			get
			{
				return (int)GetValue(RetryDelayProperty);
			}
			set
			{
				SetValue(RetryDelayProperty, value);
			}
		}

		public int LoadingDelay
		{
			get
			{
				return (int)GetValue(LoadingDelayProperty);
			}
			set
			{
				SetValue(LoadingDelayProperty, value);
			}
		}

		public double DownsampleWidth
		{
			get
			{
				return (double)GetValue(DownsampleWidthProperty);
			}
			set
			{
				SetValue(DownsampleWidthProperty, value);
			}
		}

		public double DownsampleHeight
		{
			get
			{
				return (double)GetValue(DownsampleHeightProperty);
			}
			set
			{
				SetValue(DownsampleHeightProperty, value);
			}
		}

		public bool DownsampleUseDipUnits
		{
			get
			{
				return (bool)GetValue(DownsampleUseDipUnitsProperty);
			}
			set
			{
				SetValue(DownsampleUseDipUnitsProperty, value);
			}
		}

		public TimeSpan? CacheDuration
		{
			get
			{
				return (TimeSpan?)GetValue(CacheDurationProperty);
			}
			set
			{
				SetValue(CacheDurationProperty, value);
			}
		}

		public LoadingPriority LoadingPriority
		{
			get
			{
				return (LoadingPriority)GetValue(LoadingPriorityProperty);
			}
			set
			{
				SetValue(LoadingPriorityProperty, value);
			}
		}

		public bool? BitmapOptimizations
		{
			get
			{
				return (bool?)GetValue(BitmapOptimizationsProperty);
			}
			set
			{
				SetValue(BitmapOptimizationsProperty, value);
			}
		}

		public bool? FadeAnimationEnabled
		{
			get
			{
				return (bool?)GetValue(FadeAnimationEnabledProperty);
			}
			set
			{
				SetValue(FadeAnimationEnabledProperty, value);
			}
		}

		public bool? FadeAnimationForCachedImages
		{
			get
			{
				return (bool?)GetValue(FadeAnimationForCachedImagesProperty);
			}
			set
			{
				SetValue(FadeAnimationForCachedImagesProperty, value);
			}
		}

		public bool? InvalidateLayoutAfterLoaded
		{
			get
			{
				return (bool?)GetValue(InvalidateLayoutAfterLoadedProperty);
			}
			set
			{
				SetValue(InvalidateLayoutAfterLoadedProperty, value);
			}
		}

		public int? FadeAnimationDuration
		{
			get
			{
				return (int?)GetValue(FadeAnimationDurationProperty);
			}
			set
			{
				SetValue(FadeAnimationDurationProperty, value);
			}
		}

		public bool? TransformPlaceholders
		{
			get
			{
				return (bool?)GetValue(TransformPlaceholdersProperty);
			}
			set
			{
				SetValue(TransformPlaceholdersProperty, value);
			}
		}

		public CacheType? CacheType
		{
			get
			{
				return (CacheType?)GetValue(CacheTypeProperty);
			}
			set
			{
				SetValue(CacheTypeProperty, value);
			}
		}

		public List<ITransformation> Transformations
		{
			get
			{
				return (List<ITransformation>)GetValue(TransformationsProperty);
			}
			set
			{
				SetValue(TransformationsProperty, value);
			}
		}

		public IDataResolver CustomDataResolver
		{
			get
			{
				return (IDataResolver)GetValue(CustomDataResolverProperty);
			}
			set
			{
				SetValue(CustomDataResolverProperty, value);
			}
		}

		public IDataResolver CustomLoadingPlaceholderDataResolver
		{
			get
			{
				return (IDataResolver)GetValue(CustomLoadingPlaceholderDataResolverProperty);
			}
			set
			{
				SetValue(CustomLoadingPlaceholderDataResolverProperty, value);
			}
		}

		public IDataResolver CustomErrorPlaceholderDataResolver
		{
			get
			{
				return (IDataResolver)GetValue(CustomErrorPlaceholderDataResolverProperty);
			}
			set
			{
				SetValue(CustomErrorPlaceholderDataResolverProperty, value);
			}
		}

		public string LoadingPlaceholderImagePath
		{
			get
			{
				return (string)GetValue(LoadingPlaceholderImagePathProperty);
			}
			set
			{
				SetValue(LoadingPlaceholderImagePathProperty, value);
			}
		}

		public string ErrorPlaceholderImagePath
		{
			get
			{
				return (string)GetValue(ErrorPlaceholderImagePathProperty);
			}
			set
			{
				SetValue(ErrorPlaceholderImagePathProperty, value);
			}
		}

		public string ImagePath
		{
			get
			{
				return (string)GetValue(ImagePathProperty);
			}
			set
			{
				SetValue(ImagePathProperty, value);
			}
		}

		public Func<CancellationToken, Task<Stream>> ImageStream
		{
			get
			{
				return (Func<CancellationToken, Task<Stream>>)GetValue(ImageStreamProperty);
			}
			set
			{
				SetValue(ImageStreamProperty, value);
			}
		}

		public Stretch Stretch
		{
			get
			{
				return (Stretch)GetValue(StretchProperty);
			}
			set
			{
				SetValue(StretchProperty, value);
			}
		}

		public HorizontalAlignment HorizontalImageAlignment
		{
			get
			{
				return (HorizontalAlignment)GetValue(HorizontalImageAlignmentProperty);
			}
			set
			{
				SetValue(HorizontalImageAlignmentProperty, value);
			}
		}

		public VerticalAlignment VerticalImageAlignment
		{
			get
			{
				return (VerticalAlignment)GetValue(VerticalImageAlignmentProperty);
			}
			set
			{
				SetValue(VerticalImageAlignmentProperty, value);
			}
		}

		public string CustomCacheKey
		{
			get;
			set;
		}

		public event EventHandler<SuccessEventArgs> OnSuccess;

		public event EventHandler<FFImageLoading.Args.ErrorEventArgs> OnError;

		public event EventHandler<FinishEventArgs> OnFinish;

		public event EventHandler<DownloadStartedEventArgs> OnDownloadStarted;

		public event EventHandler<DownloadProgressEventArgs> OnDownloadProgress;

		public event EventHandler<FileWriteFinishedEventArgs> OnFileWriteFinished;

		public MvxCachedImageView()
		{
			base.HorizontalContentAlignment = HorizontalAlignment.Stretch;
			base.HorizontalAlignment = HorizontalAlignment.Stretch;
			base.VerticalContentAlignment = VerticalAlignment.Stretch;
			base.VerticalAlignment = VerticalAlignment.Stretch;
			_internalImage = new Image
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Opacity = 1.0
			};
			base.Content = _internalImage;
			Transformations = new List<ITransformation>();
		}

		protected static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MvxCachedImageView mvxCachedImageView = (MvxCachedImageView)d;
			if (!mvxCachedImageView.IsInDesignMode)
			{
				mvxCachedImageView.UpdateImageLoadingTask();
			}
		}

		protected static void OnTransformationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MvxCachedImageView mvxCachedImageView = (MvxCachedImageView)d;
			if (!mvxCachedImageView.IsInDesignMode && mvxCachedImageView._lastImageSource != null)
			{
				mvxCachedImageView.UpdateImageLoadingTask();
			}
		}

		private static void StretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MvxCachedImageView)d)._internalImage.Stretch = (Stretch)e.NewValue;
		}

		private static void HorizontalImageAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MvxCachedImageView)d)._internalImage.HorizontalAlignment = (HorizontalAlignment)e.NewValue;
		}

		private static void VerticalImageAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MvxCachedImageView)d)._internalImage.VerticalAlignment = (VerticalAlignment)e.NewValue;
		}

		public void Cancel()
		{
			try
			{
				IScheduledWork scheduledWork = _scheduledWork;
				if (scheduledWork != null && !scheduledWork.IsCancelled)
				{
					scheduledWork.Cancel();
				}
			}
			catch (Exception)
			{
			}
		}

		public void Reload()
		{
			UpdateImageLoadingTask();
		}

		protected virtual void UpdateImageLoadingTask()
		{
			ImageSourceBinding ffSource = GetImageSourceBinding(ImagePath, ImageStream);
			ImageSourceBinding imageSourceBinding = GetImageSourceBinding(LoadingPlaceholderImagePath, null);
			Cancel();
			TaskParameter taskParameter = null;
			if (ffSource == null)
			{
				_internalImage.Source = null;
				IsLoading = false;
				return;
			}
			IsLoading = true;
			if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.Url)
			{
				taskParameter = ImageService.Instance.LoadUrl(ffSource.Path, CacheDuration);
			}
			else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.CompiledResource)
			{
				taskParameter = ImageService.Instance.LoadCompiledResource(ffSource.Path);
			}
			else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.ApplicationBundle)
			{
				taskParameter = ImageService.Instance.LoadFileFromApplicationBundle(ffSource.Path);
			}
			else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.Filepath)
			{
				taskParameter = ImageService.Instance.LoadFile(ffSource.Path);
			}
			else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.Stream)
			{
				taskParameter = ImageService.Instance.LoadStream(ffSource.Stream);
			}
			else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.EmbeddedResource)
			{
				taskParameter = ImageService.Instance.LoadEmbeddedResource(ffSource.Path);
			}
			if (taskParameter == null)
			{
				return;
			}
			if (imageSourceBinding != null && imageSourceBinding != null)
			{
				taskParameter.LoadingPlaceholder(imageSourceBinding.Path, imageSourceBinding.ImageSource);
			}
			if (!string.IsNullOrWhiteSpace(ErrorPlaceholderImagePath))
			{
				ImageSourceBinding imageSourceBinding2 = GetImageSourceBinding(ErrorPlaceholderImagePath, null);
				if (imageSourceBinding2 != null)
				{
					taskParameter.ErrorPlaceholder(imageSourceBinding2.Path, imageSourceBinding2.ImageSource);
				}
			}
			if (CustomDataResolver != null)
			{
				taskParameter.WithCustomDataResolver(CustomDataResolver);
				taskParameter.WithCustomLoadingPlaceholderDataResolver(CustomLoadingPlaceholderDataResolver);
				taskParameter.WithCustomErrorPlaceholderDataResolver(CustomErrorPlaceholderDataResolver);
			}
			if ((int)DownsampleHeight != 0 || (int)DownsampleWidth != 0)
			{
				if (DownsampleUseDipUnits)
				{
					taskParameter.DownSampleInDip((int)DownsampleWidth, (int)DownsampleHeight);
				}
				else
				{
					taskParameter.DownSample((int)DownsampleWidth, (int)DownsampleHeight);
				}
			}
			if (RetryCount > 0)
			{
				taskParameter.Retry(RetryCount, RetryDelay);
			}
			if (BitmapOptimizations.HasValue)
			{
				taskParameter.BitmapOptimizations(BitmapOptimizations.Value);
			}
			if (FadeAnimationEnabled.HasValue)
			{
				taskParameter.FadeAnimation(FadeAnimationEnabled.Value, null, FadeAnimationDuration);
			}
			if (FadeAnimationEnabled.HasValue && FadeAnimationForCachedImages.HasValue)
			{
				taskParameter.FadeAnimation(FadeAnimationEnabled.Value, FadeAnimationForCachedImages.Value, FadeAnimationDuration);
			}
			if (TransformPlaceholders.HasValue)
			{
				taskParameter.TransformPlaceholders(TransformPlaceholders.Value);
			}
			if (Transformations != null && Transformations.Count > 0)
			{
				taskParameter.Transform(Transformations);
			}
			if (InvalidateLayoutAfterLoaded.HasValue)
			{
				taskParameter.InvalidateLayout(InvalidateLayoutAfterLoaded.Value);
			}
			taskParameter.WithPriority(LoadingPriority);
			if (CacheType.HasValue)
			{
				taskParameter.WithCache(CacheType.Value);
			}
			if (LoadingDelay > 0)
			{
				taskParameter.Delay(LoadingDelay);
			}
			taskParameter.Finish(delegate(IScheduledWork work)
			{
				IsLoading = false;
				this.OnFinish?.Invoke(this, new FinishEventArgs(work));
			});
			taskParameter.Success(delegate(ImageInformation imageInformation, LoadingResult loadingResult)
			{
				this.OnSuccess?.Invoke(this, new SuccessEventArgs(imageInformation, loadingResult));
				_lastImageSource = ffSource;
			});
			if (this.OnError != null)
			{
				taskParameter.Error(delegate(Exception ex)
				{
					this.OnError?.Invoke(this, new FFImageLoading.Args.ErrorEventArgs(ex));
				});
			}
			if (this.OnDownloadStarted != null)
			{
				taskParameter.DownloadStarted(delegate(DownloadInformation downloadInformation)
				{
					this.OnDownloadStarted(this, new DownloadStartedEventArgs(downloadInformation));
				});
			}
			if (this.OnDownloadProgress != null)
			{
				taskParameter.DownloadProgress(delegate(DownloadProgress progress)
				{
					this.OnDownloadProgress(this, new DownloadProgressEventArgs(progress));
				});
			}
			if (this.OnFileWriteFinished != null)
			{
				taskParameter.FileWriteFinished(delegate(FileWriteInfo info)
				{
					this.OnFileWriteFinished(this, new FileWriteFinishedEventArgs(info));
				});
			}
			if (!string.IsNullOrWhiteSpace(CustomCacheKey))
			{
				taskParameter.CacheKey(CustomCacheKey);
			}
			SetupOnBeforeImageLoading(taskParameter);
			_scheduledWork = taskParameter.Into(_internalImage);
		}

		protected virtual void SetupOnBeforeImageLoading(TaskParameter imageLoader)
		{
		}

		protected virtual ImageSourceBinding GetImageSourceBinding(string imagePath, Func<CancellationToken, Task<Stream>> imageStream)
		{
			if (string.IsNullOrWhiteSpace(imagePath) && imageStream == null)
			{
				return null;
			}
			if (imageStream != null)
			{
				return new ImageSourceBinding(FFImageLoading.Work.ImageSource.Stream, "Stream");
			}
			if (imagePath.StartsWith("res:", StringComparison.OrdinalIgnoreCase))
			{
				string path = imagePath.Split(new string[1]
				{
					"res:"
				}, StringSplitOptions.None)[1];
				return new ImageSourceBinding(FFImageLoading.Work.ImageSource.CompiledResource, path);
			}
			if (imagePath.IsDataUrl())
			{
				return new ImageSourceBinding(FFImageLoading.Work.ImageSource.Url, imagePath);
			}
			if (Uri.TryCreate(imagePath, UriKind.Absolute, out var result))
			{
				if (result.Scheme == "file")
				{
					return new ImageSourceBinding(FFImageLoading.Work.ImageSource.Filepath, result.LocalPath);
				}
				if (result.Scheme == "resource")
				{
					return new ImageSourceBinding(FFImageLoading.Work.ImageSource.EmbeddedResource, imagePath);
				}
				if (result.Scheme == "app")
				{
					return new ImageSourceBinding(FFImageLoading.Work.ImageSource.CompiledResource, result.LocalPath);
				}
				return new ImageSourceBinding(FFImageLoading.Work.ImageSource.Url, imagePath);
			}
			return new ImageSourceBinding(FFImageLoading.Work.ImageSource.CompiledResource, imagePath);
		}

		public virtual void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				Cancel();
			}
		}
	}
}
