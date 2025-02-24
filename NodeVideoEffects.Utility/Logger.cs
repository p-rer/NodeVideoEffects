using System.Collections.Immutable;

namespace NodeVideoEffects.Utility
{
    public static class Logger
    {
        private static readonly LinkedList<(DateTime, LogLevel, string, object?)> Logs = [];

        public static void Write(LogLevel level, string message, object? obj = null)
        {
            Logs.AddLast((DateTime.Now, level, message, obj));
            if (Logs.Count > 1500)
            {
                Logs.RemoveFirst();
            }
            LogUpdated.Invoke(null, EventArgs.Empty);
        }

        public static ImmutableList<(DateTime, LogLevel, string, object?)> Read() => Logs.ToImmutableList();

        public static EventHandler LogUpdated = delegate { };
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }
}
