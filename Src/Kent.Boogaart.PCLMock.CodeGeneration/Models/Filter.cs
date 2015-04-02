namespace Kent.Boogaart.PCLMock.CodeGeneration.Models
{
    public sealed class Filter
    {
        private readonly FilterType type;
        private readonly string pattern;

        public Filter(FilterType type, string pattern)
        {
            this.type = type;
            this.pattern = pattern;
        }

        public FilterType Type => this.type;

        public string Pattern => this.pattern;
    }
}