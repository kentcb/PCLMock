namespace PCLMock.CodeGeneration.Logging
{
    using System;
    using System.Reactive.Disposables;
    using Microsoft.Extensions.Logging;

    public sealed class BuildalyzerLogFactory : ILoggerFactory
    {
        private readonly ILogSink logSink;

        public BuildalyzerLogFactory(ILogSink logSink)
        {
            this.logSink = logSink;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName) =>
            new Logger(this.logSink, categoryName);

        public void Dispose()
        {
        }

        private sealed class Logger : ILogger
        {
            private readonly ILogSink logSink;
            private readonly string categoryName;

            public Logger(ILogSink logSink, string categoryName)
            {
                this.logSink = logSink;
                this.categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) =>
                Disposable.Empty;

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) =>
                true;

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                this.logSink.Log(
                    typeof(BuildalyzerLogFactory),
                    ToLogLevel(logLevel),
                    formatter(state, exception));

            private static LogLevel ToLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                    case Microsoft.Extensions.Logging.LogLevel.Trace:
                    case Microsoft.Extensions.Logging.LogLevel.None:
                        return LogLevel.Debug;
                    case Microsoft.Extensions.Logging.LogLevel.Information:
                        return LogLevel.Info;
                    case Microsoft.Extensions.Logging.LogLevel.Warning:
                        return LogLevel.Warn;
                    default:
                        return LogLevel.Error;
                }
            }
        }
    }
}