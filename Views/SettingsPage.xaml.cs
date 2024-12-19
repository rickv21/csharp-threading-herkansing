using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage(List<WeatherService> weatherServices, Dictionary<int, List<Models.WeatherDataModel>> hourlyData, bool simulateMode)
        {
            InitializeComponent();

            BindingContext = new SettingsPageViewModel(weatherServices, hourlyData, simulateMode);
        }
    }
}
