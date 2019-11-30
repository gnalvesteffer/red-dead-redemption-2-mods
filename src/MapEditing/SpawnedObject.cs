using RDR2;
using RDR2.Math;

namespace MapEditing
{
    internal class SpawnedObject
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public string HashValue;
        public Entity Entity;

        public SpawnedObject(string hashValue, Vector3 position, Vector3 rotation, Entity entity)
        {
            HashValue = hashValue;
            Position = position;
            Rotation = rotation;
            Entity = entity;
        }
    }
}