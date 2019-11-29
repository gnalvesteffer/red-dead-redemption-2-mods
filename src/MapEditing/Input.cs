using System;
using System.Windows.Forms;

namespace MapEditing
{
    public class Input
    {
        public string Name;
        public Keys Key;
        public int CooldownDurationInTicks;
        public bool CanBeUsedOutsideOfMapEditor;
        public Action Handler;
        public long LastUsageTick;


        public Input(
            string name,
            Keys key,
            int cooldownDurationInTicks,
            bool canBeUsedOutsideOfMapEditor,
            Action handler
        )
        {
            Name = name;
            Key = key;
            CooldownDurationInTicks = cooldownDurationInTicks;
            CanBeUsedOutsideOfMapEditor = canBeUsedOutsideOfMapEditor;
            Handler = handler;
        }
    }
}