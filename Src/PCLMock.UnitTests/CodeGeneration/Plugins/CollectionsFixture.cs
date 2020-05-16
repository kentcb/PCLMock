namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Plugins;
    using Xunit;

    public sealed class CollectionsFixture
    {
        [Theory]
        [InlineData("Enumerables", Language.CSharp)]
        [InlineData("Collections", Language.CSharp)]
        [InlineData("Lists", Language.CSharp)]
        [InlineData("Dictionaries", Language.CSharp)]
        [InlineData("Sets", Language.CSharp)]
        [InlineData("ImmutableLists", Language.CSharp)]
        [InlineData("ImmutableDictionaries", Language.CSharp)]
        [InlineData("ImmutableQueues", Language.CSharp)]
        [InlineData("ImmutableSets", Language.CSharp)]
        [InlineData("ImmutableStacks", Language.CSharp)]
        [InlineData("Recursive", Language.CSharp)]
        public Task can_generate_mocks(string testName, Language language) =>
            Helpers.GenerateAndVerifyMocksUsingEmbeddedResources(
                this.GetType(),
                testName,
                language,
                metadataReferences: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(HashSet<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(ImmutableList).Assembly.Location),
                    },
                new IPlugin[]
                    {
                        new Collections(),
                    });
    }
}