namespace PCLMock.CodeGeneration.Logging
{
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;

    public struct LogEvent
    {
        private readonly string source;
        private readonly LogLevel level;
        private readonly string message;
        private readonly ImmutableArray<object> messageArgs;

        public LogEvent(
            string source,
            LogLevel level,
            string message,
            ImmutableArray<object> messageArgs)
        {
            this.source = source;
            this.level = level;
            this.message = message;
            this.messageArgs = messageArgs;
        }

        public string Source => this.source;

        public LogLevel Level => this.level;

        public string Message => this.message;

        public ImmutableArray<object> MessageArgs => this.messageArgs;

        public string FormattedMessage
        {
            get
            {
                if (this.message == null)
                {
                    return null;
                }

                if (this.messageArgs.IsDefaultOrEmpty)
                {
                    return this.message;
                }

                return string.Format(CultureInfo.InvariantCulture, this.message, this.messageArgs.ToArray());
            }
        }
    }
}
