namespace PCLMock.CodeGeneration.Logging
{
    public interface ILogSink
    {
        bool IsEnabled
        {
            get;
        }

        void Log(LogLevel level, string message);
    }
}