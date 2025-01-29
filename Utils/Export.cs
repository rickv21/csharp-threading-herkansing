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

            // Creates folder 'ExportWeatherDat' if it doesnt exists
            if (!Directory.Exists(_exportFolder))
            {
                Directory.CreateDirectory(_exportFolder);
            }
        }

        /// <summary>
        /// Executes export tasks
        /// 
        /// Multithreading: export-method starts 3 threads to export weatherdata to JSON, CSV and TXT-files
        /// export functions are executed in parallel, potentially speeding up processing
        /// 
        /// Based on paragraph 'Multithreading' in: https://stackify.com/c-threading-and-multithreading-a-guide-with-examples/
        /// </summary>
        public async Task ExportWeatherData(List<WeatherDisplayItem> weatherItems, LocationModel selectedLocation, List<LocationModel> locations)
        {
            LocationModel location = selectedLocation ?? locations.First();

            // Filename based on place and datetime
            string timestamp = $"{location}_{DateTime.Now:ddMMyyyy_HHmmss}";

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
        private void ExportToJson(List<WeatherDisplayItem> weatherItems, LocationModel location, string timestamp)
        {
            if (weatherItems == null || weatherItems.Count == 0)
            {
                Debug.WriteLine("Geen weerdata beschikbaar om te exporteren naar JSON.");
                return;
            }

            string filePath = Path.Combine(_exportFolder, $"WeatherData_{timestamp}.json");

            var weatherWithLocation = new
            {
                Location = location.Name,
                WeatherData = weatherItems // Direct de lijst opslaan zonder extra serialisatie
            };

            string jsonWithLocation = JsonSerializer.Serialize(weatherWithLocation, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonWithLocation);
            Debug.WriteLine($"Weather data exported to JSON: {filePath}");
        }


        /// <summary>
        /// Exports weather data to CSV
        /// </summary>
        private void ExportToCsv(List<WeatherDisplayItem> weatherItems, LocationModel location, string timestamp)
        {
            string filePath = Path.Combine(_exportFolder, $"WeatherData_{timestamp}.csv");

            var csvLines = new List<string> { "Plaats;Tijdstip;Weersomstandigheden;Min Temperatuur;Max Temperatuur;Vochtigheid" };

            csvLines.AddRange(weatherItems.Select(item =>
                $"{location.Name};{item.TimeStamp};{item.Condition.Trim()};{GetTemperatureValue(item.MinTemp)};{GetTemperatureValue(item.MaxTemp)};{item.Humidity}"
            ));

            File.WriteAllLines(filePath, csvLines, Encoding.UTF8);
            Debug.WriteLine($"Weather data exported to CSV: {filePath}");
        }

        /// <summary>
        /// Exports weather data to TXT
        /// </summary>
        private void ExportToTxt(List<WeatherDisplayItem> weatherItems, LocationModel location, string timestamp)
        {
            string filePath = Path.Combine(_exportFolder, $"WeatherData_{timestamp}.txt");

            var txtLines = new List<string> { $"Locatie: {location.Name}" };
            txtLines.AddRange(weatherItems.Select(item => item.DisplayText));

            File.WriteAllLines(filePath, txtLines);
            Debug.WriteLine($"Weather data exported to TXT: {filePath}");
        }

        /// <summary>
        /// Trims min and max temperature for CSV export so as example only 8°C is displayed in the csv column
        /// </summary>
        private string GetTemperatureValue(string temperature)
        {
            return System.Text.RegularExpressions.Regex.Replace(temperature, @"[^\d.,-]", "") + " °C";
        }
    }
}
