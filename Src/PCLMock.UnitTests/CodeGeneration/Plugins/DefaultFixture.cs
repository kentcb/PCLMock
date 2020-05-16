namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration;
    using Xunit;

    public sealed class DefaultFixture
    {
        [Theory]
        [InlineData("Simple", Language.CSharp)]
        public Task can_generate_mocks(string testName, Language language) =>
            // The default plugin is always included last, so we specify no extra plugins here.
            Helpers.GenerateAndVerifyMocksUsingEmbeddedResources(
                this.GetType(),
                testName,
                language,
                metadataReferences: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    });
    }
}
