using System.Windows.Forms;
using RDR2.Math;

namespace MapEditing.Utilities
{
    internal static class ScreenUtility
    {
        public static Vector2 GetScreenResolution()
        {
            var screenResolution = Screen.PrimaryScreen.Bounds; // TODO: make reliable
            return new Vector2(screenResolution.Width, screenResolution.Height);
        }

        public static Vector2 GetAbsolutePosition(Vector2 normalizedPosition)
        {
            var screenResolution = GetScreenResolution();
            return new Vector2(
                normalizedPosition.X * screenResolution.X,
                normalizedPosition.Y * screenResolution.Y
            );
        }

        public static Vector2 GetNormalizedPosition(Vector2 absolutePosition)
        {
            var screenResolution = GetScreenResolution();
            return new Vector2(
                absolutePosition.X / screenResolution.X,
                absolutePosition.Y / screenResolution.Y
            );
        }

        public static float GetAspectRatio()
        {
            var resolution = GetScreenResolution();
            return resolution.X / resolution.Y;
        }
    }
}
