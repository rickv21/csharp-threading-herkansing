using Newtonsoft.Json;
using System.Collections.ObjectModel;
using WeatherApp.Models;
using WeatherApp.ViewModels;

namespace WeatherApp.Views
{
    public partial class CityView : ContentPage
    {
        private LocationViewModel _viewModel;
        public ObservableCollection<LocationModel> SavedLocations { get; set; } = new ObservableCollection<LocationModel>();

        public CityView()
        {
            InitializeComponent();
            _viewModel = new LocationViewModel();
            BindingContext = _viewModel;
        }

        private void OnItemTapped(object sender, EventArgs e)
        {
            var tappedLocation = ((TappedEventArgs)e).Parameter as LocationModel;
            _ = _viewModel.SaveSelectedLocation(tappedLocation);
        }
    }
}