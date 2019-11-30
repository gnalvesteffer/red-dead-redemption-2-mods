using System;
using MapEditing.MapEditing;
using MapEditing.Utilities;
using RDR2;

namespace MapEditing.Scripts
{
    public class MapEditorScript : Script
    {
        private static string LoadedText = "Loaded Map Editor by Xorberax";

        private readonly MapEditor _mapEditor = new MapEditor();

        public MapEditorScript()
        {
            Tick += OnTick;
            KeyDown += _mapEditor.OnKeyDown;
            KeyUp += _mapEditor.OnKeyUp;
            DllImportsUtility.AllocConsole();
            Console.WriteLine(LoadedText);
            NativeUtility.UserFriendlyPrint(LoadedText);
        }

        private void OnTick(object sender, EventArgs e)
        {
            _mapEditor.OnTick();
        }
    }
}
