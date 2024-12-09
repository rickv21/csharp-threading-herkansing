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
                    UpdateImageSources();
                }
            }
        }

        private List<LocationModel> _locations;
        public List<LocationModel> Locations
        {
            set => _locations = value;
            get => _locations;
        }

        private string imageSource1;
        public string ImageSource1
        {
            get => imageSource1;
            set
            {
                if (imageSource1 != value)
                {
                    imageSource1 = value;
                    OnPropertyChanged(nameof(ImageSource1));
                }
            }
        }

        private string imageSource2;
        public string ImageSource2
        {
            get => imageSource2;
            set
            {
                if (imageSource2 != value)
                {
                    imageSource2 = value;
                    OnPropertyChanged(nameof(ImageSource2));
                }
            }
        }

        private string imageSource3;
        public string ImageSource3
        {
            get => imageSource3;
            set
            {
                if (imageSource3 != value)
                {
                    imageSource3 = value;
                    OnPropertyChanged(nameof(ImageSource3));
                }
            }
        }

        private string imageSource4;
        public string ImageSource4
        {
            get => imageSource4;
            set
            {
                if (imageSource4 != value)
                {
                    imageSource4 = value;
                    OnPropertyChanged(nameof(ImageSource4));
                }
            }
        }

        private string imageSource5;
        public string ImageSource5
        {
            get => imageSource5;
            set
            {
                if (imageSource5 != value)
                {
                    imageSource5 = value;
                    OnPropertyChanged(nameof(ImageSource5));
                }
            }
        }

        public WeatherOverviewViewModel()
        {
            TabNames = new ObservableCollection<string>();
            Locations = new List<LocationModel>();

            // Sample data
            Locations.Add(new LocationModel("Emmen", new List<WeatherDataModel> {
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0)
    }));
            Locations.Add(new LocationModel("Damsko", new List<WeatherDataModel> {
        new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0)
    }));

            SetTabNames();

            // Set the first tab as the selected tab
            SelectedTab = TabNames.FirstOrDefault();

            // Update the image sources to reflect the initial tab
        }
        private void SetTabNames()
        {
            foreach (var location in Locations)
            {
                TabNames.Add(location.Name);
            }
        }

        private void UpdateImageSources()
        {
            // Reset image sources to default
            ImageSource1 = "default.png";
            ImageSource2 = "default.png";
            ImageSource3 = "default.png";
            ImageSource4 = "default.png";
            ImageSource5 = "default.png";

            Console.WriteLine($"SelectedTab: {SelectedTab}");

            LocationModel selectedLocation = Locations.FirstOrDefault(location => location.Name == SelectedTab);
            if (selectedLocation != null)
            {
                // Iterate through the WeatherData and update the image sources
                var weatherDataList = selectedLocation.WeatherData.Take(5).ToList(); // Take up to 5 weather data entries
                for (int i = 0; i < weatherDataList.Count; i++)
                {
                    switch (weatherDataList[i].Condition)
                    {
                        case WeatherCondition.SUNNY:
                            SetImageSource(i, "sunny.png");
                            break;
                        case WeatherCondition.CLOUDY:
                            SetImageSource(i, "cloudy.png");
                            break;
                        default:
                            SetImageSource(i, "snow.png");
                            break;
                    }
                    Console.WriteLine($"ImageSource{i + 1}: sunny.png");
                }
            }
            else
            {
                Console.WriteLine("No selected location found.");
            }
        }


        // Helper method to update the appropriate image source
        private void SetImageSource(int index, string source)
        {
            switch (index)
            {
                case 0:
                    ImageSource1 = source;
                    break;
                case 1:
                    ImageSource2 = source;
                    break;
                case 2:
                    ImageSource3 = source;
                    break;
                case 3:
                    ImageSource4 = source;
                    break;
                case 4:
                    ImageSource5 = source;
                    break;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
