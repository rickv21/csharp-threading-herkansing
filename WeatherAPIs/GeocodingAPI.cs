using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class GeocodingAPI : APIService
    {
        public GeocodingAPI() : base("Geocoding", "https://api.geoapify.com", 3000, -1)
        {
        }

        /// <summary>
        /// Retrieve locations from the API
        /// </summary>
        /// <param name="searchQuery">The user-inputted search query</param>
        /// <returns>A task with an APIResponse which contains a list of LocationModels</returns>
        public async Task<APIResponse<List<LocationModel>>> GetLocationAsync(string searchQuery)
        {
            // Check if the request limit has been reached
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<LocationModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached. Please reset the limit.",
                    Source = Name
                };
            }

            string responseBody;

            using (HttpClient client = new())
            {
                string url = $"{_baseURL}/v1/geocode/search?filter=countrycode:nl&text={Uri.EscapeDataString(searchQuery)}&format=json&lang=nl&apiKey={_apiKey}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new APIResponse<List<LocationModel>>
                    {
                        Success = false,
                        ErrorMessage = $"Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}",
                        Source = Name
                    };
                }

                responseBody = await response.Content.ReadAsStringAsync();
            }

            var locations = ParseLocations(responseBody);

            return new APIResponse<List<LocationModel>>
            {
                Success = true,
                Data = locations,
                Source = Name
            };
        }

        /// <summary>
        /// Parse the locations retrieved from the API
        /// </summary>
        /// <param name="responseBody">The string with the location data</param>
        /// <returns>A list with LocationModels</returns>
        private List<LocationModel> ParseLocations(string responseBody)
        {
            var locations = new List<LocationModel>();
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

            foreach (var result in jsonResponse.results)
            {
                string city = result.city != null ? result.city.ToString() : "Onbekend";
                string state = result.state != null ? (string)result.state : "Onbekend";
                string country = result.country != null ? (string)result.country : "Onbekend";
                string placeId = result.place_id != null ? (string)result.place_id : "Onbekend";
                double latitude = result.lat != null ? (double)result.lat : 0;
                double longitude = result.lon != null ? (double)result.lon : 0;

                var location = new LocationModel(city, state, country, placeId, latitude, longitude, null);
                locations.Add(location);
            }

            return locations;
        }
    }
}
