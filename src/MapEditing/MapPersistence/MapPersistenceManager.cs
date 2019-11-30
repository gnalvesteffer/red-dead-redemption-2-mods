using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MapEditing.MapPersistence
{
    internal class MapPersistenceManager
    {
        private static readonly MapObjectSerializer _mapObjectSerializer = new MapObjectSerializer();
        private static readonly MapObjectDeserializer _mapObjectDeserializer = new MapObjectDeserializer();

        public IEnumerable<SerializableMapObject> LoadMap(string mapFilePath)
        {
            return File.ReadAllLines(mapFilePath).Select(_mapObjectDeserializer.Deserialize);
        }

        public void SaveMap(string mapFilePath, IEnumerable<MapObject> mapObjects)
        {
            var serializedMapObjects = mapObjects.Select(mapObject =>
                _mapObjectSerializer.Serialize(new SerializableMapObject(mapObject))
            );
            Directory.CreateDirectory(new FileInfo(mapFilePath).DirectoryName);
            File.WriteAllLines(mapFilePath, serializedMapObjects);
        }
    }
}
