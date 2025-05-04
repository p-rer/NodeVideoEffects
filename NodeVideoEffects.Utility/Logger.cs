using System.Collections.Immutable;

namespace NodeVideoEffects.Utility;

public static class Logger
{
    private static readonly LinkedList<Tuple<DateTime, LogLevel, string, object?>> Logs = [];
    private static readonly Queue<Tuple<DateTime, LogLevel, string, object?>> ConsoleLogs = [];
    private static byte _levels = 0x0F;
    private static bool _isConsoleWriting;
    
    static Logger()
    {
        LogUpdated += (_, _) =>
        {
            if (_isConsoleWriting || ConsoleLogs.Count <= 0) return;
            _isConsoleWriting = true;
            WriteLogToConsole();
            _isConsoleWriting = false;
        };
    }

    private static void WriteLogToConsole()
    {
        while (true)
        {
            var log = ConsoleLogs.Dequeue();
            Console.Write($@"{log.Item1:HH:mm:ss.fff}");
            Console.ForegroundColor = log.Item2 switch
            {
                LogLevel.Debug => ConsoleColor.Green,
                LogLevel.Info => ConsoleColor.Blue,
                LogLevel.Warn => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };
            Console.Write(@" {0,-5}", log.Item2);
            Console.ResetColor();
            Console.WriteLine(@"  {0,-20}", log.Item3);
            if (log.Item4 != null)
            {
                Console.WriteLine(log.Item4);
            }
            Console.WriteLine();
            if (ConsoleLogs.Count > 0) continue;
            break;
        }
    }

    public static void Write(LogLevel level, string message, object? obj = null)
    {
        var log = new Tuple<DateTime, LogLevel, string, object?>(DateTime.Now, level, message, obj);
        Logs.AddLast(log);
        ConsoleLogs.Enqueue(log);
        if (Logs.Count > 1500) Logs.RemoveFirst();
        LogUpdated?.Invoke(null, EventArgs.Empty);
    }
    
    public static void Clear()
    {
        Logs.Clear();
        LogUpdated?.Invoke(null, EventArgs.Empty);
    }

    // levels: 0x01 = Debug, 0x02 = Info, 0x04 = Warn, 0x08 = Error
    // levels can be combined, e.g. 0x03 = Debug + Info
    public static void Filter(byte levels)
    {
        _levels = levels;
        LogUpdated?.Invoke(null, EventArgs.Empty);
    }

    public static ImmutableList<Tuple<DateTime, LogLevel, string, object?>> Read()
    {
        return Logs
            .Where(log => ((byte)log.Item2 & _levels) != 0)
            .ToImmutableList();
    }

    public static EventHandler? LogUpdated;
}

public enum LogLevel
{
    Debug = 0x01,
    Info = 0x02,
    Warn = 0x04,
    Error = 0x08
}