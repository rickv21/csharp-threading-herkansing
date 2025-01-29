using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.Utils
{
    public class PlacesManager
    {
        JsonFileManager jsonFileManager = new JsonFileManager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "places.json"));

        /// <summary>
        /// Update the JSON file with favorite places
        /// </summary>
        public void UpdatePlacesJson(ObservableCollection<LocationModel> savedLocations)
        {
            try
            {
                JObject locationsObject = [];
                foreach (var location in savedLocations)
                {
                    string placeId = location.PlaceId;
                    locationsObject[placeId] = JObject.FromObject(new
                    {
                        location.Name,
                        location.Latitude,
                        location.Longitude,
                        location.Country,
                        location.State,
                        WeatherData = location.WeatherData != null ? JArray.FromObject(location.WeatherData) : []
                    });
                }

                jsonFileManager.SetData(locationsObject, ["Locations"]);
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Load the locations from the places.json
        /// </summary>
        /// <param name="filePath">The file path of places.json</param>
        /// <returns>A list of favorited locations.</returns>
        public List<LocationModel> LoadLocationsFromFile()
        {
            JObject locationsObject = (jsonFileManager.GetData("Locations") as JObject) ?? new JObject();

            return locationsObject.Properties()
                .Select(prop =>
                {
                    var location = prop.Value.ToObject<LocationModel>(); // Deserialize
                    if (location != null)
                    {
                        location.PlaceId = prop.Name;
                        Debug.WriteLine("NAME: " + location.PlaceId);
                    }
                    return location;
                })
                .Where(location => location != null && !string.IsNullOrEmpty(location!.Name))
                .Select(location => location!)
                .ToList();
        }


        /// <summary>
        /// Save the selected location to places.json
        /// </summary>
        /// <param name="selectedLocation">The selected location</param>
        /// <returns>True if saved, false if the location already exists</returns>
        public SaveLocationResult SaveLocation(LocationModel location)
        {
            JObject locationsObject = (JObject)jsonFileManager.GetData(["Locations"]) ?? [];

            string placeId = location.PlaceId;

            // Check if the location already exists
            if (IsLocationDuplicate(locationsObject, location))
            {
                return SaveLocationResult.DuplicateLocation;
            }

            var locationObject = new JObject
            {
                ["Name"] = location.Name,
                ["Latitude"] = location.Latitude,
                ["Longitude"] = location.Longitude,
                ["Country"] = location.Country,
                ["State"] = location.State,
                ["WeatherData"] = null
            };

            // Add the location with place_id as the key
            locationsObject[placeId] = locationObject;
            jsonFileManager.SetData(locationsObject, ["Locations"]);

            return SaveLocationResult.Success;
        }

        /// <summary>
        /// Check if a location has already been added as a favorite
        /// </summary>
        /// <param name="locationsToken">The locations token containing all saved locations</param>
        /// <param name="selectedLocation">The location to be added</param>
        /// <returns></returns>
        public bool IsLocationDuplicate(JObject locationsToken, LocationModel selectedLocation)
        {
            return locationsToken.Properties().Any(prop =>
            {
                var loc = prop.Value as JObject;
                var latitude = loc?["Latitude"]?.Value<double>();
                var longitude = loc?["Longitude"]?.Value<double>();

                return latitude == selectedLocation.Latitude && longitude == selectedLocation.Longitude;
            });
        }
    }
}
