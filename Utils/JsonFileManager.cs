using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Utils
{
    /// <summary>
    /// This class manages JSON data stored in a file.
    /// </summary>
    public class JsonFileManager
    {
        private readonly string _filePath;

        public JsonFileManager()
        {
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "weatherAppData.json");
        }

        /// <summary>
        /// Gets all JSON data from the file.
        /// </summary>
        /// <returns>A <see cref="JObject"/> representing the JSON data.</returns>
        public JObject GetAllJson()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                return JObject.Parse(json);
            }

            return new JObject();
        }

        /// <summary>
        /// Saves all JSON data to the file.
        /// </summary>
        /// <param name="data">The <see cref="JObject"/> representing the JSON data to save.</param>
        public void SaveAllJson(JObject data)
        {
            string json = data.ToString();
            File.WriteAllText(_filePath, json);
        }

        /// <summary>
        /// Gets a section of the JSON data as a class object.
        /// </summary>
        /// <typeparam name="T">The type of the class to convert the JSON data to.</typeparam>
        /// <param name="key">The key of the section to get.</param>
        /// <returns>An object of type <typeparamref name="T"/> representing the JSON data, or null if the section does not exist.</returns>
        public T? GetSectionAsClass<T>(string key) where T : class
        {
            JObject root = GetAllJson();
            return root[key]?.ToObject<T>();
        }

        /// <summary>
        /// Sets a section of the JSON data.
        /// </summary>
        /// <typeparam name="T">The type of the value to set in the JSON data.</typeparam>
        /// <param name="key">The key of the section to set.</param>
        /// <param name="value">The value to set in the JSON data.</param>
        public void SetSection<T>(string key, T value)
        {
            JObject root = GetAllJson();
            root[key] = JToken.FromObject(value);
            SaveAllJson(root);
        }

        /// <summary>
        /// Gets a value from the JSON data using a specified path of keys.
        /// </summary>
        /// <param name="keys">An array of keys representing the path to the value.</param>
        /// <returns>The value as an object, or null if the path does not exist.</returns>
        public object? GetData(params string[] keys)
        {
            JObject root = GetAllJson();
            JToken? token = root;

            // Traverse the JSON hierarchy to the specified key path
            foreach (var key in keys)
            {
                token = token?[key];
                if (token == null) break; // Exit if any level doesn't exist
            }

            return token?.ToObject<object>(); // Return as object (can cast to specific type)
        }

        /// <summary>
        /// Sets a value in the JSON data using a specified path of keys.
        /// </summary>
        /// <param name="value">The value to set in the JSON data.</param>
        /// <param name="keys">An array of keys representing the path to the value.</param>
        public void SetData(object value, params string[] keys)
        {
            JObject root = GetAllJson();
            JToken current = root;

            // Traverse the JSON hierarchy to the specified key path
            for (int i = 0; i < keys.Length - 1; i++)
            {
                string key = keys[i];
                if (current[key] is not JObject next)
                {
                    next = new JObject();
                    current[key] = next;
                }
                current = next;
            }

            current[keys[^1]] = JToken.FromObject(value); // Set value at final key
            SaveAllJson(root);
        }
    }
}
