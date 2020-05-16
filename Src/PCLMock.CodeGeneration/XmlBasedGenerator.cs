namespace PCLMock.CodeGeneration
{
    using System.Collections.Immutable;
    using System.IO;
    using System.Xml.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration.Models;

    public static class XmlBasedGenerator
    {
        public static IImmutableList<SyntaxNode> GenerateMocks(
            ILogSink logSink,
            Language language,
            IImmutableList<Compilation> compilations,
            string xmlPath)
        {
            logSink = logSink
                .WithSource(typeof(XmlBasedGenerator));

            if (!File.Exists(xmlPath))
            {
                logSink.Error("XML input file '{0}' not found.", xmlPath);
                return ImmutableList<SyntaxNode>.Empty;
            }

            logSink.Info("Loading XML input file '{0}'.", xmlPath);

            var document = XDocument.Load(xmlPath);
            var configuration = Configuration.FromXDocument(logSink, document);

            return Generator.GenerateMocks(
                logSink,
                language,
                compilations,
                configuration.GetPlugins(),
                configuration.GetInterfacePredicate(),
                configuration.GetNamespaceSelector(),
                configuration.GetNameSelector());
        }
    }
}