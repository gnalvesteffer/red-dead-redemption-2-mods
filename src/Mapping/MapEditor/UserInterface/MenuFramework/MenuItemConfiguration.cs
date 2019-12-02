using System;
using System.Collections.Generic;
using System.Drawing;

namespace MapEditing.UserInterface.MenuFramework
{
    internal class MenuItemConfiguration
    {
        public Func<string> GetDisplayText { get; set; }
        public Color UnfocusedBackgroundColor { get; set; }
        public Color FocusedBackgroundColor { get; set; }
        public Color UnfocusedTextColor { get; set; }
        public Color FocusedTextColor { get; set; }
        public int FontSizeInPixels { get; set; }
        public Action OnSelect { get; set; }
        public IEnumerable<MenuItemConfiguration> ChildMenuItems { get; set; }
    }
}
