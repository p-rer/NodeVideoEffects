using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows;

namespace NodeVideoEffects.Utility;

public static class Logger
{
    private static readonly LinkedList<Tuple<DateTime, LogLevel, string, object?>> Logs = [];
    private static readonly ConcurrentQueue<Tuple<DateTime, LogLevel, string, object?, Assembly>> ConsoleLogs = [];
    private static byte _levels = 0x0F;
    private static readonly Lock LogsLock = new();

    public static EventHandler? LogUpdated;

    private static void WriteLogToConsole()
    {
        try
        {
            var result = ConsoleLogs.TryDequeue(out var log);
            if (!result || log == null) return;
            Console.Write(@"");
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

            var asm = log.Item5.FullName ?? "";
            Console.CursorLeft = Console.BufferWidth - asm.Length - 3;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"in " + asm);
            Console.ResetColor();
            Console.WriteLine();
        }
        catch
        {
            // ignored
        }
    }

    public static void Write(LogLevel level, string message, object? obj = null)
    {
        var last = Logs.Last?.Value;
        if (last != null && last.Item2 == level && last.Item3 == message && last.Item4 == obj) return;
        try
        {
            Task.Run(() =>
            {
                var log = new Tuple<DateTime, LogLevel, string, object?>(DateTime.Now, level, message, obj);
#if DEBUG
#else
            if (level != LogLevel.Debug)
#endif
                lock (LogsLock)
                {
                    Logs.AddLast(log);
                    ConsoleLogs.Enqueue(new Tuple<DateTime, LogLevel, string, object?, Assembly>(log.Item1, log.Item2,
                        log.Item3, log.Item4, Assembly.GetCallingAssembly()));
                    if (Logs.Count > 1500) Logs.RemoveFirst();
                    LogUpdated?.Invoke(null, EventArgs.Empty);
                    Application.Current.Dispatcher.Invoke(WriteLogToConsole);
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(@"Logger error: " + e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    public static void Clear()
    {
        try
        {
            lock (LogsLock)
            {
                Logs.Clear();
                Console.Clear();
                LogUpdated?.Invoke(null, EventArgs.Empty);
            }
        }
        catch
        {
            // ignore
        }
    }

    // levels: 0x01 = Debug, 0x02 = Info, 0x04 = Warn, 0x08 = Error
    // levels can be combined, e.g. 0x03 = Debug + Info
    public static void Filter(byte levels)
    {
        lock (LogsLock)
        {
            _levels = levels;
            LogUpdated?.Invoke(null, EventArgs.Empty);
        }
    }

    public static ImmutableList<Tuple<DateTime, LogLevel, string, object?>> Read()
    {
        lock (LogsLock)
        {
            return Logs
                .Where(log => ((byte)log.Item2 & _levels) != 0)
                .ToImmutableList();
        }
    }
}

public enum LogLevel
{
    Debug = 0x01,
    Info = 0x02,
    Warn = 0x04,
    Error = 0x08
}