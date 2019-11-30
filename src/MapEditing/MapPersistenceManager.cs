using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MapEditing
{
    internal class MapPersistenceManager
    {
        private static MapObjectSerializer _mapObjectSerializer = new MapObjectSerializer();
        private static MapObjectDeserializer _mapObjectDeserializer = new MapObjectDeserializer();

        public IEnumerable<MapObject> LoadMap(string mapFilePath)
        {
            return File.ReadAllLines(mapFilePath).Select(_mapObjectDeserializer.Deserialize);
        }

        public void SaveMap(string mapFilePath, IEnumerable<MapObject> mapObjects)
        {
            Directory.CreateDirectory(new FileInfo(mapFilePath).DirectoryName);
            File.WriteAllLines(mapFilePath, mapObjects.Select(_mapObjectSerializer.Serialize));
        }
    }
}
