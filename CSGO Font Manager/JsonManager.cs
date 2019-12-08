using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CSGO_Font_Manager
{
    public class JsonManager<T> where T: new()
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
            File.WriteAllText(_targetFile, json);
        }
    }
}
