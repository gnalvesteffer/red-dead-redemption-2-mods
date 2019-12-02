using System.Drawing;
using RDR2.Math;

namespace MapEditing.UserInterface.Basic
{
    internal class ColoredElement : Element
    {
        public Color Color { get; set; }

        public ColoredElement(Vector2 normalizedScreenPosition, Color color) : base(normalizedScreenPosition)
        {
            Color = color;
        }

        public ColoredElement(Color color) : base(Vector2.Zero)
        {
            Color = color;
        }

        public ColoredElement() : this(Color.White)
        {
        }

        public override void Draw()
        {
        }
    }
}
