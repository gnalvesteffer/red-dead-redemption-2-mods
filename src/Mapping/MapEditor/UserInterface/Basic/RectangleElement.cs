using System.Drawing;
using MapEditing.Utilities;
using RDR2.Math;

namespace MapEditing.UserInterface.Basic
{
    internal class RectangleElement : ColoredElement
    {
        public Vector2 AbsoluteSize { get; set; }

        public RectangleElement(
            Vector2 normalizedScreenPosition,
            Vector2 absoluteSize,
            Color color
        ) : base(normalizedScreenPosition, color)
        {
            AbsoluteSize = absoluteSize;
        }

        public RectangleElement(Vector2 absoluteSize) : this(Vector2.Zero, absoluteSize, Color.Black)
        {
        }

        public override void Draw()
        {
            var screenResolution = ScreenUtility.GetScreenResolution();
            var normalizedSize = new Vector2(
                AbsoluteSize.X / screenResolution.X,
                AbsoluteSize.Y / screenResolution.Y
            );
            var normalizedPositionOffset = new Vector2(
                normalizedSize.X / 2,
                normalizedSize.Y
            );
            var normalizedPosition = NormalizedScreenPosition + normalizedPositionOffset;
            NativeUtility.DrawRectangle(normalizedPosition, normalizedSize, Color);
        }
    }
}
