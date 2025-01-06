using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using WeatherApp.Utils;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            BindingContext = new SettingsPageViewModel(App.Current.Handler.MauiContext.Services.GetService<WeatherAppData>());
        }
    }
}
