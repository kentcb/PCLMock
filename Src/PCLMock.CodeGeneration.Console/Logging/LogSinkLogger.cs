namespace PCLMock.CodeGeneration.Console.Logging
{
    using System;
    using System.Reactive.Disposables;
    using MS = Microsoft.Extensions.Logging;
    using PCLMock.CodeGeneration.Logging;

    public sealed class LogSinkLogger : MS.ILogger
    {
        private readonly ILogSink logSink;

        public LogSinkLogger(ILogSink logSink)
        {
            this.logSink = logSink;
        }

        public IDisposable BeginScope<TState>(TState state) => Disposable.Empty;

        public bool IsEnabled(MS.LogLevel logLevel) => true;

        public void Log<TState>(
            MS.LogLevel logLevel,
            MS.EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (logLevel == MS.LogLevel.Trace)
            {
                return;
            }

            var message = formatter(state, exception);

            // Very annoyingly, MSBuild appends new lines onto messages itself.
            if (message.EndsWith("\r\n"))
            {
                message = message.Substring(0, message.Length - 2);
            }

            switch (logLevel)
            {
                case MS.LogLevel.Debug:
                    logSink.Debug(message);
                    break;
                case MS.LogLevel.Information:
                    logSink.Info(message);
                    break;
                case MS.LogLevel.Warning:
                    logSink.Warn(message);
                    break;
                default:
                    logSink.Error(message);
                    break;
            }
        }
    }
}
