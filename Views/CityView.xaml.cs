using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using WeatherApp.Models;
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
            SavedLocations =  _viewModel.SavedLocations;
        }

        private void OnItemTapped(object sender, EventArgs e)
        {
            var tappedLocation = ((TappedEventArgs)e).Parameter as LocationModel;
            if (_viewModel.SaveSelectedLocation(tappedLocation) == false)
            {
                DisplayAlert("Informatie", "Deze locatie is al opgeslagen als favoriet!", "OK");
            }
        }
    }
}