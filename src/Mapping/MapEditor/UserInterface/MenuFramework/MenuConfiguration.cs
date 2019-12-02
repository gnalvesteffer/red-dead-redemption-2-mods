using System.Collections.Generic;
using System.Drawing;

namespace MapEditing.UserInterface.MenuFramework
{
    internal class MenuConfiguration
    {
        public string Title { get; set; }
        public int TitleFontSizeInPixels { get; set; }
        public Color TitleBackgroundColor { get; set; }
        public Color TitleTextColor { get; set; }
        public IEnumerable<MenuItemConfiguration> MenuItems { get; set; }
    }
}
