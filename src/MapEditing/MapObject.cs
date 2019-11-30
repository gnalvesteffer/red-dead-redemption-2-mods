using RDR2;
using RDR2.Math;

namespace MapEditing
{
    internal class MapObject
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public string HashValue;
        public Entity Entity;

        public MapObject(string hashValue, Vector3 position, Vector3 rotation, Entity entity)
        {
            HashValue = hashValue;
            Position = position;
            Rotation = rotation;
            Entity = entity;
        }

        public MapObject(string hashValue, Vector3 position, Vector3 rotation)
        {
            HashValue = hashValue;
            Position = position;
            Rotation = rotation;
        }
    }
}
