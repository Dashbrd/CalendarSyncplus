using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Text;

namespace CalendarSyncPlus.Domain.File.Json
{
    public class JsonSerializer<T> : IJsonSerializer<T> where T : class, new()
    {
        public T Deserialize(string json)
        {
            return Deserialize(json, Encoding.UTF8);
        }

        public T Deserialize(string json, Encoding encoding)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("XML cannot be null or empty", "xml");
            }
                        
            return JsonConvert.DeserializeObject<T>(json);
        }

        public T DeserializeFromFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename", "Json filename cannot be null or empty");
            }

            if (!System.IO.File.Exists(filename))
            {
                throw new FileNotFoundException("Cannot find Json file to deserialize", filename);
            }

            T obj;
            
            try
            {
                // Open the file containing the data that you want to deserialize.
                using (TextReader reader = System.IO.File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    obj = serializer.Deserialize(reader, typeof(T)) as T;
                }
            }
            catch
            {
                throw;
            }
            return obj;
        }

        public string Serialize(T source)
        {
            throw new NotImplementedException();
        }

        public void SerializeToFile(T source, string filename)
        {
            using (StreamWriter file = new StreamWriter(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Formatting = Formatting.Indented;
                using (JsonWriter writer = new JsonTextWriter(file))
                {
                    serializer.Serialize(writer, source);
                }
            }
        }
    }
}
