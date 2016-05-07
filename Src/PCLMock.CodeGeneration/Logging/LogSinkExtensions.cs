namespace PCLMock.CodeGeneration.Logging
{
    using System;
    using System.Globalization;

    public static class LogSinkExtensions
    {
        public static void Log(this ILogSink @this, Type source, LogLevel level, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, level, message);
        }

        public static void Debug(this ILogSink @this, Type source, string message)
        {
            @this.Log(source, LogLevel.Debug, message);
        }

        public static void Debug(this ILogSink @this, Type source, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, LogLevel.Debug, message);
        }

        public static void Info(this ILogSink @this, Type source, string message)
        {
            @this.Log(source, LogLevel.Info, message);
        }

        public static void Info(this ILogSink @this, Type source, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, LogLevel.Info, message);
        }

        public static void Warn(this ILogSink @this, Type source, string message)
        {
            @this.Log(source, LogLevel.Warn, message);
        }

        public static void Warn(this ILogSink @this, Type source, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, LogLevel.Warn, message);
        }

        public static void Positive(this ILogSink @this, Type source, string message)
        {
            @this.Log(source, LogLevel.Positive, message);
        }

        public static void Positive(this ILogSink @this, Type source, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, LogLevel.Positive, message);
        }

        public static void Negative(this ILogSink @this, Type source, string message)
        {
            @this.Log(source, LogLevel.Negative, message);
        }

        public static void Negative(this ILogSink @this, Type source, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, LogLevel.Negative, message);
        }

        public static void Error(this ILogSink @this, Type source, string message)
        {
            @this.Log(source, LogLevel.Error, message);
        }

        public static void Error(this ILogSink @this, Type source, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(source, LogLevel.Error, message);
        }
    }
}