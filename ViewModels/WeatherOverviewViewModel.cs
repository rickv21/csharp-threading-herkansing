using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    internal class WeatherOverviewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> TabNames { get; set; }

        private string selectedTab;
        public string SelectedTab
        {
            get => selectedTab;
            set
            {
                if (selectedTab != value)
                {
                    selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));
                    UpdateImageSource();
                }
            }
        }

        private List<LocationModel> _locations;
        public List<LocationModel> Locations
        {
            set => _locations = value;
            get => _locations;
        }

        private string imageSource;
        public string ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public WeatherOverviewViewModel()
        {
            TabNames = new ObservableCollection<string>();
            Locations = new List<LocationModel>();
            Locations.Add(new LocationModel("Emmen", new List<WeatherDataModel> { 
                new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0) 
            }));
            Locations.Add(new LocationModel("Damsko", new List<WeatherDataModel> {
                new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0)
            }));

            SetTabNames();

            SelectedTab = TabNames.FirstOrDefault();
        }
        private void SetTabNames()
        {
            foreach (var location in Locations)
            {
                TabNames.Add(location.Name);
            }
        }

        private void UpdateImageSource()
        {

            LocationModel selectedLocation = null;
            foreach (var location in Locations)
            {
                if(location.Name == SelectedTab)
                {
                    selectedLocation = location;
                    break;
                }
            }
            if (selectedLocation != null)
            {
                switch (selectedLocation.WeatherData.First().Condition)
                {
                    case WeatherCondition.SUNNY:
                        ImageSource = "sunny.png";
                        break;
                    case WeatherCondition.CLOUDY:
                        ImageSource = "cloudy.png";
                        break;
                    default:
                        ImageSource = "snow.png";
                        break;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
