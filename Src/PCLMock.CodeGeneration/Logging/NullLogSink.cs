namespace PCLMock.CodeGeneration.Logging
{
    public sealed class NullLogSink : ILogSink
    {
        public static readonly NullLogSink Instance = new NullLogSink();

        private NullLogSink()
        {
        }

        public void Log(LogEvent logEvent)
        {
        }
    }
}