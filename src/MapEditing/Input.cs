using System;
using System.Windows.Forms;

namespace MapEditing
{
    internal class Input
    {
        public string Name;
        public Keys Key;
        public bool IsRepeatable;
        public int CooldownDurationInTicks;
        public bool CanBeUsedOutsideOfMapEditor;
        public Action Handler;
        public long LastUsageTick;


        public Input(
            string name,
            Keys key,
            bool isRepeatable,
            int cooldownDurationInTicks,
            bool canBeUsedOutsideOfMapEditor,
            Action handler
        )
        {
            Name = name;
            Key = key;
            IsRepeatable = isRepeatable;
            CooldownDurationInTicks = cooldownDurationInTicks;
            CanBeUsedOutsideOfMapEditor = canBeUsedOutsideOfMapEditor;
            Handler = handler;
        }
    }
}
