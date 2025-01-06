using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private string weekDayString;
        public string WeekDayString
        {
            get { return weekDayString; }
            set 
            { 
                weekDayString = value;
                OnPropertyChanged(nameof(WeekDayString));
            }
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
        private string time1; 
        public string Time1 { 
            get => time1; 
            set 
            { 
                if (time1 != value) 
                { 
                    time1 = value; 
                    OnPropertyChanged(nameof(Time1)); 
                } 
            } 
        }
        private string time2;
        public string Time2
        {
            get => time2;
            set
            {
                if (time2 != value)
                {
                    time2 = value;
                    OnPropertyChanged(nameof(Time2));
                }
            }
        }
        private string time3;
        public string Time3
        {
            get => time3;
            set
            {
                if (time3 != value)
                {
                    time3 = value;
                    OnPropertyChanged(nameof(Time3));
                }
            }
        }
        private string time4;
        public string Time4
        {
            get => time4;
            set
            {
                if (time4 != value)
                {
                    time4 = value;
                    OnPropertyChanged(nameof(Time4));
                }
            }
        }
        private string time5;
        public string Time5
        {
            get => time5;
            set
            {
                if (time5 != value)
                {
                    time5 = value;
                    OnPropertyChanged(nameof(Time5));
                }
            }
        }

        private string humidity1; 
        public string Humidity1 
        { 
            get => humidity1; 
            set 
            { 
                if (humidity1 != value) 
                { 
                    humidity1 = value; 
                    OnPropertyChanged(nameof(Humidity1)); 
                } 
            } 
        }

        private string humidity2;
        public string Humidity2
        {
            get => humidity2;
            set
            {
                if (humidity2 != value)
                {
                    humidity2 = value;
                    OnPropertyChanged(nameof(Humidity2));
                }
            }
        }
        private string humidity3;
        public string Humidity3
        {
            get => humidity3;
            set
            {
                if (humidity3 != value)
                {
                    humidity3 = value;
                    OnPropertyChanged(nameof(Humidity3));
                }
            }
        }
        private string humidity4;
        public string Humidity4
        {
            get => humidity4;
            set
            {
                if (humidity4 != value)
                {
                    humidity4 = value;
                    OnPropertyChanged(nameof(Humidity4));
                }
            }
        }
        private string humidity5;
        public string Humidity5
        {
            get => humidity5;
            set
            {
                if (humidity5 != value)
                {
                    humidity5 = value;
                    OnPropertyChanged(nameof(Humidity5));
                }
            }
        }
        private string minTemp1; 
        public string MinTemp1
        { 
            get => minTemp1; 
            set 
            { 
                if (minTemp1 != value) 
                { 
                    minTemp1 = value; 
                    OnPropertyChanged(nameof(MinTemp1)); 
                } 
            } 
        }
        private string minTemp2;
        public string MinTemp2
        {
            get => minTemp2;
            set
            {
                if (minTemp2 != value)
                {
                    minTemp2 = value;
                    OnPropertyChanged(nameof(MinTemp2));
                }
            }
        }
        private string minTemp3;
        public string MinTemp3
        {
            get => minTemp3;
            set
            {
                if (minTemp3 != value)
                {
                    minTemp3 = value;
                    OnPropertyChanged(nameof(MinTemp3));
                }
            }
        }
        private string minTemp4;
        public string MinTemp4
        {
            get => minTemp4;
            set
            {
                if (minTemp4 != value)
                {
                    minTemp4 = value;
                    OnPropertyChanged(nameof(MinTemp4));
                }
            }
        }
        private string minTemp5;
        public string MinTemp5
        {
            get => minTemp5;
            set
            {
                if (minTemp5 != value)
                {
                    minTemp5 = value;
                    OnPropertyChanged(nameof(MinTemp5));
                }
            }
        }
        private string maxTemp1; 
        public string MaxTemp1
        { 
            get => maxTemp1; 
            set 
            { 
                if (maxTemp1 != value) 
                { 
                    maxTemp1 = value; 
                    OnPropertyChanged(nameof(MaxTemp1)); 
                } 
            } 
        }
        private string maxTemp2;
        public string MaxTemp2
        {
            get => maxTemp2;
            set
            {
                if (maxTemp2 != value)
                {
                    maxTemp2 = value;
                    OnPropertyChanged(nameof(MaxTemp2));
                }
            }
        }
        private string maxTemp3;
        public string MaxTemp3
        {
            get => maxTemp3;
            set
            {
                if (maxTemp3 != value)
                {
                    maxTemp3 = value;
                    OnPropertyChanged(nameof(MaxTemp3));
                }
            }
        }
        private string maxTemp4;
        public string MaxTemp4
        {
            get => maxTemp4;
            set
            {
                if (maxTemp4 != value)
                {
                    maxTemp4 = value;
                    OnPropertyChanged(nameof(MaxTemp4));
                }
            }
        }
        private string maxTemp5;
        public string MaxTemp5
        {
            get => maxTemp5;
            set
            {
                if (maxTemp5 != value)
                {
                    maxTemp5 = value;
                    OnPropertyChanged(nameof(MaxTemp5));
                }
            }
        }
        public WeatherOverviewViewModel()
        {
            TabNames = new ObservableCollection<string>();
            Locations = new List<LocationModel>();

            Locations.Add(new LocationModel("Emmen", new List<WeatherDataModel> {
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0)
    }));
            Locations.Add(new LocationModel("Amsterdam", new List<WeatherDataModel> {
        new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.CLOUDY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0),
        new WeatherDataModel(WeatherCondition.SUNNY, DateTime.Now, 1.1, 1.1, 2.0)
    }));

            SetTabNames();

            SelectedTab = TabNames.FirstOrDefault();
            WeekDayString = "Week weergave";
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
                            SetImageSource(i, "sunny.png", weatherDataList);
                            break;
                        case WeatherCondition.CLOUDY:
                            SetImageSource(i, "cloudy.png", weatherDataList);
                            break;
                        default:
                            SetImageSource(i, "snow.png", weatherDataList);
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
        private void SetImageSource(int index, string source, List<WeatherDataModel> list)
        {
            switch (index)
            {
                case 0:
                    ImageSource1 = source;
                    Humidity1 = "humidity: " + list[0].Humidity + "%";
                    minTemp1 = "min: " + list[0].MinTemperature + "\u00B0C";
                    MaxTemp1 = "max: " + list[0].MaxTemperature + "\u00B0C";
                    Time1 = list[0].TimeStamp.ToString("HH:mm");
                    break;
                case 1:
                    ImageSource2 = source;
                    Humidity2 = "humidity: " + list[1].Humidity + "%";
                    minTemp2 = "min: " + list[1].MinTemperature + "\u00B0C";
                    MaxTemp2 = "max: " + list[1].MaxTemperature + "\u00B0C";
                    Time2 = list[1].TimeStamp.ToString("HH:mm");
                    break;
                case 2:
                    ImageSource3 = source;
                    Humidity3 = "humidity: " + list[2].Humidity + "%";
                    minTemp3 = "min: " + list[2].MinTemperature + "\u00B0C";
                    MaxTemp3 = "max: " + list[2].MaxTemperature + "\u00B0C";
                    Time3 = list[2].TimeStamp.ToString("HH:mm");
                    break;
                case 3:
                    ImageSource4 = source;
                    Humidity4 = "humidity: " + list[3].Humidity + "%";
                    minTemp4 = "min: " + list[3].MinTemperature + "\u00B0C";
                    MaxTemp4 = "max: " + list[3].MaxTemperature + "\u00B0C";
                    Time4 = list[3].TimeStamp.ToString("HH:mm");
                    break;
                case 4:
                    ImageSource5 = source;
                    Humidity5 = "humidity: " + list[4].Humidity + "%";
                    minTemp5 = "min: " + list[4].MinTemperature + "\u00B0C";
                    MaxTemp5 = "max: " + list[4].MaxTemperature + "\u00B0C";
                    Time5 = list[4].TimeStamp.ToString("HH:mm");
                    break;
            }
        }

        public void OnRightButtonClicked(object sender, EventArgs e)        {
            if (WeekDayString.Equals("Dag overzicht"))
            {
                WeekDayString = "Week overzicht";
            }
            else
            {
                WeekDayString = "Dag overzicht";
            }
        }

        public void OnExportButtonClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
