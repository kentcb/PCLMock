namespace PCLMock.CodeGeneration.Console.Logging
{
    using System;
    using PCLMock.CodeGeneration.Logging;

    public sealed class ConsoleLogSink : ILogSink
    {
        public static readonly ConsoleLogSink Instance = new ConsoleLogSink();

        private ConsoleLogSink()
        {
        }

        public void Log(LogEvent logEvent)
        {
            var currentColor = Console.ForegroundColor;
            var color = logEvent.Level switch
            {
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warn => ConsoleColor.DarkYellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            Console.ForegroundColor = color;
            Console.Write("[");
            Console.Write(logEvent.Source ?? "<<UNKNOWN SOURCE>>");
            Console.Write("] ");
            Console.WriteLine(logEvent.FormattedMessage);
            Console.ForegroundColor = currentColor;
        }
    }
}