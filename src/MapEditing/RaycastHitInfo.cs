using RDR2.Math;

namespace MapEditing
{
    internal class RaycastHitInfo
    {
        public bool DidHit { get; }
        public Vector3 HitPosition { get; }
        public Vector3 SurfaceNormal { get; }

        public RaycastHitInfo(bool didHit, Vector3 hitPosition, Vector3 surfaceNormal)
        {
            DidHit = didHit;
            HitPosition = hitPosition;
            SurfaceNormal = surfaceNormal;
        }
    }
}
