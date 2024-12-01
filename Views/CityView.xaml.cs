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
            var tappedLocation = (LocationModel)((TappedEventArgs)e).Parameter;
            SaveSelectedLocation(tappedLocation);
        }

        // Method to save the location to the places.json file
        private void SaveSelectedLocation(LocationModel selectedLocation)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\..\")); // Go up to the solution directory
                string testDataPath = Path.Combine(solutionDirectory, "TestData");

                if (!Directory.Exists(testDataPath))
                {
                    Directory.CreateDirectory(testDataPath);
                }

                string filePath = Path.Combine(testDataPath, "places.json");
                List<LocationModel> existingLocations = new List<LocationModel>();

                if (File.Exists(filePath))
                {
                    string existingJson = File.ReadAllText(filePath);

                    if (!string.IsNullOrEmpty(existingJson))
                    {
                        try
                        {
                            existingLocations = JsonConvert.DeserializeObject<List<LocationModel>>(existingJson) ?? new List<LocationModel>();
                        }
                        catch (JsonException ex)
                        {
                            existingLocations = new List<LocationModel>();
                        }
                    }
                }

                bool locationExists = existingLocations.Any(loc =>
                    loc.Latitude == selectedLocation.Latitude && loc.Longitude == selectedLocation.Longitude);

                if (locationExists)
                {
                    DisplayAlert("Info", "This location is already saved!", "OK");
                    return;
                }

                _viewModel.SavedLocations.Add(selectedLocation);
                existingLocations.Add(selectedLocation);
                string json = JsonConvert.SerializeObject(existingLocations, Formatting.Indented);
                File.WriteAllText(filePath, json);
                DisplayAlert("Saved", "Location saved successfully!", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to save location: {ex.Message}", "OK");
            }
        }
    }
}