using System.Collections.Generic;
using System.Linq;
using MapEditing.UserInterface.Basic;
using MapEditing.Utilities;
using RDR2.Math;

namespace MapEditing.UserInterface.MenuFramework
{
    internal class Menu : Element
    {
        private static readonly Vector2 _menuPosition = new Vector2(15, 15);
        private readonly List<MenuItem> _menuItems = new List<MenuItem>();
        private MenuItem _focusedMenuItem;

        public bool IsVisible { get; set; }

        public Menu(MenuConfiguration menuConfiguration)
        {
            foreach (var menuItemConfiguration in menuConfiguration.MenuItems)
            {
                _menuItems.Add(new MenuItem(menuItemConfiguration));
            }
            _focusedMenuItem = _menuItems.FirstOrDefault();
        }

        public void NavigateMenu(MenuNavigationDirection direction)
        {
            var deltaIndex = (int)direction;
            var selectedMenuItemIndex = _menuItems.IndexOf(_focusedMenuItem);
            _focusedMenuItem = _menuItems[(selectedMenuItemIndex + deltaIndex).Clamp(0, _menuItems.Count - 1)];
        }

        public void SelectFocusedMenuItem()
        {
            _focusedMenuItem?.Select();
        }

        public override void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            for (var menuItemIndex = 0; menuItemIndex < _menuItems.Count; menuItemIndex++)
            {
                var menuItem = _menuItems[menuItemIndex];
                menuItem.AbsoluteScreenPosition = new Vector2(
                    _menuPosition.X,
                    _menuPosition.Y + menuItem.GetMenuItemAbsoluteSize().Y * menuItemIndex
                );
                menuItem.IsFocused = menuItem == _focusedMenuItem;
                menuItem.Draw();
            }
        }
    }
}
