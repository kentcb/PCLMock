namespace PCLMock.CodeGeneration.SourceGenerator.Logging
{
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration.Logging;

    public sealed class SourceGeneratorContextLogSink : ILogSink
    {
#pragma warning disable RS2008 // Enable analyzer release tracking

        private static readonly DiagnosticDescriptor diagnosticDescriptor = new DiagnosticDescriptor(
                // Every log event originating from PCLMock itself will have the same ID.
                id: "PM0100",
                title: "PCLMock Source Generation",
                messageFormat: "{0}",
                category: "mocks",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

#pragma warning restore RS2008 // Enable analyzer release tracking

        private readonly SourceGeneratorContext sourceGeneratorContext;

        public SourceGeneratorContextLogSink(SourceGeneratorContext sourceGeneratorContext)
        {
            this.sourceGeneratorContext = sourceGeneratorContext;
        }

        public void Log(LogEvent logEvent)
        {
            var effectiveSeverity = logEvent.Level switch
            {
                LogLevel.Warn => DiagnosticSeverity.Warning,
                LogLevel.Error => DiagnosticSeverity.Error,
                _ => DiagnosticSeverity.Info
            };
            var diagnostic = Diagnostic.Create(
                diagnosticDescriptor,
                Location.None,
                effectiveSeverity,
                additionalLocations: null,
                properties: null,
                logEvent.FormattedMessage);
            this.sourceGeneratorContext.ReportDiagnostic(diagnostic);
        }
    }
}
