using System.Runtime.CompilerServices;

namespace NovaProject.Core.Services;

public static class Logger
{
    private static List<string> _logs = new();
    public static void LogInfo(string msg,
        [CallerMemberName] string membername="N/A",
        [CallerFilePath] string path="N/A")
    {
        path = Path.GetFileNameWithoutExtension(path);
        string log = $"[?][{path}.{membername}] {msg}";
        _logs.Add(log);
        Console.WriteLine(log);
    }

    public static void LogError(string msg,
        [CallerMemberName] string membername="N/A",
        [CallerFilePath] string path="N/A")
    {
        path = Path.GetFileNameWithoutExtension(path);
        string log = $"[!][{path}.{membername}] {msg}";
        _logs.Add(log);
        Console.WriteLine(log);
    }

    public static void LogCritical(string msg,
        [CallerMemberName] string membername="N/A",
        [CallerFilePath] string path="N/A")
    {
        path = Path.GetFileNameWithoutExtension(path);
        string log = $"[!!!][{path}.{membername}] {msg}";
        _logs.Add(log);
        Console.WriteLine(log);
    }

    public static void WriteLogFile(string msg)
    {
        throw new NotImplementedException();
    }
}