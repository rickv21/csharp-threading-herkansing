using WeatherApp.Utils;
using WeatherApp.ViewModels;

namespace WeatherApp.Views
{
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            InitializeComponent();

            BindingContext = new TestViewModel(App.Current.Handler.MauiContext.Services.GetService<WeatherAppData>());
        }
    }
}
