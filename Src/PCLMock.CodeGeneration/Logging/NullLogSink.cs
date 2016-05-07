namespace PCLMock.CodeGeneration.Logging
{
    using System;

    public sealed class NullLogSink : ILogSink
    {
        public static readonly NullLogSink Instance = new NullLogSink();

        private NullLogSink()
        {
        }

        public bool IsEnabled => false;

        public void Log(Type source, LogLevel level, string message)
        {
        }
    }
}