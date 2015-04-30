namespace Kent.Boogaart.PCLMock.CodeGeneration.Models
{
    public sealed class Transformation
    {
        private readonly string pattern;
        private readonly string replacement;

        public Transformation(string pattern, string replacement)
        {
            this.pattern = pattern;
            this.replacement = replacement;
        }

        public string Pattern => this.pattern;

        public string Replacement => this.replacement;
    }
}