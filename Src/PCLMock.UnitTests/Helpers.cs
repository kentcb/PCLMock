using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using PCLMock.CodeGeneration;
using PCLMock.CodeGeneration.Logging;
using Xunit;

namespace PCLMock.UnitTests
{
    internal static class Helpers
    {
        public static async Task GenerateAndVerifyMocksUsingEmbeddedResources(
            Type fixtureType,
            string testName,
            Language language,
            IEnumerable<MetadataReference> metadataReferences = default,
            IPlugin[] plugins = default,
            // Should only be enabled temporarily to overwrite expected output with the actual output, making it easy to update
            // expected output files when code changes necessitate.
            bool overwriteExpectedOutput = false)
        {
            var resourceBaseName = $"{fixtureType.FullName}Resources.";
            var inputResourceName = $"{resourceBaseName}{testName}Input_{language}.txt";
            var outputResourceName = $"{resourceBaseName}{testName}Output_{language}.txt";
            var platformPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            // Manually add some references that are always required.
            var supplementedMetadataReferences = (metadataReferences ?? Enumerable.Empty<MetadataReference>())
                .Concat(
                    new[]
                    {
                        MetadataReference.CreateFromFile(Path.Combine(platformPath, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(MockBase<>).Assembly.Location),
                    });

            using (var inputStream = typeof(Helpers).Assembly.GetManifestResourceStream(inputResourceName))
            {
                var projectInfo = ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "AdhocProject",
                    "AdhocProject",
                    language.ToSyntaxGeneratorLanguageName(),
                    compilationOptions: language == Language.CSharp
                        ? (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        : new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                    metadataReferences: supplementedMetadataReferences);

                // Create an adhoc workspace and project containing the input source.
                var workspace = new AdhocWorkspace();
                var project = workspace
                    .AddProject(projectInfo)
                    .AddDocument("Source.cs", SourceText.From(inputStream))
                    .Project;
                var compilation = await project.GetCompilationAsync();
                var diagnostics = compilation.GetDiagnostics();

                // Ensure there are no issues with the input project, otherwise it might be confusing to understand issues with the generated mocks.
                Assert.Empty(diagnostics);

                var logSink = new StringLogSink()
                    .WithMinimumLevel(LogLevel.Warn);
                var generatedMocks = Generator.GenerateMocks(
                    logSink,
                    language,
                    ImmutableList.Create(compilation),
                    (plugins ?? Enumerable.Empty<IPlugin>()).ToImmutableList(),
                    x => true,
                    x => "The.Namespace",
                    x => $"{x.Name}Mock");

                var log = logSink.ToString();
                // There shouldn't be any errors or warnings in the log or something has gone wrong generating mocks.
                Assert.Equal("", log);

                var generatedMocksAggregated = generatedMocks
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
                        acc => acc.ToString())
                    .NormalizeLineEndings();

                using (var outputStream = typeof(Helpers).Assembly.GetManifestResourceStream(outputResourceName))
                using (var outputStreamReader = new StreamReader(outputStream))
                {
                    var expectedCode = outputStreamReader.ReadToEnd();

                    // make sure version changes don't break the tests
                    expectedCode = expectedCode
                        .Replace("$VERSION$", typeof(MockBase<>).Assembly.GetName().Version.ToString())
                        .NormalizeLineEndings();

                    // useful when converting generated code to something that can be pasted into an expectation file
                    var sanitisedResult = generatedMocksAggregated.Replace(typeof(MockBase<>).Assembly.GetName().Version.ToString(), "$VERSION$");

                    if (overwriteExpectedOutput)
                    {
                        var currentDirectory = Path.GetDirectoryName(typeof(Helpers).Assembly.Location);
                        var outputDirectory = Path.Combine(currentDirectory, @"..\..\..\", "CodeGeneration", "Plugins", $"{fixtureType.Name}Resources");
                        var outputFileName = Path.Combine(outputDirectory, $"{testName}Output_{language}.txt");

                        if (!File.Exists(outputFileName))
                        {
                            throw new InvalidOperationException(@"Asked to overwrite output, but cannot determine output path.");
                        }

                        File.WriteAllText(outputFileName, sanitisedResult);

                        Assert.True(false, "Output successfully overwritten - now disable the overwriteExpectedOutput flag.");
                    }

                    Assert.Equal(expectedCode, generatedMocksAggregated);
                }

                // Now combine the original code with the generated code and make sure there are no problems.
                var workspaceWithGeneratedCode = new AdhocWorkspace();
                var projectWithGeneratedCode = workspaceWithGeneratedCode
                    .AddProject(projectInfo)
                    .AddDocument("Source.cs", SourceText.From(inputStream))
                    .Project
                    .AddDocument("GeneratedSource.cs", SourceText.From(generatedMocksAggregated))
                    .Project;
                var compilationWithGeneratedCode = await projectWithGeneratedCode.GetCompilationAsync();
                var diagnosticsToIgnore = new HashSet<string>(
                    new []
                    {
                        // Don't require assembly identities to match exactly because PCLMock is built against netstandard whereas we're running
                        // tests against a specific platform. This means, e.g. System.Linq.Expressions won't be the exact same assembly.
                        "CS1701",
                    });
                var diagnosticsWithGeneratedCode = compilationWithGeneratedCode
                    .GetDiagnostics()
                    .Where(diagnostic => !diagnosticsToIgnore.Contains(diagnostic.Id))
                    .ToImmutableArray();

                // Ensure there are no issues with the input project, otherwise it might be confusing to understand issues with the generated mocks.
                Assert.Empty(diagnostics);
            }
        }
    }
}
