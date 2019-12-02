using MapEditing.Utilities;
using RDR2.Math;

namespace MapEditing.UserInterface.Basic
{
    internal abstract class Element
    {
        /// <summary>
        /// Screen position in pixels.
        /// </summary>
        public Vector2 AbsoluteScreenPosition
        {
            get => ScreenUtility.GetAbsolutePosition(NormalizedScreenPosition);
            set => NormalizedScreenPosition = ScreenUtility.GetNormalizedPosition(value);
        }

        public Vector2 NormalizedScreenPosition { get; set; }

        public Element(Vector2 normalizedScreenPosition)
        {
            NormalizedScreenPosition = normalizedScreenPosition;
        }

        public Element() : this(Vector2.Zero)
        {
        }

        public abstract void Draw();
    }
}
