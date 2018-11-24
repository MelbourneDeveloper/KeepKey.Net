using Hid.Net;
using Hid.Net.UWP;
using System.Collections.Generic;
using System.Threading.Tasks;
using app = KeepKey.Net.XamarinFormsSample.App;

namespace KeepKey.Net.XamarinFormsSample.UWP
{
    public sealed partial class MainPage
    {
        private UWPHidDevicePoller poller;

        public MainPage()
        {
            InitializeComponent();

            var taskCompletionSource = new TaskCompletionSource<IHidDevice>();
            var keepKeyHidDevice = new UWPHidDevice();
            keepKeyHidDevice.DataHasExtraByte = false;
            keepKeyHidDevice.Connected += KeepKeyHidDevice_Connected;
            poller = new UWPHidDevicePoller(KeepKeyManager.ProductId, KeepKeyManager.VendorId, new List<string> { @"\\?\USB#VID_2B24&PID_0001#343737341247363332002C00#{a5dcbf10-6530-11d2-901f-00c04fb951ed}" }, keepKeyHidDevice);
            LoadApplication(new app(keepKeyHidDevice));
        }

        private void KeepKeyHidDevice_Connected(object sender, System.EventArgs e)
        {
            poller.Stop();
        }
    }
}
