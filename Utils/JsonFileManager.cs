using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Utils
{
    public class JsonFileManager
    {
        private readonly string _filePath;

        public JsonFileManager()
        {
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "weatherAppData.json");
        }

        public JObject GetJson()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                return JObject.Parse(json);
            }

            return new JObject();
        }

        public void SaveJson(JObject data)
        {
            string json = data.ToString();
            File.WriteAllText(_filePath, json);
        }

        public T? GetData<T>(string key) where T : class
        {
            JObject root = GetJson();
            return root[key]?.ToObject<T>();
        }

        public void SetData<T>(string key, T value)
        {
            JObject root = GetJson();
            root[key] = JToken.FromObject(value);
            SaveJson(root);
        }

        public object? GetNestedValue(params string[] keys)
        {
            JObject root = GetJson();
            JToken? token = root;

            foreach (var key in keys)
            {
                token = token?[key];
                if (token == null) break; // Exit if any level doesn't exist
            }

            return token?.ToObject<object>(); // Return as object (can cast to specific type)
        }

        public void SetNestedValue(object value, params string[] keys)
        {
            JObject root = GetJson();
            JToken current = root;

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
            SaveJson(root);
        }

    }

}