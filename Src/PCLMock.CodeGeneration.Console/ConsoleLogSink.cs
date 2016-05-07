namespace PCLMock.CodeGeneration.Console
{
    using System;
    using PCLMock.CodeGeneration.Logging;

    public sealed class ConsoleLogSink : ILogSink
    {
        public static readonly ConsoleLogSink Instance = new ConsoleLogSink();

        private ConsoleLogSink()
        {
        }

        public bool IsEnabled => true;

        public void Log(Type source, LogLevel level, string message)
        {
            var currentColor = Console.ForegroundColor;
            ConsoleColor color;

            switch (level)
            {
                case LogLevel.Debug:
                    color = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    color = ConsoleColor.White;
                    break;
                case LogLevel.Warn:
                    color = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.Negative:
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.Positive:
                    color = ConsoleColor.Green;
                    break;
                case LogLevel.Error:
                    color = ConsoleColor.Red;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            Console.ForegroundColor = color;
            Console.Write("[");
            Console.Write(source.FullName);
            Console.Write("] ");
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }
    }
}