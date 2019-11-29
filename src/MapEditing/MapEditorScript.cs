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
        }

        private void OnTick(object sender, EventArgs e)
        {
            _mapEditor.OnTick();
        }
    }
}