using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class LocationViewModel
    {
        private readonly GeocodingAPI _api;
        private string _searchQuery;
        private Action<string> _onSearchQueryChanged;

        public ObservableCollection<LocationModel> SearchResults { get; set; }
        public ObservableCollection<LocationModel> SavedLocations { get; set; } = new ObservableCollection<LocationModel>();

        public ICommand SearchCommand { get; }

        public LocationViewModel()
        {
            _api = new GeocodingAPI();
            SavedLocations = new ObservableCollection<LocationModel>();
            SearchResults = new ObservableCollection<LocationModel>();
            SearchCommand = new Command(async () => await PerformSearch());
            LoadSavedLocations();
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    _onSearchQueryChanged?.Invoke(value);
                    SearchCommand.Execute(null);
                }
            }
        }

        private async Task PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;

            var response = await _api.GetLocationAsync(SearchQuery);
            if (response.Success)
            {
                SearchResults.Clear();
                foreach (var result in response.Data)
                {
                    SearchResults.Add(result);
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", response.ErrorMessage, "OK");
            }
        }

        public void LoadSavedLocations()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\..\")); // Go up to the solution directory
            string testDataPath = Path.Combine(solutionDirectory, "TestData");
            string filePath = Path.Combine(testDataPath, "places.json");

            var savedLocations = LoadLocationsFromFile(filePath);

            // Clear existing data and add new saved locations
            SavedLocations.Clear();
            foreach (var location in savedLocations)
            {
                SavedLocations.Add(location);
            }
        }

        private List<LocationModel> LoadLocationsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                Debug.WriteLine(filePath);
                var json = File.ReadAllText(filePath);
                Debug.WriteLine(json);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        // Deserialize the JSON to a List<LocationModel>
                        var locations = JsonConvert.DeserializeObject<List<LocationModel>>(json);
                        return locations ?? new List<LocationModel>();
                    }
                    catch (JsonException ex)
                    {
                        return new List<LocationModel>();
                    }
                }
            }
            return new List<LocationModel>();
        }
    }
}