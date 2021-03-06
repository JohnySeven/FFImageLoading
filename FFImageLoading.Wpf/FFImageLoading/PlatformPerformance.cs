using System.Threading;

namespace FFImageLoading
{
	public class PlatformPerformance : IPlatformPerformance
	{
		public int GetCurrentManagedThreadId()
		{
			return Thread.CurrentThread.ManagedThreadId;
		}

		public int GetCurrentSystemThreadId()
		{
			return System.Windows.Application.Current.Dispatcher.Thread.ManagedThreadId;
		}

		public string GetMemoryInfo()
		{
			return "[PERFORMANCE] Memory - not implemented";
		}
	}
}
