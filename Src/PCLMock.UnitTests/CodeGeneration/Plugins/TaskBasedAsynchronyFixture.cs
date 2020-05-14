namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Plugins;
    using Xunit;

    public sealed class TaskBasedAsynchronyFixture
    {
        [Theory]
        [InlineData("NonTaskBased", Language.CSharp)]
        [InlineData("NonGenericTask", Language.CSharp)]
        [InlineData("GenericTask", Language.CSharp)]
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
                        MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    },
                new IPlugin[]
                    {
                        new TaskBasedAsynchrony(),
                    });
    }
}