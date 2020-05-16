namespace PCLMock.CodeGeneration.SourceGenerator
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using PCLMock.CodeGeneration.Logging;
    using PCLMock.CodeGeneration.SourceGenerator.Logging;

    [Generator]
    public sealed class MockSourceGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
        }

        public void Execute(SourceGeneratorContext context)
        {
            var xmlFiles = context
                .AdditionalFiles
                .Where(additionalFile => Path.GetFileName(additionalFile.Path) == "PCLMock.xml")
                .ToList();

            if (xmlFiles.Count == 0)
            {
                ReportDiagnostic(context, "PM0001", "PM0001: No XML configuration found", "No XML configuration file named 'PCLMock.xml' was found. See the documentation for more details.", DiagnosticSeverity.Error);
                return;
            }
            else if (xmlFiles.Count > 1)
            {
                ReportDiagnostic(context, "PM0002", "PM0002: Multiple XML configurations found", "Multiple XML configuration files named 'PCLMock.xml' were found where only one is expected.", DiagnosticSeverity.Error);
                return;
            }

            var xmlFile = xmlFiles.Single();

            if (!File.Exists(xmlFile.Path))
            {
                ReportDiagnostic(context, "PM0003", "PM0003: XML configuration file not found", "XML configuration file at '{0}' was not found.", DiagnosticSeverity.Error, xmlFile.Path);
                return;
            }

            var logSink = new SourceGeneratorContextLogSink(context)
                .WithMinimumLevel(LogLevel.Info)
                .WithSource(typeof(MockSourceGenerator));

            try
            {
                var syntaxNodes = XmlBasedGenerator
                    .GenerateMocks(
                        logSink,
                        Language.CSharp,
                        ImmutableList.Create(context.Compilation),
                        xmlFile.Path);
                var source = syntaxNodes
                    .Aggregate(
                        new StringBuilder(),
                        (acc, next) => acc.AppendLine(next.ToFullString()),
                        acc => acc.ToString());

                context.AddSource($"PCLMock", SourceText.From(source, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                logSink.Error("Failed during mock generation: {0}", ex);
            }
        }

        private static void ReportDiagnostic(SourceGeneratorContext context, string id, string title, string messageFormat, DiagnosticSeverity defaultSeverity, params object[] messageArgs)
        {
            var diagnosticDescriptor = new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: "mocks",
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: true);
            var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None, messageArgs);
            context.ReportDiagnostic(diagnostic);
        }
    }    
}
