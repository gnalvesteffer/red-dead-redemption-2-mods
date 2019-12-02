using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditing.UserInterface.Basic;
using MapEditing.Utilities;
using RDR2.Math;

namespace MapEditing.UserInterface.MenuFramework
{
    internal class MenuItem : Element
    {
        private readonly Func<string> _getDisplayText;
        private readonly int _fontSizeInPixels;
        private readonly Color _unfocusedBackgroundColor;
        private readonly Color _focusedBackgroundColor;
        private readonly Color _unfocusedTextColor;
        private readonly Color _focusedTextColor;
        private readonly Action _onSelect;
        private readonly RectangleElement _rectangleElement;
        private readonly TextElement _textElement;

        public IEnumerable<MenuItem> ChildMenuItems { get; set; } // ToDo: draw child menu items
        public bool IsFocused { get; set; }

        public MenuItem(
            Func<string> getDisplayText,
            int fontSizeInPixels,
            Color unfocusedBackgroundColor,
            Color focusedBackgroundColor,
            Color unfocusedTextColor,
            Color focusedTextColor,
            Action onSelect,
            Vector2 absoluteScreenPosition,
            IEnumerable<MenuItem> childMenuItems
        )
        {
            _getDisplayText = getDisplayText;
            _fontSizeInPixels = fontSizeInPixels;
            _unfocusedBackgroundColor = unfocusedBackgroundColor;
            _focusedBackgroundColor = focusedBackgroundColor;
            _unfocusedTextColor = unfocusedTextColor;
            _focusedTextColor = focusedTextColor;
            _rectangleElement = new RectangleElement(GetMenuItemAbsoluteSize());
            _textElement = new TextElement(string.Empty, fontSizeInPixels);
            _onSelect = onSelect;
            AbsoluteScreenPosition = absoluteScreenPosition;
            ChildMenuItems = childMenuItems;
        }

        public MenuItem(
            Func<string> getDisplayText,
            int fontSizeInPixels,
            Color unfocusedBackgroundColor,
            Color focusedBackgroundColor,
            Color unfocusedTextColor,
            Color focusedTextColor,
            Action onSelect,
            Vector2 absoluteScreenPosition
        ) : this(
            getDisplayText,
            fontSizeInPixels,
            unfocusedBackgroundColor,
            focusedBackgroundColor,
            unfocusedTextColor,
            focusedTextColor,
            onSelect,
            absoluteScreenPosition,
            new List<MenuItem>()
        )
        {
        }

        public MenuItem(MenuItemConfiguration menuItemConfiguration) : this(
            menuItemConfiguration.GetDisplayText,
            menuItemConfiguration.FontSizeInPixels,
            menuItemConfiguration.UnfocusedBackgroundColor,
            menuItemConfiguration.FocusedBackgroundColor,
            menuItemConfiguration.UnfocusedTextColor,
            menuItemConfiguration.FocusedTextColor,
            menuItemConfiguration.OnSelect,
            Vector2.Zero
        )
        {
        }

        /// <summary>
        /// The size in pixels.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetMenuItemAbsoluteSize()
        {
            var displayText = _getDisplayText();
            return new Vector2(
                NativeUtility.GetTextScale(displayText, _fontSizeInPixels).X * _fontSizeInPixels,
                _fontSizeInPixels
            );
        }

        public void Select()
        {
            _onSelect();
        }

        public override void Draw()
        {
            _rectangleElement.NormalizedScreenPosition = NormalizedScreenPosition;
            _rectangleElement.AbsoluteSize = GetMenuItemAbsoluteSize();
            _rectangleElement.Color = IsFocused ? _focusedBackgroundColor : _unfocusedBackgroundColor;
            _rectangleElement.Draw();
            _textElement.NormalizedScreenPosition = NormalizedScreenPosition;
            _textElement.Color = IsFocused ? _focusedTextColor : _unfocusedTextColor;
            _textElement.Message = _getDisplayText();
            _textElement.Draw();
        }
    }
}
