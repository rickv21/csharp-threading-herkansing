using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace WeatherApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnNavigateButtonClicked(object sender, EventArgs e)
        {
            // Navigeer naar overview
            await Navigation.PushAsync(new WeatherOverviewView());
        }
    }
}
