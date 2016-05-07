namespace PCLMock.CodeGeneration.Logging
{
    using System;
    using System.Text;

    public sealed class StringLogSink : ILogSink
    {
        private readonly StringBuilder stringBuilder;

        public StringLogSink()
        {
            this.stringBuilder = new StringBuilder();
        }

        public bool IsEnabled => true;

        public void Log(Type source, LogLevel level, string message) =>
            this
                .stringBuilder
                .Append("[")
                .Append(source.FullName)
                .Append("] [")
                .Append(level)
                .Append("] ")
                .AppendLine(message);

        public override string ToString() =>
            this.stringBuilder.ToString();
    }
}