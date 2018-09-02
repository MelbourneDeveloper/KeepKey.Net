
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Hid.Net.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace KeepKey.Net.XamarinFormsSample.Droid
{
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Activity(Label = "KeepKey.Net.XamarinFormsSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        #region Fields
        private AndroidHidDevice _KeepKeyHidDevice;
        private UsbDeviceAttachedReceiver _KeepKeyUsbDeviceAttachedReceiver;
        private UsbDeviceDetachedReceiver _KeepKeyUsbDeviceDetachedReceiver;
        private readonly object _ReceiverLock = new object();
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _KeepKeyHidDevice = new AndroidHidDevice(GetSystemService(UsbService) as UsbManager, ApplicationContext, 3000, 64, KeepKeyManager.VendorId, KeepKeyManager.ProductId);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);
            RegisterReceiver();
            LoadApplication(new App(_KeepKeyHidDevice));
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver();
        }

        private void RegisterReceiver()
        {
            try
            {
                lock (_ReceiverLock)
                {
                    if (_KeepKeyUsbDeviceAttachedReceiver != null)
                    {
                        UnregisterReceiver(_KeepKeyUsbDeviceAttachedReceiver);
                        _KeepKeyUsbDeviceAttachedReceiver.Dispose();
                    }

                    _KeepKeyUsbDeviceAttachedReceiver = new UsbDeviceAttachedReceiver(_KeepKeyHidDevice);
                    RegisterReceiver(_KeepKeyUsbDeviceAttachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));


                    if (_KeepKeyUsbDeviceDetachedReceiver != null)
                    {
                        UnregisterReceiver(_KeepKeyUsbDeviceDetachedReceiver);
                        _KeepKeyUsbDeviceDetachedReceiver.Dispose();
                    }


                    _KeepKeyUsbDeviceDetachedReceiver = new UsbDeviceDetachedReceiver(_KeepKeyHidDevice);
                    RegisterReceiver(_KeepKeyUsbDeviceDetachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
                }
            }
            catch
            {

            }
        }
    }
}