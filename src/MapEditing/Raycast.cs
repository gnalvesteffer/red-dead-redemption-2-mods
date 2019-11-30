using RDR2;
using RDR2.Math;

namespace MapEditing
{
    internal class Raycast
    {
        public Vector3 StartPosition { get; }
        public Vector3 EndPosition { get; }
        public Entity EntityToIgnore { get; }

        public Raycast(Vector3 startPosition, Vector3 endPosition, Entity entityToIgnore = null)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            EntityToIgnore = entityToIgnore;
        }

        public RaycastHitInfo GetHitInfo()
        {
            var rayHandle = Utilities.StartShapeTestRay(StartPosition, EndPosition, Utilities.ShapeTestIntersectionLevel.IntersectObjects, EntityToIgnore);
            var shapeTestResult = Utilities.GetShapeTestResult(rayHandle);
            return new RaycastHitInfo(
                shapeTestResult.didHit,
                shapeTestResult.hitPosition,
                shapeTestResult.surfaceNormal
            );
        }
    }
}
