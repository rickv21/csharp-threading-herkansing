using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Utils
{
    /// <summary>
    /// This class manages to export weatherdata
    /// </summary>
    internal class Export
    {
        private readonly string _exportFolder;

        public Export()
        {
            // Location where export documents get saved
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _exportFolder = Path.Combine(documentsFolder, "ExportWeatherData");

            // Creates folder 'ExportWeatherData' if it doesnt exists
            if (!Directory.Exists(_exportFolder))
            {
                Directory.CreateDirectory(_exportFolder);
            }
        }

        /// <summary>
        /// Executes export tasks
        /// </summary>
        /// <remarks>
        /// ### ** Multithreading **
        /// Export-method starts 3 threads to export weatherdata to JSON, CSV and TXT-files
        /// Export functions are executed in parallel, potentially speeding up processing
        /// Based on paragraph 'Multithreading' in: https://stackify.com/c-threading-and-multithreading-a-guide-with-examples/
        /// </remarks>
        public async Task ExportWeatherData(List<WeatherDisplayModel> weatherItems, LocationModel selectedLocation, List<LocationModel> locations)
        {
            LocationModel location = selectedLocation ?? locations.First();

            // Filename based on place and datetime
            string timestamp = $"{location}_{DateTime.Now:ddMMyyyy_HHmmss}";

            //Start the threads
            await Task.WhenAll(
                Task.Run(() => ExportToJson(weatherItems, location, timestamp)),
                Task.Run(() => ExportToCsv(weatherItems, location, timestamp)),
                Task.Run(() => ExportToTxt(weatherItems, location, timestamp))
            );

            await Shell.Current.DisplayAlert("Export Succesvol", $"Bestanden opgeslagen in: {_exportFolder}", "OK");
        }

        /// <summary>
        /// Exports weather data to JSON
        /// </summary>
        private void ExportToJson(List<WeatherDisplayModel> weatherItems, LocationModel location, string timestamp)
        {
            if (weatherItems == null || weatherItems.Count == 0)
            {
                return;
            }

            string filePath = Path.Combine(_exportFolder, $"WeatherData_{timestamp}.json");

            var weatherWithLocation = new
            {
                Location = location.Name,
                WeatherData = weatherItems.Select(item => new
                {
                    item.WeatherData.Condition,
                    item.WeatherData.ConditionFormatted,
                    item.WeatherData.TimeStamp,
                    item.WeatherData.MinTemperature,
                    item.WeatherData.MaxTemperature,
                    item.WeatherData.Humidity,
                    item.LocalizedName,
                    item.DisplayText
                })
            };

            string jsonWithLocation = JsonSerializer.Serialize(weatherWithLocation, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonWithLocation);
        }


        /// <summary>
        /// Exports weather data to CSV
        /// </summary>
        private void ExportToCsv(List<WeatherDisplayModel> weatherItems, LocationModel location, string timestamp)
        {
            string filePath = Path.Combine(_exportFolder, $"WeatherData_{timestamp}.csv");

            var csvLines = new List<string> { "Plaats;Tijdstip;Weersomstandigheden;Vertaalde weersomstandigheden;Min Temperatuur;Max Temperatuur;Vochtigheid" };

            csvLines.AddRange(weatherItems.Select(item =>
                $"{location.Name};{item.WeatherData.TimeStamp};{item.WeatherData.Condition};{item.WeatherData.ConditionFormatted};{(item.WeatherData.MinTemperature)};{(item.WeatherData.MaxTemperature)};{item.WeatherData.Humidity}"
            ));

            File.WriteAllLines(filePath, csvLines, Encoding.UTF8);
        }

        /// <summary>
        /// Exports weather data to TXT
        /// </summary>
        private void ExportToTxt(List<WeatherDisplayModel> weatherItems, LocationModel location, string timestamp)
        {
            string filePath = Path.Combine(_exportFolder, $"WeatherData_{timestamp}.txt");

            var txtLines = new List<string> { $"Locatie: {location.Name}" };
            txtLines.AddRange(weatherItems.Select(item => item.DisplayText));

            File.WriteAllLines(filePath, txtLines);
        }
    }
}
