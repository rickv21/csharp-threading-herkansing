using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Weather Overview screen. 
    /// </summary>
    public class WeatherOverviewViewModel
    {
        private DateTime currentDate; // Tracks the current date for fetching weather data.
        private List<LocationModel> locations; // List of locations to fetch weather data for.
        private List<WeatherService> services; // List of weather API services.
        private bool SimulateMode = true; // Simulate mode for testing purposes.

        private bool _isLoading;
        /// <summary>
        /// Indicates if the data is currently being loaded. Used for showing a loading indicator.
        /// </summary>
        public bool IsLoading 
        { 
            get => _isLoading;
            set {
                _isLoading = value;
                OnPropertyChanged(); 
            } 
        }

        private ObservableCollection<WeatherDisplayItem> _weatherItems;
        /// <summary>
        /// Collection of weather items to display in the UI.
        /// </summary>
        public ObservableCollection<WeatherDisplayItem> WeatherItems
        {
            get => _weatherItems;
            set
            {
                _weatherItems = value;
                OnPropertyChanged();
            }
        }

        // Commands for UI interaction
        public ICommand ExportCommand { get; }
        public ICommand SettingsCommand { get; }

        // Event for notifying UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Debug.WriteLine($"Property {propertyName} changed.");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes the ViewModel, sets default values, and starts loading data.
        /// </summary>
        public WeatherOverviewViewModel() 
        {
            this.currentDate = DateTime.Now;
            this.services = [];
            this.locations = [];
            this.WeatherItems = new ObservableCollection<WeatherDisplayItem>();

            ExportCommand = new Command(Export);
            SettingsCommand = new Command(OpenSettings);

            Start();
        }

        /// <summary>
        /// Starts loading services, locations, and updating the UI.
        /// </summary>
        public async void Start()
        {
            MainThread.BeginInvokeOnMainThread(() => IsLoading = true);

            // Add a default location for testing purposes
            locations.Add(new("Emmen", 52.787701, 6.894810)); //TODO: Needs to be obtained from location manager.

            try
            {
                // Add supported weather services
                services.Add(new OpenWeatherMapAPI());
                services.Add(new AccuWeatherAPI());
                services.Add(new WeerLiveAPI());
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error loading API", ex.Message, "OK");
                Debug.WriteLine($"Error loading API: {ex}");
            }

            // Update the GUI and setup the map
            await UpdateGUI();
            await SetupMap();
            MainThread.BeginInvokeOnMainThread(() => IsLoading = false);
        }

        /// <summary>
        /// Fetches weather data asynchronously from all available weather services for a specified location and date.
        /// </summary>
        /// <param name="location">The location for which weather data is to be fetched.</param>
        /// <param name="date">The date for which weather data is required.</param>
        /// <returns>
        /// An array of <see cref="APIResponse{T}"/> objects, each containing weather data from one of the available services.
        /// Returns an empty array if no services are available or an error occurs.
        /// </returns>
        /// <remarks>
        /// ### **Task Parallel Library (TPL) Overview**
        /// The TPL is a .NET framework for managing and executing parallel and asynchronous code using the **Task** abstraction.  
        /// It enables developers to perform concurrent operations efficiently by leveraging:
        /// 1. **Thread Pool Management**: Tasks are queued to the thread pool, avoiding the overhead of manually creating and destroying threads.  
        /// 2. **Concurrency Without Blocking**: Tasks allow asynchronous execution, keeping threads free for other operations.  
        /// 3. **Simplified Syntax and Error Handling**: TPL simplifies managing complex asynchronous workflows using features like continuations, `Task.WhenAll`, and `Task.WhenAny`.  
        ///
        /// #### **Why TPL is Used in This Method**
        /// In this method, TPL is used to concurrently fetch weather data from multiple weather services:
        /// - Each weather service implements an asynchronous method (`GetWeatherDataAsync`).
        /// - These methods are invoked concurrently using TPL, ensuring efficient resource utilization and reduced response time compared to sequential processing.  
        ///</remarks>
        public async Task<APIResponse<List<WeatherDataModel>>[]> FetchWeatherDataAsync(LocationModel location, DateTime date)
        {
            Debug.WriteLine($"Fetching weather data for {location.Name} on {date}");
            if (services == null || services.Count == 0)
            {
                Debug.WriteLine("No services available.");
                return Array.Empty<APIResponse<List<WeatherDataModel>>>(); // Return an empty array
            }

            try
            {
                // Fetch weather data from all services concurrently
                var tasks = services.Select(service =>
                    service.GetWeatherDataAsync(date, location, SimulateMode)
                );

                Debug.WriteLine("Fetching weather data from services...");
                var results = await Task.WhenAll(tasks);
                return results; // Return results for further processing
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather data: {ex.Message}");
                return Array.Empty<APIResponse<List<WeatherDataModel>>>(); // Return an empty array on error
            }
        }

        /// <summary>
        /// Aggregates weather data by hour and processes it for display.
        /// </summary>
        /// <returns>Dictionary of aggregated weather data indexed by hour.</returns>
        private async Task<Dictionary<int, WeatherDataModel>> UpdateHourlyData()
        {
            var hourlyData = new Dictionary<int, List<WeatherDataModel>>();
            LocationModel location = locations[0]; //Temp hardcoded.
            var results = await FetchWeatherDataAsync(location, currentDate);
            foreach (var result in results)
            {
                if (result.Success)
                {
                    if(result.Data.Count == 0)
                    {
                        Debug.WriteLine("Data is empty!");
                        await Shell.Current.DisplayAlert("Error", "Data is empty!", "OK");
                    }
                    foreach (WeatherDataModel apiData in result.Data)
                    {
                        Debug.WriteLine(apiData.ToString());
                        int hour = apiData.TimeStamp.Hour;
                        if (!hourlyData.ContainsKey(hour))
                        {
                            hourlyData[hour] = new List<WeatherDataModel>();
                        }

                        hourlyData[hour].Add(apiData);
                    }
                }
                else
                {
                    Debug.WriteLine(result.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", result.ErrorMessage, "OK");
                }
            }

            var aggregatedData = hourlyData.ToDictionary(hourEntry => hourEntry.Key, hourEntry =>
            {
                int hour = hourEntry.Key;
                var dataList = hourEntry.Value;

                double totalHumidity = -1;
                double minTemperature = double.MaxValue; 
                double maxTemperature = double.MinValue;
                int validHumidityCount = 0;
                WeatherCondition aggregatedCondition = WeatherCondition.UNKNOWN; // Default value

                foreach (var data in dataList)
                {
                    // Aggregate humidity, excluding -1 values.
                    if (data.Humidity != -1)
                    {
                        totalHumidity += data.Humidity;
                        validHumidityCount++;
                    }

                    // Aggregate temperatures.
                    minTemperature = Math.Min(minTemperature, data.MinTemperature);
                    maxTemperature = Math.Max(maxTemperature, data.MaxTemperature);

                    if (aggregatedCondition == WeatherCondition.UNKNOWN)
                    {
                        aggregatedCondition = data.Condition;
                    }
                }

                double averageHumidity = validHumidityCount > 0 ? totalHumidity / validHumidityCount : 0;

                return new WeatherDataModel(
                    aggregatedCondition,
                    new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, hour, 0, 0), // Set the hour.
                    minTemperature,
                    maxTemperature,
                    averageHumidity
                );
            }
         );

            var sortedAggregatedData = aggregatedData.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return sortedAggregatedData;
        }

        /// <summary>
        /// Updates the UI with aggregated weather data.
        /// </summary>
        public async Task UpdateGUI()
        {
            var data = await UpdateHourlyData();
            WeatherItems.Clear();
            foreach (var entry in data)
            {
                var model = entry.Value;
                var weatherItem = new WeatherDisplayItem(GetWeatherIcon(model.Condition), $"{entry.Key}:00 - {model.ToString()}");
                Debug.WriteLine("GUI - " + model.ToString());
                WeatherItems.Add(weatherItem);
            }
        }

        /// <summary>
        /// Fetches the appropriate weather icon for a condition.
        /// </summary>
        /// <param name="condition">The weather condition.</param>
        /// <returns>An ImageSource for the icon.</returns>
        public ImageSource GetWeatherIcon(WeatherCondition condition)
        {
            string iconName = condition.ToString().ToLower();
            string iconPath = $"WeatherApp.Resources.Images.Weather.{iconName}.png";
            var assembly = GetType().Assembly;

            try
            {
                using (var stream = assembly.GetManifestResourceStream(iconPath))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine("Icon not found, using fallback.");
                        return ImageSource.FromResource("WeatherApp.Resources.Images.Weather.unknown.png", assembly);
                    }

                    return ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred while fetching the icon: {ex.Message}. Using fallback image.");
                return ImageSource.FromResource("WeatherApp.Resources.Images.Weather.unknown.png", assembly);
            }
        }

        /// <summary>
        /// Sets up the map interface.
        /// </summary>
        public async Task SetupMap()
        {
            //TODO
        }

        /// <summary>
        /// Handles exporting data.
        /// </summary>
        public void Export()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens the settings page.
        /// </summary>
        public void OpenSettings()
        {
            throw new NotImplementedException();
        }

    }
}
