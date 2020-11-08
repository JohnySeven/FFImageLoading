using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using FFImageLoading.Work;

namespace FFImageLoading.Targets
{
	public class ImageTarget : Target<BitmapSource, Image>
	{
		private readonly WeakReference<Image> _controlWeakReference;

		public override bool IsValid => Control != null;

		public override Image Control
		{
			get
			{
				if (!_controlWeakReference.TryGetTarget(out var target))
				{
					return null;
				}
				if (target == null)
				{
					return null;
				}
				return target;
			}
		}

		public ImageTarget(Image control)
		{
			_controlWeakReference = new WeakReference<Image>(control);
		}

		public override void SetAsEmpty(IImageLoaderTask task)
		{
			Image control = Control;
			if (control != null)
			{
				control.Source = null;
			}
		}

		public override void Set(IImageLoaderTask task, BitmapSource image, bool animated)
		{
			animated = false;
			if (task.IsCancelled)
			{
				return;
			}
			Image control = Control;
			if (control == null || control.Source == image)
			{
				return;
			}
			TaskParameter parameters = task.Parameters;
			if (animated)
			{
				int num = (parameters.FadeAnimationDuration.HasValue ? parameters.FadeAnimationDuration.Value : ImageService.Instance.Config.FadeAnimationDuration);
				DoubleAnimation doubleAnimation = new DoubleAnimation();
				doubleAnimation.Duration = TimeSpan.FromMilliseconds(num);
				doubleAnimation.From = 0.0;
				doubleAnimation.To = 1.0;
				doubleAnimation.EasingFunction = new CubicEase
				{
					EasingMode = EasingMode.EaseInOut
				};
				Storyboard storyboard = new Storyboard();
				Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Image.Opacity"));
				Storyboard.SetTarget(doubleAnimation, control);
				storyboard.Children.Add(doubleAnimation);
				storyboard.Begin();
				control.Source = image;
				if (IsLayoutNeeded(task))
				{
					control.UpdateLayout();
				}
			}
			else
			{
				control.Source = image;
				if (IsLayoutNeeded(task))
				{
					control.UpdateLayout();
				}
			}
		}

		private bool IsLayoutNeeded(IImageLoaderTask task)
		{
			if (task.Parameters.InvalidateLayoutEnabled.HasValue)
			{
				if (!task.Parameters.InvalidateLayoutEnabled.Value)
				{
					return false;
				}
			}
			else if (!task.Configuration.InvalidateLayout)
			{
				return false;
			}
			return true;
		}
	}
}
