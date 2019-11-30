using Newtonsoft.Json;
using RDR2.Math;
using XorberaxMapEditor.MapEditing;

namespace XorberaxMapEditor.MapPersistence
{
    public class SerializableMapObject
    {
        public string ModelName { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        [JsonIgnore]
        public Vector3 Position => new Vector3(PositionX, PositionY, PositionZ);

        [JsonIgnore]
        public Vector3 Rotation => new Vector3(RotationX, RotationY, RotationZ);

        public SerializableMapObject()
        {
        }

        internal SerializableMapObject(MapObject mapObject)
        {
            ModelName = mapObject.ModelName;
            PositionX = mapObject.Position.X;
            PositionY = mapObject.Position.Y;
            PositionZ = mapObject.Position.Z;
            RotationX = mapObject.Rotation.X;
            RotationY = mapObject.Rotation.Y;
            RotationZ = mapObject.Rotation.Z;
        }
    }
}
