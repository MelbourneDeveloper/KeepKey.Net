
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Widget;
using Device.Net;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace KeepKey.Net.XamarinFormsSample.Droid
{
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Activity(Label = "KeepKey.Net.XamarinFormsSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                var usbManager = GetSystemService(UsbService) as UsbManager;
                if (usbManager == null) throw new Exception("UsbManager is null");

                //Register the factory for creating Usb devices. This only needs to be done once.
                AndroidUsbDeviceFactory.Register(usbManager, base.ApplicationContext);

                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);
                Forms.Init(this, savedInstanceState);
                LoadApplication(new App());
            }
            catch(Exception ex)
            {
                Toast.MakeText(ApplicationContext, ex.ToString(), ToastLength.Long).Show();
            }
        }
    }
}