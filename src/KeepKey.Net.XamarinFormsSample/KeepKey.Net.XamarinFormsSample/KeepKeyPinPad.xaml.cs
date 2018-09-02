using System;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace KeepKey.Net.XamarinFormsSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KeepKeyPinPad
    {
        public event EventHandler OKClicked;

        public string Pin => PinBox.Text;

        public static async Task<string> GetPin()
        {
            var keepKeyPinPad = new KeepKeyPinPad();
            await App.MainNavigationPage.Navigation.PushModalAsync(keepKeyPinPad);
            var taskCompletionSource = new TaskCompletionSource<string>();

            async void CompletedHandler(object s, EventArgs args)
            {
                await App.MainNavigationPage.Navigation.PopModalAsync();

                var currentApp = App.Current as App;

                taskCompletionSource.SetResult(keepKeyPinPad.Pin);
            }

            keepKeyPinPad.OKClicked += CompletedHandler;

            return await taskCompletionSource.Task;
        }

        public KeepKeyPinPad()
        {
            InitializeComponent();
        }

        private void Button1_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "1";
        }

        private void Button2_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "2";
        }

        private void Button3_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "3";
        }

        private void Button4_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "4";
        }

        private void Button5_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "5";
        }

        private void Button6_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "6";
        }

        private void Button7_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "7";
        }

        private void Button8_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "8";
        }

        private void Button9_Clicked(object sender, EventArgs e)
        {
            PinBox.Text += "9";
        }

        private void Backspace_Clicked(object sender, EventArgs e)
        {
            if (PinBox.Text?.Length > 0)
            {
                PinBox.Text = PinBox.Text.Substring(0, PinBox.Text.Length - 1);
            }
        }

        private void Enter_Clicked(object sender, EventArgs e)
        {
            if (PinBox.Text == null || PinBox.Text.Length < 1)
            {
                return;
            }

            OKClicked?.Invoke(this, e);
        }
    }
}