using System.Collections.Immutable;

namespace NodeVideoEffects.Utility
{
    public static class Logger
    {
        static LinkedList<(DateTime, LogLevel, string)> logs = [];

        public static void Write(LogLevel level, string message)
        {
            logs.AddLast((DateTime.Now, level, message));
            if (logs.Count > 1500)
            {
                logs.RemoveFirst();
            }
            LogUpdated.Invoke(null, new());
        }

        public static ImmutableList<(DateTime, LogLevel, string)> Read() => logs.ToImmutableList();

        public static EventHandler LogUpdated = delegate { };
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }
}
