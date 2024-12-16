using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class GeocodingAPI : WeatherService
    {
        public GeocodingAPI() : base("Geocoding", "https://api.geoapify.com", 3000, -1)
        {
        }

        public async Task<APIResponse<List<LocationModel>>> GetLocationAsync(string searchQuery, bool simulate = false)
        {
            // Check if the request limit has been reached
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<LocationModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached. Please reset the limit.",
                    Data = null
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("geocoding_location_test.json");  // Simulated response for testing
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}/v1/geocode/search?text={Uri.EscapeDataString(searchQuery)}&format=json&apiKey={_apiKey}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        return new APIResponse<List<LocationModel>>
                        {
                            Success = false,
                            ErrorMessage = $"Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}",
                            Data = null
                        };
                    }

                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            CountRequest();
            var locations = ParseLocations(responseBody);

            return new APIResponse<List<LocationModel>>
            {
                Success = true,
                Data = locations
            };
        }

        private List<LocationModel> ParseLocations(string responseBody)
        {
            var locations = new List<LocationModel>();
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

            foreach (var result in jsonResponse.results)
            {
                string city = result.city != null ? result.city.ToString() : "Unknown City";
                string state = result.state != null ? (string)result.state : "Unknown State";
                string country = result.country != null ? (string)result.country : "Unknown Country";
                string placeId = result.place_id != null ? (string)result.place_id : "Unknown Place ID";
                double latitude = result.lat != null ? (double)result.lat : 0;
                double longitude = result.lon != null ? (double)result.lon : 0;

                var location = new LocationModel(city, state, country, placeId, latitude, longitude);
                locations.Add(location);
            }

            return locations;
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
            return null;
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location, bool simulate = false)
        {
            return null;
        }

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
           return WeatherCondition.UNKNOWN;
        }
    }
}
