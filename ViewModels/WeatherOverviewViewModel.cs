using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    internal class WeatherOverviewViewModel
    {
        private List<LocationModel> _locations;
        public List<LocationModel> Locations
        {
            get { return _locations; }
            set { _locations = value; }
        }
        private List<WeatherService> _weatherServices;
        public List<WeatherService> WeatherServices
        {
            get { return _weatherServices; }
            set { _weatherServices = value; }
        }
        public ObservableCollection<string> TabNames { get; } = new ObservableCollection<string>();

        public WeatherOverviewViewModel()
        {
            TabNames.Add("1");
            TabNames.Add("111111111111111111111111111111111111111111111");
            TabNames.Add("3");
        }
        private void SetTabs()
        {
            foreach (var location in Locations)
            {
                TabNames.Add(location.Name);
            }
        }
    }
}
