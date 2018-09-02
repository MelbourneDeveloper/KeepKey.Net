using Hid.Net;
using Hid.Net.UWP;
using System.Threading.Tasks;
using Windows.UI.Popups;
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
            keepKeyHidDevice.Connected += KeepKeyHidDevice_Connected;
            poller = new UWPHidDevicePoller(KeepKeyManager.ProductId, KeepKeyManager.VendorId, keepKeyHidDevice);
            LoadApplication(new app(keepKeyHidDevice));

            new MessageDialog("Unfortunately, KeepKey's Vendor Id is not accepted by the UWP runtime at the permissions level currently. Please check again soon and see https://github.com/MelbourneDeveloper/KeepKey.Net/issues/2").ShowAsync();

        }

        private void KeepKeyHidDevice_Connected(object sender, System.EventArgs e)
        {
            poller.Stop();
        }
    }
}
