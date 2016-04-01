namespace PCLMock.UnitTests.CodeGeneration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using PCLMock.CodeGeneration;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
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
        public async Task can_generate_simple_mock(string resourceBaseName, Language language)
        {
            var inputResourceName = "PCLMock.UnitTests.CodeGeneration.GeneratorFixtureResources." + resourceBaseName + "Input_" + language.ToString() + ".txt";
            var outputResourceName = "PCLMock.UnitTests.CodeGeneration.GeneratorFixtureResources." + resourceBaseName + "Output_" + language.ToString() + ".txt";

            using (var inputStream = this.GetType().Assembly.GetManifestResourceStream(inputResourceName))
            using (var outputStream = this.GetType().Assembly.GetManifestResourceStream(outputResourceName))
            using (var outputStreamReader = new StreamReader(outputStream))
            {
                var workspace = new AdhocWorkspace();
                var projectId = ProjectId.CreateNewId();
                var versionStamp = VersionStamp.Create();
                var projectInfo = ProjectInfo.Create(
                    projectId,
                    versionStamp,
                    "AdhocProject",
                    "AdhocProject",
                    language.ToSyntaxGeneratorLanguageName(),
                    metadataReferences: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(MockBase<>).Assembly.Location)
                    });
                var project = workspace.AddProject(projectInfo);
                workspace.AddDocument(projectId, "Source.cs", SourceText.From(inputStream));
                var solution = workspace.CurrentSolution;

                var results =
                    (await Generator.GenerateMocksAsync(
                        language,
                        solution,
                        x => true,
                        x => "The.Namespace",
                        x => "Mock"));
                var result = results
                    .Single()
                    .ToString();

                var expectedCode = outputStreamReader.ReadToEnd();

                // make sure version changes don't break the tests
                expectedCode = expectedCode.Replace("$VERSION$", typeof(MockBase<>).Assembly.GetName().Version.ToString());

                // useful when converting generated code to something that can be pasted into an expectation file
                var sanitisedResult = result.Replace(typeof(MockBase<>).Assembly.GetName().Version.ToString(), "$VERSION$");

                Assert.Equal(expectedCode, result);
            }
        }
    }
}