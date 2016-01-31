namespace PCLMock.CodeGeneration
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using PCLMock.CodeGeneration.Models;
    using Microsoft.CodeAnalysis;

    public static class XmlBasedGenerator
    {
        public static string GenerateMocks(
            string solutionPath,
            string xmlPath,
            string language)
        {
            var castLanguage = (Language)Enum.Parse(typeof(Language), language);
            return GenerateMocks(castLanguage, solutionPath, xmlPath);
        }

        public static string GenerateMocks(
            Language language,
            string solutionPath,
            string xmlPath)
        {
            return GenerateMocksAsync(language, solutionPath, xmlPath)
                .Result
                .Select(x => x.ToFullString())
                .Aggregate(new StringBuilder(), (current, next) => current.AppendLine(next), x => x.ToString());
        }

        public async static Task<IImmutableList<SyntaxNode>> GenerateMocksAsync(
            Language language,
            string solutionPath,
            string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                throw new IOException("XML input file '" + xmlPath + "' not found.");
            }

            var document = XDocument.Load(xmlPath);
            var configuration = Configuration.FromXDocument(document);

            return await Generator.GenerateMocksAsync(
                language,
                solutionPath,
                configuration.GetInterfacePredicate(),
                configuration.GetNamespaceSelector(),
                configuration.GetNameSelector());
        }
    }
}