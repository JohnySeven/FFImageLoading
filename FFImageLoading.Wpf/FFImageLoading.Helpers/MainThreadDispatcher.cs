using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FFImageLoading.Helpers
{
	public class MainThreadDispatcher : IMainThreadDispatcher
	{
		private Dispatcher _dispatcher;

		public async void Post(Action action)
		{
			if (action == null)
			{
				return;
			}
			if (_dispatcher == null)
			{
				_dispatcher = Application.Current.Dispatcher;
			}
			if (_dispatcher.CheckAccess())
			{
				action();
				return;
			}
			await _dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
			{
				action();
			});
		}

		public Task PostAsync(Action action)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Post(delegate
			{
				try
				{
					action?.Invoke();
					tcs.SetResult(result: true);
				}
				catch (Exception exception)
				{
					tcs.SetException(exception);
				}
			});
			return tcs.Task;
		}

		public Task PostAsync(Func<Task> action)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Post(async delegate
			{
				try
				{
					await (action?.Invoke()).ConfigureAwait(continueOnCapturedContext: false);
					tcs.SetResult(result: true);
				}
				catch (Exception ex2)
				{
					Exception ex = ex2;
					tcs.SetException(ex);
				}
			});
			return tcs.Task;
		}
	}
}
