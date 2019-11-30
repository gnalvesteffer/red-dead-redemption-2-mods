using System.Runtime.InteropServices;

namespace MapEditing
{
    internal static class DllImports
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();
    }
}
