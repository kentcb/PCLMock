namespace PCLMock.CodeGeneration.Console.Logging
{
    using PCLMock.CodeGeneration.Logging;
    using MS = Microsoft.Extensions.Logging;

    public sealed class LogSinkLoggerProvider : MS.ILoggerProvider
    {
        private readonly ILogSink logSink;

        public LogSinkLoggerProvider(ILogSink logSink)
        {
            this.logSink = logSink;
        }

        public MS.ILogger CreateLogger(string categoryName) =>
            new LogSinkLogger(this.logSink.WithSource(categoryName));

        public void Dispose()
        {
        }
    }
}
