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

        public bool IsEnabled => true;

        public void Log(LogLevel level, string message) =>
            this.stringBuilder.AppendLine(message);

        public override string ToString() =>
            this.stringBuilder.ToString();
    }
}