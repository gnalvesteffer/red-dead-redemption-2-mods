using MapEditing.Utilities;

namespace MapEditing.UserInterface
{
    internal class RectangleElement : Element
    {
        public override void Draw()
        {
            NativeUtility.DrawRectangle(
                AbsoluteScreenPosition.X,
                AbsoluteScreenPosition.Y,
                Size.X,
                Size.Y,
                Color.R,
                Color.G,
                Color.B,
                Color.A
            );
        }
    }
}
