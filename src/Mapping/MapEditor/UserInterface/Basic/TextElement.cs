using System.Drawing;
using MapEditing.Utilities;

namespace MapEditing.UserInterface.Basic
{
    internal class TextElement : ColoredElement
    {
        public string Message { get; set; }
        public int FontSizeInPixels { get; set; } = 12;
        public bool IsCentered { get; set; }

        public TextElement(string message, int fontSizeInPixels = 12) : this(message, Color.White)
        {
            FontSizeInPixels = fontSizeInPixels;
        }

        public TextElement(string message, Color color) : this(color)
        {
            Message = message;
        }

        public TextElement(Color color) : base(color)
        {
        }

        public TextElement() : base(Color.White)
        {
        }

        public override void Draw()
        {
            NativeUtility.DrawText(
                Message,
                FontSizeInPixels,
                Color,
                NormalizedScreenPosition,
                IsCentered
            );
        }
    }
}
