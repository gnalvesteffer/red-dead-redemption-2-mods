using System.Drawing;
using MapEditing.Utilities;

namespace MapEditing.UserInterface
{
    internal class TextElement : Element
    {
        public string Message { get; set; }

        public TextElement() : base(Color.White)
        {
        }

        public TextElement(Color color) : base(color)
        {
        }

        public override void Draw()
        {
            NativeUtility.DrawText(
                Message,
                NormalizedScreenPosition,
                0.5f,
                Color.White,
                true
            );
        }
    }
}
