using System.Runtime.InteropServices;

namespace XorberaxMapEditor.Utilities
{
    internal static class DllImportsUtility
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();
    }
}
