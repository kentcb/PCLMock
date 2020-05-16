namespace PCLMock.UnitTests.CodeGeneration
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Plugins;
    using Xunit;

    public sealed class GeneratorFixture
    {
        [Theory]
        [InlineData("SimpleInterface", Language.CSharp)]
        [InlineData("InternalInterface", Language.CSharp)]
        [InlineData("InterfaceWithGenericMethods", Language.CSharp)]
        [InlineData("GenericInterface", Language.CSharp)]
        [InlineData("NonMockableMembers", Language.CSharp)]
        [InlineData("PartialInterface", Language.CSharp)]
        [InlineData("InheritingInterface", Language.CSharp)]
        [InlineData("DuplicateMember", Language.CSharp)]
        [InlineData("NameClash", Language.CSharp)]
        [InlineData("Indexers", Language.CSharp)]
        [InlineData("OutAndRef", Language.CSharp)]
        // TODO: VB is totally borked - calls to syntaxGenerator.WithStatements don't seem to add the statements! Will need to look into this at a later date
        //[InlineData("SimpleInterface", Language.VisualBasic)]
        //[InlineData("InterfaceWithGenericMethods", Language.VisualBasic)]
        //[InlineData("GenericInterface", Language.VisualBasic)]
        //[InlineData("InterfaceWithNonMockableMembers", Language.VisualBasic)]
        //[InlineData("PartialInterface", Language.VisualBasic)]
        //[InlineData("InheritingInterface", Language.VisualBasic)]
        public Task can_generate_mocks(string testName, Language language) =>
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