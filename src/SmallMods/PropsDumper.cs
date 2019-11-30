using System.IO;
using System.Linq;
using RDR2;
using RDR2.Native;

public class PropsDumper : Script
{
    private static readonly string PropsDumpFilePath = "scripts/props-dump.txt";

    public PropsDumper()
    {
        DumpProps();
    }

    private void DumpProps()
    {
        var props = World.GetAllProps();
        Directory.CreateDirectory(new FileInfo(PropsDumpFilePath).DirectoryName);
        File.WriteAllLines(PropsDumpFilePath, props.Select(prop => prop.ToString()));
        Print(props.Length);
    }

    private static void Print(object text)
    {
        var createdString = Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text.ToString());
        Function.Call(Hash._LOG_SET_CACHED_OBJECTIVE, createdString);
        Function.Call(Hash._LOG_PRINT_CACHED_OBJECTIVE);
        Function.Call(Hash._LOG_CLEAR_CACHED_OBJECTIVE);
    }
}
