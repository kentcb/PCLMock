namespace PCLMock.CodeGeneration
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration.Models;

    public static class XmlBasedGenerator
    {
        private static readonly Type logSource = typeof(XmlBasedGenerator);

        public static string GenerateMocks(
            ILogSink logSink,
            string solutionPath,
            string xmlPath,
            string language)
        {
            var castLanguage = (Language)Enum.Parse(typeof(Language), language);
            return GenerateMocks(logSink, castLanguage, solutionPath, xmlPath);
        }

        public static string GenerateMocks(
            ILogSink logSink,
            Language language,
            string solutionPath,
            string xmlPath)
        {
            return GenerateMocksAsync(logSink, language, solutionPath, xmlPath)
                .Result
                .Select(x => x.ToFullString())
                .Aggregate(new StringBuilder(), (current, next) => current.AppendLine(next), x => x.ToString());
        }

        public async static Task<IImmutableList<SyntaxNode>> GenerateMocksAsync(
            ILogSink logSink,
            Language language,
            string solutionPath,
            string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                var message = $"XML input file '{xmlPath}' no found.";
                logSink.Error(logSource, message);
                throw new IOException(message);
            }

            logSink.Info(logSource, "Loading XML input file '{0}'.", xmlPath);

            var document = XDocument.Load(xmlPath);
            var configuration = Configuration.FromXDocument(logSink, document);

            return await Generator.GenerateMocksAsync(
                logSink,
                language,
                solutionPath,
                configuration.GetInterfacePredicate(),
                configuration.GetNamespaceSelector(),
                configuration.GetNameSelector(),
                configuration.GetPlugins());
        }
    }
}