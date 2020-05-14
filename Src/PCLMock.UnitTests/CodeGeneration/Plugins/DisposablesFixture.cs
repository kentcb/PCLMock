namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Plugins;
    using Xunit;

    public sealed class DisposablesFixture
    {
        [Theory]
        [InlineData("Disposables", Language.CSharp)]
        public Task can_generate_mocks(string testName, Language language) =>
            Helpers.GenerateAndVerifyMocksUsingEmbeddedResources(
                this.GetType(),
                testName,
                language,
                metadataReferences: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Disposable).Assembly.Location),
                    },
                new IPlugin[]
                    {
                        new Disposables(),
                    });
    }
}