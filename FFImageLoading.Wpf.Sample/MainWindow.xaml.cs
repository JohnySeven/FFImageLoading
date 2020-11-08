using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FFImageLoading.Forms.Platform;
using Xamarin.Forms.Platform;
using Xamarin.Forms.Platform.WPF;

namespace FFImageLoading.Wpf.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
	{
        public MainWindow()
        {
            InitializeComponent();

			var config = new FFImageLoading.Config.Configuration()
			{
				VerboseLogging = false,
				VerbosePerformanceLogging = false,
				VerboseMemoryCacheLogging = false,
				VerboseLoadingCancelledLogging = false,
				DiskCachePath = System.IO.Path.GetTempPath()
			};

			ImageService.Instance.Initialize(config);
			Xamarin.Forms.Forms.Init();
			CachedImageRenderer.Init();
			LoadApplication(new Forms.Sample.App());
		}
    }
}
