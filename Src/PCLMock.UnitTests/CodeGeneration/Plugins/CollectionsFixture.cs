namespace PCLMock.UnitTests.CodeGeneration.Plugins
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Text;
    using PCLMock.CodeGeneration;
    using PCLMock.CodeGeneration.Logging;
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
        public async Task can_generate_mocks(string resourceBaseName, Language language)
        {
            var inputResourceName = "PCLMock.UnitTests.CodeGeneration.Plugins.CollectionsFixtureResources." + resourceBaseName + "Input_" + language.ToString() + ".txt";
            var outputResourceName = "PCLMock.UnitTests.CodeGeneration.Plugins.CollectionsFixtureResources." + resourceBaseName + "Output_" + language.ToString() + ".txt";

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
                        MetadataReference.CreateFromFile(typeof(MockBase<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(ImmutableList).Assembly.Location)
                    });
                var project = workspace.AddProject(projectInfo);
                workspace.AddDocument(projectId, "Source.cs", SourceText.From(inputStream));
                var syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language.ToSyntaxGeneratorLanguageName());
                var solution = workspace.CurrentSolution;
                var compilation = await solution
                    .Projects
                    .Single()
                    .GetCompilationAsync();
                var syntaxTree = compilation
                    .SyntaxTrees
                    .Single();
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var sut = new Collections();
                var memberSyntaxes = syntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .Where(syntaxNode => syntaxNode is BaseMethodDeclarationSyntax || syntaxNode is BasePropertyDeclarationSyntax);

                var result = memberSyntaxes
                    .Select(syntax => semanticModel.GetDeclaredSymbol(syntax))
                    .Select(symbol => sut.GenerateConfigureBehavior(NullLogSink.Instance, syntaxGenerator, semanticModel, symbol))
                    .Aggregate(
                        new StringBuilder(),
                        (acc, next) =>
                        {
                            if (next != null)
                            {
                                acc.AppendLine(next.ToFullString());
                            }

                            return acc;
                        },
                        x => x.ToString())
                    .NormalizeLineEndings();

                var expectedCode = outputStreamReader
                    .ReadToEnd()
                    .NormalizeLineEndings();

                Assert.Equal(expectedCode, result);
            }
        }
    }
}