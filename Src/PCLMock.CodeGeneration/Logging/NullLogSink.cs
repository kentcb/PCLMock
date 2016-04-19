namespace PCLMock.CodeGeneration.Logging
{
    public sealed class NullLogSink : ILogSink
    {
        public static readonly NullLogSink Instance = new NullLogSink();

        private NullLogSink()
        {
        }

        public bool IsEnabled => false;

        public void Log(LogLevel level, string message)
        {
        }
    }
}