using System;
using RDR2;

namespace MapEditing
{
    public class MapEditorScript : Script
    {
        private readonly MapEditor _mapEditor = new MapEditor();

        public MapEditorScript()
        {
            Tick += OnTick;
            KeyDown += _mapEditor.OnKeyDown;
            KeyUp += _mapEditor.OnKeyUp;
        }

        private void OnTick(object sender, EventArgs e)
        {
            _mapEditor.OnTick();
        }
    }
}