using Newtonsoft.Json;
using System.IO;

namespace CSGO_Font_Manager
{
    public class JsonManager<T> where T : new()
    {
        private string _targetFile { get; set; }

        public JsonManager(string targetFile)
        {
            _targetFile = targetFile;
        }

        public T Load()
        {
            if (!File.Exists(_targetFile)) return new T();
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(_targetFile));
        }

        public void Save(T Data)
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            (new FileInfo(_targetFile)).Directory.Create();
            File.WriteAllText(_targetFile, json);
        }
    }
}