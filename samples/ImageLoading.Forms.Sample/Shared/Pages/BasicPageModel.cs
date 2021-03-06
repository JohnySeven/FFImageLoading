using System;
using System.IO;
using Xamarin.Forms;
using Xamvvm;

namespace FFImageLoading.Forms.Sample
{
    
    public class BasicPageModel : BasePageModel
    {
        public void Reload()
        {
			// ImageUrl = Helpers.GetRandomImageUrl();
			Image = ImageSource.FromStream(() => new FileStream(@"C:\Users\info\Desktop\DCIM\100D5100\DSC_0607.JPG", FileMode.Open));
        }

        public ImageSource Image { get; set; }
    }
}
