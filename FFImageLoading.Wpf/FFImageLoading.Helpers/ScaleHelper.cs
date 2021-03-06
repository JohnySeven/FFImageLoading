using System.Threading.Tasks;
using FFImageLoading.Wpf;

namespace FFImageLoading.Helpers
{
	public static class ScaleHelper
	{
		private static double? _scale;

		public static double Scale
		{
			get
			{
				if (!_scale.HasValue)
				{
					InitAsync().ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
				}
				return _scale.Value;
			}
		}

		public static async Task InitAsync()
		{
			if (!_scale.HasValue)
			{
				IMainThreadDispatcher dispatcher = ImageService.Instance.Config.MainThreadDispatcher;
				await dispatcher.PostAsync(delegate
				{
					_scale = 1.0;
				}).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}
}
