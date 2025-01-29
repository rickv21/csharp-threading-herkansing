using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class SettingsPageViewModel : BindableObject
    {
        private readonly WeatherAppData _weatherAppData;
        public ObservableCollection<WeatherService> WeatherServices { get; set; }

        public SettingsPageViewModel(WeatherAppData weatherAppData)
        {
            _weatherAppData = weatherAppData;
            WeatherServices = new ObservableCollection<WeatherService>(_weatherAppData.WeatherServices.Values);
            SaveCommand = new Command(SaveSettings);
        }

        public ICommand SaveCommand { get; }

        /// <summary>
        /// Save the settings of the application
        /// </summary>
        private async void SaveSettings()
        {
            _weatherAppData.WeatherServices = WeatherServices.ToDictionary(item => item.Name);

            JsonFileManager jsonManager = new();

            foreach (WeatherService service in WeatherServices)
            {
                jsonManager.SetData(service.IsEnabled, "status", service.Name, "enabled");
                Debug.WriteLine(service.Name + " - " + service.IsEnabled);
            }

            await Shell.Current.GoToAsync("///Main");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}