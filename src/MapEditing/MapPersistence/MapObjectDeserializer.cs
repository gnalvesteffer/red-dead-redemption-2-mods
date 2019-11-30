using Newtonsoft.Json;

namespace MapEditing.MapPersistence
{
    internal class MapObjectDeserializer
    {
        public SerializableMapObject Deserialize(string serializedMapObject)
        {
            return JsonConvert.DeserializeObject<SerializableMapObject>(serializedMapObject);
        }
    }
}
