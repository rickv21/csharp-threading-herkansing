using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.WeatherAPIs
{
    public class APIResponse<T> {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; } }
}
