namespace PCLMock.CodeGeneration.Logging
{
    using System;
    using System.Collections.Immutable;

    public static class LogSinkExtensions
    {
        public static ILogSink WithSource(this ILogSink @this, Type source) =>
            new LogSinkWithSource(@this, source.FullName);

        public static ILogSink WithSource(this ILogSink @this, string source) =>
            new LogSinkWithSource(@this, source);

        public static ILogSink WithMinimumLevel(this ILogSink @this, LogLevel minimumLevel) =>
            new LogSinkWithMinimumLevel(@this, minimumLevel);

        public static void Debug(this ILogSink @this, string message)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Debug,
                message,
                default);
            @this.Log(logEvent);
        }

        public static void Debug(this ILogSink @this, string message, params object[] messageArgs)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Debug,
                message,
                ImmutableArray.Create(messageArgs));
            @this.Log(logEvent);
        }

        public static void Info(this ILogSink @this, string message)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Info,
                message,
                default);
            @this.Log(logEvent);
        }

        public static void Info(this ILogSink @this, string message, params object[] messageArgs)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Info,
                message,
                ImmutableArray.Create(messageArgs));
            @this.Log(logEvent);
        }

        public static void Warn(this ILogSink @this, string message)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Warn,
                message,
                default);
            @this.Log(logEvent);
        }

        public static void Warn(this ILogSink @this, string message, params object[] messageArgs)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Warn,
                message,
                ImmutableArray.Create(messageArgs));
            @this.Log(logEvent);
        }

        public static void Error(this ILogSink @this, string message)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Error,
                message,
                default);
            @this.Log(logEvent);
        }

        public static void Error(this ILogSink @this, string message, params object[] messageArgs)
        {
            var logEvent = new LogEvent(
                default,
                LogLevel.Error,
                message,
                ImmutableArray.Create(messageArgs));
            @this.Log(logEvent);
        }

        private sealed class LogSinkWithSource : ILogSink
        {
            private readonly ILogSink inner;
            private readonly string source;

            public LogSinkWithSource(ILogSink inner, string source)
            {
                this.inner = inner;
                this.source = source;
            }

            public void Log(LogEvent logEvent)
            {
                if (logEvent.Source == null)
                {
                    // Only overwrite the source if it's not already present, otherwise nobody can override our override!
                    logEvent = new LogEvent(
                        this.source,
                        logEvent.Level,
                        logEvent.Message,
                        logEvent.MessageArgs);
                }

                this.inner.Log(logEvent);
            }

            public override string ToString() => this.inner.ToString();
        }

        private sealed class LogSinkWithMinimumLevel : ILogSink
        {
            private readonly ILogSink inner;
            private readonly LogLevel minimumLevel;

            public LogSinkWithMinimumLevel(ILogSink inner, LogLevel minimumLevel)
            {
                this.inner = inner;
                this.minimumLevel = minimumLevel;
            }

            public void Log(LogEvent logEvent)
            {
                if (logEvent.Level < this.minimumLevel)
                {
                    return;
                }

                this.inner.Log(logEvent);
            }

            public override string ToString() => this.inner.ToString();
        }
    }
}