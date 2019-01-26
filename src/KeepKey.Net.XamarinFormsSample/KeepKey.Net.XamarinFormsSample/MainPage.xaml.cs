using Hardwarewallets.Net.AddressManagement;
using Xamarin.Forms;

namespace KeepKey.Net.XamarinFormsSample
{
    public partial class MainPage : ContentPage
    {
        #region Fields
        private KeepKeyManagerBroker KeepKeyManagerBroker;
        private bool _IsDisplayed;
        #endregion

        #region Constructor
        public MainPage()
        {
            InitializeComponent();
        }
        #endregion

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_IsDisplayed) return;
            _IsDisplayed = true;

            KeepKeyManagerBroker = new KeepKeyManagerBroker(KeepKeyPinPad.GetPin, 2000);
            var keepKeyManager = await KeepKeyManagerBroker.WaitForFirstTrezorAsync();
            var coinTable = await keepKeyManager.GetCoinTable();
            keepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);

            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                var address = await keepKeyManager.GetAddressAsync(new BIP44AddressPath(false, 0, 0, false, 0), false, true);
                TheLabel.Text = $"First Bitcoin Address: {address}";
                TheActivityIndicator.IsRunning = false;
            });
        }
    }
}
