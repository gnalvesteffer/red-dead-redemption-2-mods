using RDR2;
using RDR2.Math;

namespace XorberaxMapEditor.MapEditing
{
    internal class MapObject
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public string ModelName;
        public Entity Entity;

        public MapObject(string modelName, Vector3 position, Vector3 rotation, Entity entity)
        {
            ModelName = modelName;
            Position = position;
            Rotation = rotation;
            Entity = entity;
        }
    }
}
