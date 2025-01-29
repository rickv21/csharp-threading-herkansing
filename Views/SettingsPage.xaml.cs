using WeatherApp.Utils;
using WeatherApp.ViewModels;

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
