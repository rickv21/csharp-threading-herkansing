using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace WeatherApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            MapWebView.Source = "map.html";
        }
    }
}
