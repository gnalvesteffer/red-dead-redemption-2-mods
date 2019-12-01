using System.Drawing;
using MapEditing.Utilities;
using RDR2.Math;

namespace MapEditing.UserInterface
{
    internal abstract class Element
    {
        protected Vector2 AbsoluteScreenPosition => ScreenUtility.GetAbsolutePosition(NormalizedScreenPosition);
        public Vector2 NormalizedScreenPosition { get; set; }
        public Vector2 Size { get; set; }
        public Color Color { get; set; }

        public Element(Color color)
        {
            Color = color;
        }

        public Element() : this(Color.White)
        {
        }

        public abstract void Draw();
    }
}
