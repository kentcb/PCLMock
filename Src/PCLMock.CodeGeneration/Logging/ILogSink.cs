namespace PCLMock.CodeGeneration.Logging
{
    public interface ILogSink
    {
        void Log(LogEvent logEvent);
    }
}