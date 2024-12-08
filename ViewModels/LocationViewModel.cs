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
        private CancellationTokenSource _debounceCts;

        public ObservableCollection<LocationModel> SearchResults { get; set; }
        public ObservableCollection<LocationModel> SavedLocations { get; set; } = new ObservableCollection<LocationModel>();

        public ICommand SearchCommand { get; }

        public ICommand RemoveLocationCommand { get; }

        public LocationViewModel()
        {
            _api = new GeocodingAPI();
            RemoveLocationCommand = new Command<LocationModel>(RemoveLocation);
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

        private void RemoveLocation(LocationModel location)
        {
            if (SavedLocations.Contains(location))
            {
                SavedLocations.Remove(location);
                UpdatePlacesJson();
            }
        }

        private void UpdatePlacesJson()
        {
            try
            {
                if (File.Exists(GetFilePath()))
                {
                    string json = JsonConvert.SerializeObject(SavedLocations.ToList(), Formatting.Indented);
                    File.WriteAllText(GetFilePath(), json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update places.json: {ex.Message}");
            }
        }

        private string GetFilePath()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\..\")); // Go up to the solution directory
            string testDataPath = Path.Combine(solutionDirectory, "TestData");
            string filePath = Path.Combine(testDataPath, "places.json");

            return filePath;
        }

        private async Task PerformSearch()
        {
            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts.Dispose();
            }

            _debounceCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(1500, _debounceCts.Token);
                if (string.IsNullOrWhiteSpace(SearchQuery)) return;
                
                if (SearchQuery.Length > 2)
                {
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
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled due to a new keystroke
            }
        }

        public void LoadSavedLocations()
        {
            var savedLocations = LoadLocationsFromFile(GetFilePath());

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

        public bool SaveSelectedLocation(LocationModel selectedLocation)
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
                        catch (JsonException)
                        {
                            existingLocations = new List<LocationModel>();
                        }
                    }
                }

                bool locationExists = existingLocations.Any(loc =>
                    loc.Latitude == selectedLocation.Latitude && loc.Longitude == selectedLocation.Longitude);

                if (locationExists)
                {
                    return false;
                }

                SavedLocations.Add(selectedLocation);
                existingLocations.Add(selectedLocation);
                string json = JsonConvert.SerializeObject(existingLocations, Formatting.Indented);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}