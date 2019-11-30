namespace MapEditing
{
    internal class MapObjectSerializer
    {
        public string Serialize(MapObject mapObject)
        {
            return $"{mapObject.HashValue}|{mapObject.Position.X}|{mapObject.Position.Y}|{mapObject.Position.Z}|{mapObject.Rotation.X}|{mapObject.Rotation.Y}|{mapObject.Rotation.Z}";
        }
    }
}
