using RDR2.Math;

namespace MapEditing
{
    internal class MapObjectDeserializer
    {
        public MapObject Deserialize(string serializedMapObject)
        {
            var parts = serializedMapObject.Split('|');
            var objectHash = parts[0];
            var position = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            var rotation = new Vector3(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]));
            return new MapObject(objectHash, position, rotation);
        }
    }
}
