using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private int count = 0;
        public ICommand OpenWeerLiveCommand { get; }
        private LocationModel testLocationModel = new("Emmen", 52.787701, 6.894810);

        public MainViewModel()
        {
            TestAPICommand = new Command(async () => await OnTestButtonClick());
            OpenWeatherMapCommand = new Command(async () => await OnOpenWeatherMapClick());

            AccuWeatherCommand = new Command(async () => await OnAccuWeatherClick());
            IsDay = true;
            SimulateMode = false;
        }

        private bool _simulateMode;
        public bool SimulateMode
        {
            get => _simulateMode;
            set
            {
                _simulateMode = value;
                OnPropertyChanged();
            }
        }

        private bool _isDay;
        private bool _isWeek;

        public bool IsDay
        {
            get => _isDay;
            set
            {
                if (_isDay != value)
                {
                    _isDay = value;
                    if (value) IsWeek = false; // Ensure only one option is selected
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWeek
        {
            get => _isWeek;
            set
            {
                if (_isWeek != value)
                {
                    _isWeek = value;
                    if (value) IsDay = false; // Ensure only one option is selected
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand TestAPICommand { get; }

        private async Task OnTestButtonClick()
        {
            TestAPI api;
            try
            {
                api = new TestAPI();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error loading API", ex.Message, "OK");
                Debug.WriteLine($"Error loading Test API: {ex.Message}");
                return;
            }

            await HandleButtonClick(api);
        }

        public ICommand OpenWeatherMapCommand { get; }

        private async Task OnOpenWeatherMapClick()
        {
            OpenWeatherMapAPI api;
            try
            {
                api = new OpenWeatherMapAPI();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error loading API", ex.Message, "OK");
                Debug.WriteLine($"Error loading Open Weather Map API: {ex.Message}");
                return;
            }

            await HandleButtonClick(api);
        }

        public ICommand AccuWeatherCommand { get; }

        private async Task OnAccuWeatherClick()
        {
            AccuWeatherAPI api;
            try
            {
                api = new AccuWeatherAPI();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error loading API", ex.Message, "OK");
                Debug.WriteLine($"Error loading Accuweather API: {ex.Message}");
                return;
            }

            await HandleButtonClick(api);
        }

        private async Task HandleButtonClick(WeatherService api)
        {
            try
            {
                APIResponse<List<WeatherDataModel>> task;

                if (IsDay)
                {
                    task = await api.GetWeatherDataAsync(DateTime.Today, testLocationModel, SimulateMode);
                }
                else
                {
                    task = await api.GetWeatherForAWeekAsync(testLocationModel, SimulateMode);
                }

                Debug.WriteLine("Is success: " + task.Success);
                if (task.Success)
                {
                    Debug.WriteLine(task.Data);
                    //An assertion to throw a exception if Data is null when Success is true, which should never happen.
                    Debug.Assert(task.Data != null, "task.Data should not be null when task.Success is true");

                    if(task.Data.Count == 0)
                    {
                        await Shell.Current.DisplayAlert("Error", "WeatherDataModel list is empty!", "OK");
                    }

                    //OpenWeatherMap only supports showing weather for every 3 hours -.-
                    foreach (var model in task.Data)
                    {
                        Debug.WriteLine("Model loop!");
                        Debug.WriteLine(model.ToString());

                        // Show a simple alert
                        await Shell.Current.DisplayAlert("Weather Condition", model.ToString(), "OK");
                    }
                }
                else
                {
                    Debug.WriteLine(task.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", task.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
            }
        }
        private async void ExecuteOpenWeerLiveCommand()
        {
            WeerLiveAPI api;
            try
            {
                api = new WeerLiveAPI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading WeerLive API: {ex.Message}");
                return;
            }

            try
            {
                APIResponse<List<WeatherDataModel>> task;

                if (IsDay)
                {
                    task = await api.GetWeatherDataAsync(DateTime.Now, "Emmen", SimulateMode);
                }
                else
                {
                    task = await api.GetWeatherForAWeekAsync("Emmen", SimulateMode);
                }

                Debug.WriteLine("Is success: " + task.Success);
                if (task.Success)
                {
                    Debug.WriteLine(task.Data);
                    //An assertion to throw a exception if Data is null when Success is true, which should never happen.
                    Debug.Assert(task.Data != null, "task.Data should not be null when task.Success is true");

                    foreach (var model in task.Data)
                    {
                        Debug.WriteLine("Model loop!");
                        Debug.WriteLine(model.ToString());

                        // Show a simple alert
                        await Shell.Current.DisplayAlert("Weather Condition", model.ToString(), "OK");
                    }
                }

                else
                {
                    Debug.WriteLine(task.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", task.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
            }
        }
    }
}