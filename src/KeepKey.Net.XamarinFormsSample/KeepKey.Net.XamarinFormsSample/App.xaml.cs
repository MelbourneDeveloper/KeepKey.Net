using Hardwarewallets.Net.AddressManagement;
using Hid.Net;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace KeepKey.Net.XamarinFormsSample
{
    public partial class App : Application
    {
        #region Fields
        private KeepKeyManager _KeepKeyManager;
        #endregion

        #region Public Static Propertoes
        public static string Address;
        public static NavigationPage MainNavigationPage { get; private set; }
        #endregion

        #region Public Static Events
        public static event EventHandler GetAddress;
        #endregion

        #region Constructor
        public App(IHidDevice keepKeyHidDevice)
        {
            _KeepKeyManager = new KeepKeyManager(KeepKeyPinPad.GetPin, keepKeyHidDevice);
            InitializeComponent();
            keepKeyHidDevice.Connected += KeepKeyHidDevice_Connected;
            MainNavigationPage = new NavigationPage(new MainPage());
            MainPage = MainNavigationPage;
        }
        #endregion

        #region Event Handlers
        private void KeepKeyHidDevice_Connected(object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _KeepKeyManager.InitializeAsync();
                var coinTable = await _KeepKeyManager.GetCoinTable();
                _KeepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
                Address = await _KeepKeyManager.GetAddressAsync(new BIP44AddressPath(false, 0, 0, false, 0), false, true);
                GetAddress?.Invoke(this, new EventArgs());
            });
        }
        #endregion

        #region Protected Overrides
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion
    }
}
