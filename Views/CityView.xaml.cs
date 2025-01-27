using System.Collections.ObjectModel;
using WeatherApp.Models;
using WeatherApp.Utils;
using WeatherApp.ViewModels;

namespace WeatherApp.Views
{
    public partial class CityView : ContentPage
    {
        private static LocationViewModel _viewModel;
        public ObservableCollection<LocationModel> SavedLocations { get; set; }

        public CityView()
        {
            InitializeComponent();
            _viewModel = new LocationViewModel();
            BindingContext = _viewModel;
            SavedLocations = _viewModel.SavedLocations;
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            var tappedLocation = ((TappedEventArgs)e).Parameter as LocationModel;
            var result = _viewModel.SaveSelectedLocation(tappedLocation);

            switch (result)
            {
                case SaveLocationResult.DuplicateLocation:
                    await DisplayAlert("Informatie", "Deze locatie is al opgeslagen als favoriet!", "OK");
                    break;
                case SaveLocationResult.FavoriteLimitReached:
                    await DisplayAlert("Informatie", "Je hebt het maximum aantal favorieten van 5 bereikt!", "OK");
                    break;
                case SaveLocationResult.Success:
                    await DisplayAlert("Informatie", "Locatie succesvol toegevoegd aan favorieten!", "OK");
                    break;
            }
        }
    }
}