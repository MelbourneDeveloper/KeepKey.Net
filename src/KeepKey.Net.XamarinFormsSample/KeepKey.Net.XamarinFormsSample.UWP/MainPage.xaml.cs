using System;
using app = KeepKey.Net.XamarinFormsSample.App;

namespace KeepKey.Net.XamarinFormsSample.UWP
{
    public sealed partial class MainPage
    {
        private UWPHidDevicePoller poller;
        private UWPHidDevice _KeepKeyHidDevice;

        public MainPage()
        {
            InitializeComponent();
            _KeepKeyHidDevice = new UWPHidDevice();
            LoadApplication(new app(_KeepKeyHidDevice));
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _KeepKeyHidDevice.DataHasExtraByte = false;
            _KeepKeyHidDevice.Connected += KeepKeyHidDevice_Connected;
            poller = new UWPHidDevicePoller(KeepKeyManager.ProductId, KeepKeyManager.VendorId, _KeepKeyHidDevice);
        }

        private void KeepKeyHidDevice_Connected(object sender, EventArgs e)
        {
            poller.Stop();
        }
    }
}
