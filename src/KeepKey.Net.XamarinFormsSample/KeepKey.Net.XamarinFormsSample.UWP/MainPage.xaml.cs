using Hid.Net.UWP;
using Usb.Net.UWP;
using app = KeepKey.Net.XamarinFormsSample.App;

namespace KeepKey.Net.XamarinFormsSample.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            UWPHidDeviceFactory.Register();
            UWPUsbDeviceFactory.Register();

            LoadApplication(new app());
        }
    }
}
