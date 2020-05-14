namespace PCLMock.CodeGeneration.Logging
{
    using System.Text;

    public sealed class StringLogSink : ILogSink
    {
        private readonly StringBuilder stringBuilder;

        public StringLogSink()
        {
            this.stringBuilder = new StringBuilder();
        }

        public void Log(LogEvent logEvent)
        {
            this
                .stringBuilder
                .Append("[")
                .Append(logEvent.Source ?? "<<UNKNOWN SOURCE>>")
                .Append("] [")
                .Append(logEvent.Level)
                .Append("] ")
                .AppendLine(logEvent.FormattedMessage);
        }

        public override string ToString() =>
            this.stringBuilder.ToString();
    }
}