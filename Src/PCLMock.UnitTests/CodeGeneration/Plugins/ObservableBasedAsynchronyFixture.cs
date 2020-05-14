namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Plugins;
    using Xunit;

    public sealed class ObservableBasedAsynchronyFixture
    {
        [Theory]
        [InlineData("NonObservableBased", Language.CSharp)]
        [InlineData("ObservableBased", Language.CSharp)]
        [InlineData("GenericInterface", Language.CSharp)]
        [InlineData("Recursive", Language.CSharp)]
        public Task can_generate_mocks(string testName, Language language) =>
            Helpers.GenerateAndVerifyMocksUsingEmbeddedResources(
                this.GetType(),
                testName,
                language,
                metadataReferences: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Unit).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Observable).Assembly.Location),
                    },
                new IPlugin[]
                    {
                        new ObservableBasedAsynchrony(),
                    });
    }
}