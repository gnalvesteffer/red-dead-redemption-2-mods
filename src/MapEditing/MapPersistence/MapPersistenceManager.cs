using System.IO;
using Newtonsoft.Json;

namespace MapEditing.MapPersistence
{
    internal class MapPersistenceManager
    {
        public SerializableMap LoadMap(string mapFilePath)
        {
            var serializedMap = File.ReadAllText(mapFilePath);
            return JsonConvert.DeserializeObject<SerializableMap>(serializedMap);
        }

        public void SaveMap(string mapFilePath, SerializableMap serializableMap)
        {
            var serializedMap = JsonConvert.SerializeObject(serializableMap, Formatting.Indented);
            Directory.CreateDirectory(new FileInfo(mapFilePath).DirectoryName);
            File.WriteAllText(mapFilePath, serializedMap);
        }
    }
}
