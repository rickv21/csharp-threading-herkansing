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

        /// <summary>
        /// The searchQuery an user enters
        /// </summary>
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

        /// <summary>
        /// Remove a location from favorites
        /// </summary>
        /// <param name="location">The location to be removed</param>
        private void RemoveLocation(LocationModel location)
        {
            if (SavedLocations.Contains(location))
            {
                SavedLocations.Remove(location);
                UpdatePlacesJson();
            }
        }

        /// <summary>
        /// Update the JSON file with favorite places
        /// </summary>
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

        /// <summary>
        /// Get the file path of the places.json file
        /// </summary>
        /// <returns>The file path</returns>
        private string GetFilePath()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\..\")); // Go up to the solution directory
            string testDataPath = Path.Combine(solutionDirectory, "TestData");
            string filePath = Path.Combine(testDataPath, "places.json");

            return filePath;
        }

        /// <summary>
        /// Perform a search request on the Geocoding API
        /// </summary>
        /// <returns>A task representing the async operation</returns>
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

        /// <summary>
        /// Load the saved locations from the places.json
        /// </summary>
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

        /// <summary>
        /// Load the locations from the places.json
        /// </summary>
        /// <param name="filePath">The file path of places.json</param>
        /// <returns>A list of favorited locations</returns>
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

        /// <summary>
        /// Save the selected location to places.json
        /// </summary>
        /// <param name="selectedLocation">The selected location</param>
        /// <returns>True if saved, false if something went wrong</returns>
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