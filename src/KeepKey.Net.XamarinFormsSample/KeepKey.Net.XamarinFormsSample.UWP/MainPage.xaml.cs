
using Device.Net;
using Usb.Net.UWP;
using app = KeepKey.Net.XamarinFormsSample.App;

namespace KeepKey.Net.XamarinFormsSample.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            UWPUsbDeviceFactory.Register(new DebugLogger(), new DebugTracer());

            LoadApplication(new app());
        }
    }
}
