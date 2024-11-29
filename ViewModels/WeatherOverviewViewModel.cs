using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class WeatherOverviewViewModel
    {
        private DateTime currentDate;
        private List<LocationModel> locations;
        private List<WeatherService> services;
        private bool SimulateMode = true;

        public ICommand ExportCommand { get; }
        public ICommand SettingsCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WeatherOverviewViewModel() 
        {
            this.currentDate = DateTime.Now;
            this.services = [];
            this.locations = [];

            ExportCommand = new Command(Export);
            SettingsCommand = new Command(OpenSettings);
        }

        public async void Start()
        {
            locations.Add(new("Emmen", 52.787701, 6.894810)); //TODO: Needs to be obtained from location manager.

            try
            {
                services.Add(new OpenWeatherMapAPI());
                services.Add(new AccuWeatherAPI());
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error loading API", ex.Message, "OK");
                Debug.WriteLine($"Error loading Accuweather API: {ex.Message}");
            }

            UpdateHourlyData();
        }

        //TPL
        public async Task<APIResponse<List<WeatherDataModel>>[]> FetchWeatherDataAsync(LocationModel location, DateTime date)
        {

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

                var results = await Task.WhenAll(tasks);
                return results; // Return results for further processing
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather data: {ex.Message}");
                return Array.Empty<APIResponse<List<WeatherDataModel>>>(); // Return an empty array on error
            }
        }


        private async Task<Dictionary<int, WeatherDataModel>> UpdateHourlyData()
        {
            var hourlyData = new Dictionary<int, List<WeatherDataModel>>();
            LocationModel location = locations[0]; //Temp hardcoded.
            var results = await FetchWeatherDataAsync(location, currentDate);
            foreach (var result in results)
            {
                if (result.Success)
                {
                    foreach(WeatherDataModel apiData in result.Data)
                    {
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

            var aggregatedData = new Dictionary<int, WeatherDataModel>();
            foreach (var hourEntry in hourlyData)
            {
                int hour = hourEntry.Key;
                var dataList = hourEntry.Value;

                double totalHumidity = 0;
                double minTemperature = 0;
                double maxTemperature = 0;
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

                var aggregatedModel = new WeatherDataModel(
                    aggregatedCondition, // You can adjust how to aggregate the condition.
                    new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, hour, 0, 0), // Set the hour.
                    minTemperature,
                    maxTemperature,
                    averageHumidity
                );

                aggregatedData[hour] = aggregatedModel;
            }
            return aggregatedData;
        }

        public void UpdateGUI()
        {

        }

        public void SetupMap()
        {

        }

        public void Export()
        {
            throw new NotImplementedException();
        }

        public void OpenSettings()
        {
            throw new NotImplementedException();
        }

    }
}
