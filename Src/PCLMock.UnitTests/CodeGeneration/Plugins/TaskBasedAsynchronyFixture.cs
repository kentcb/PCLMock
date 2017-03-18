namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Logging;
    using PCLMock.CodeGeneration.Plugins;
    using Xunit;

    public sealed class TaskBasedAsynchronyFixture
    {
        [Theory]
        [InlineData("NonTaskBased", Language.CSharp)]
        [InlineData("NonGenericTask", Language.CSharp)]
        [InlineData("GenericTask", Language.CSharp)]
        [InlineData("GenericInterface", Language.CSharp)]
        public async Task can_generate_mocks(string resourceBaseName, Language language)
        {
            var inputResourceName = "PCLMock.UnitTests.CodeGeneration.Plugins.TaskBasedAsynchronyFixtureResources." + resourceBaseName + "Input_" + language.ToString() + ".txt";
            var outputResourceName = "PCLMock.UnitTests.CodeGeneration.Plugins.TaskBasedAsynchronyFixtureResources." + resourceBaseName + "Output_" + language.ToString() + ".txt";

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
                        NullLogSink.Instance,
                        language,
                        solution,
                        x => true,
                        x => "The.Namespace",
                        x => "Mock",
                        new IPlugin[]
                        {
                            new TaskBasedAsynchrony()
                        }.ToImmutableList()));
                var result = results
                    .Last()
                    .ToString()
                    .NormalizeLineEndings();

                var expectedCode = outputStreamReader.ReadToEnd();

                // make sure version changes don't break the tests
                expectedCode = expectedCode
                    .Replace("$VERSION$", typeof(MockBase<>).Assembly.GetName().Version.ToString())
                    .NormalizeLineEndings();

                // useful when converting generated code to something that can be pasted into an expectation file
                var sanitisedResult = result.Replace(typeof(MockBase<>).Assembly.GetName().Version.ToString(), "$VERSION$");

                Assert.Equal(expectedCode, result);
            }
        }
    }
}