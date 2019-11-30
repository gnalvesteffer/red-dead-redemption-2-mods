using Newtonsoft.Json;

namespace MapEditing.MapPersistence
{
    internal class MapObjectSerializer
    {
        public string Serialize(SerializableMapObject mapObject)
        {
            return JsonConvert.SerializeObject(mapObject);
        }
    }
}
