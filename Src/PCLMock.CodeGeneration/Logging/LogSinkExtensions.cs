namespace PCLMock.CodeGeneration.Logging
{
    using System.Globalization;

    public static class LogSinkExtensions
    {
        public static void Log(this ILogSink @this, LogLevel level, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(level, message);
        }

        public static void Debug(this ILogSink @this, string message)
        {
            @this.Log(LogLevel.Debug, message);
        }

        public static void Debug(this ILogSink @this, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Debug, message);
        }

        public static void Info(this ILogSink @this, string message)
        {
            @this.Log(LogLevel.Info, message);
        }

        public static void Info(this ILogSink @this, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Info, message);
        }

        public static void Positive(this ILogSink @this, string message)
        {
            @this.Log(LogLevel.Positive, message);
        }

        public static void Positive(this ILogSink @this, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Positive, message);
        }

        public static void Negative(this ILogSink @this, string message)
        {
            @this.Log(LogLevel.Negative, message);
        }

        public static void Negative(this ILogSink @this, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Negative, message);
        }

        public static void Error(this ILogSink @this, string message)
        {
            @this.Log(LogLevel.Error, message);
        }

        public static void Error(this ILogSink @this, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Error, message);
        }
    }
}