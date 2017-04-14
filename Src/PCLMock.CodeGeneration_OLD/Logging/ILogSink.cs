namespace PCLMock.CodeGeneration.Logging
{
    using System;

    public interface ILogSink
    {
        bool IsEnabled
        {
            get;
        }

        void Log(Type source, LogLevel level, string message);
    }
}