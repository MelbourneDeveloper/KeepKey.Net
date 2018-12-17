﻿
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Usb.Net.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace KeepKey.Net.XamarinFormsSample.Droid
{
    public class DebugTracer : Device.Net.ITracer
    {
        public string WriteSuffix { get; set; } = "KeepKeyIOWrite";
        public string ReadSuffix { get; set; } = "KeepKeyIORead";

        public void Trace(bool isWrite, byte[] data)
        {
            System.Diagnostics.Debug.WriteLine($"({string.Join(",", data)}) - {(isWrite ? WriteSuffix : ReadSuffix)} ({data.Length})");
        }
    }

    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Activity(Label = "KeepKey.Net.XamarinFormsSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        #region Fields
        private AndroidUsbDevice _KeepKeyHidDevice;
        private UsbDeviceAttachedReceiver _KeepKeyUsbDeviceAttachedReceiver;
        private UsbDeviceDetachedReceiver _KeepKeyUsbDeviceDetachedReceiver;
        private readonly object _ReceiverLock = new object();
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _KeepKeyHidDevice = new AndroidUsbDevice(GetSystemService(UsbService) as UsbManager, ApplicationContext, 3000, 64, KeepKeyManager.VendorId, KeepKeyManager.ProductId);

            _KeepKeyHidDevice.Tracer = new DebugTracer();

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