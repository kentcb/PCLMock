namespace Kent.Boogaart.PCLMock.CodeGeneration
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Kent.Boogaart.PCLMock.CodeGeneration.Models;
    using Microsoft.CodeAnalysis;

    public static class XmlBasedGenerator
    {
        public static string GenerateMocks(
            string solutionPath,
            string xmlPath,
            string language)
        {
            var castLanguage = (Language)Enum.Parse(typeof(Language), language);
            return GenerateMocks(solutionPath, xmlPath, castLanguage);
        }

        public static string GenerateMocks(
            string solutionPath,
            string xmlPath,
            Language language)
        {
            return GenerateMocksAsync(solutionPath, xmlPath, language)
                .Result
                .Select(x => x.ToFullString())
                .Aggregate(new StringBuilder(), (current, next) => current.AppendLine(next), x => x.ToString());
        }

        public async static Task<IImmutableList<SyntaxNode>> GenerateMocksAsync(
            string solutionPath,
            string xmlPath,
            Language language)
        {
            if (!File.Exists(xmlPath))
            {
                throw new IOException("XML input file '" + xmlPath + "' not found.");
            }

            var document = XDocument.Load(xmlPath);
            var configuration = Configuration.FromXDocument(document);

            return await Generator.GenerateMocksAsync(
                solutionPath,
                configuration.GetInterfacePredicate(),
                configuration.GetNamespaceSelector(),
                configuration.GetNameSelector(),
                language);
        }
    }
}