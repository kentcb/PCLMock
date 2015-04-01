namespace Kent.Boogaart.PCLMock.CodeGeneration.T4
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Kent.Boogaart.PCLMock.CodeGeneration;
    using Microsoft.CodeAnalysis;

    public static class XmlBasedGenerator
    {
        public static string GenerateMocks(
            string solutionPath,
            string xmlPath,
            string language)
        {
            var castLanguage = (Language)Enum.Parse(typeof(Language), language);
            return GenerateMocksAsync(solutionPath, xmlPath, castLanguage)
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
            var model = GetModelFromDocument(document);

            return await Generator.GenerateMocksAsync(
                solutionPath,
                GetInterfacePredicateForModel(model),
                GetNamespaceSelectorForModel(model),
                GetNameSelectorForModel(model),
                language);
        }

        private static Model GetModelFromDocument(XDocument document)
        {
            var namespaceTransformations = document
                .Root
                .XPathSelectElements("./NamespaceTransformations/Transformation")
                .Select(x => new TransformModel(x.Element("Pattern").Value, x.Element("Replacement").Value));
            var nameTransformations = document
                .Root
                .XPathSelectElements("./NameTransformations/Transformation")
                .Select(x => new TransformModel(x.Element("Pattern").Value, x.Element("Replacement").Value));
            var interfaceFilters = document
                .Root
                .XPathSelectElements("./Interfaces")
                .Single()
                .Nodes()
                .Cast<XElement>()
                .Select(x => new FilterModel(ParseFilterType(x.Name.LocalName), x.Element("Pattern").Value));
            var model = new Model(namespaceTransformations, nameTransformations, interfaceFilters);

            return model;
        }

        private static Func<INamedTypeSymbol, bool> GetInterfacePredicateForModel(Model model)
        {
            return symbol =>
            {
                var name = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}, {1}",
                    symbol.ToDisplayString(),
                    symbol.ContainingAssembly.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                var include = false;

                foreach (var filter in model.InterfaceFilters)
                {
                    if (include && filter.Type == FilterType.Exclude)
                    {
                        include = !Regex.IsMatch(name, filter.Pattern);
                    }
                    else if (!include && filter.Type == FilterType.Include)
                    {
                        include = Regex.IsMatch(name, filter.Pattern);
                    }
                }

                return include;
            };
        }

        private static Func<INamedTypeSymbol, string> GetNamespaceSelectorForModel(Model model)
        {
            return symbol => ApplyTransformations(symbol.ContainingNamespace.ToString(), model.NamespaceTransformations);
        }

        private static Func<INamedTypeSymbol, string> GetNameSelectorForModel(Model model)
        {
            return symbol => ApplyTransformations(symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), model.NameTransformations);
        }

        private static string ApplyTransformations(string input, IImmutableList<TransformModel> transformations)
        {
            foreach (var transformation in transformations)
            {
                input = Regex.Replace(input, transformation.Pattern, transformation.Replacement);
            }

            return input;
        }

        private static FilterType ParseFilterType(string text)
        {
            switch (text)
            {
                case "Include":
                    return FilterType.Include;
                case "Exclude":
                    return FilterType.Exclude;
                default:
                    throw new NotSupportedException("Filter type '" + text + "' not supported.");
            }
        }

        private sealed class Model
        {
            private readonly IImmutableList<TransformModel> namespaceTransformations;
            private readonly IImmutableList<TransformModel> nameTransformations;
            private readonly IImmutableList<FilterModel> interfaceFilters;

            public Model(
                IEnumerable<TransformModel> namespaceTransformations,
                IEnumerable<TransformModel> nameTransformations,
                IEnumerable<FilterModel> interfaceFilters)
            {
                this.namespaceTransformations = namespaceTransformations.ToImmutableList();
                this.nameTransformations = nameTransformations.ToImmutableList();
                this.interfaceFilters = interfaceFilters.ToImmutableList();
            }

            public IImmutableList<TransformModel> NamespaceTransformations => this.namespaceTransformations;

            public IImmutableList<TransformModel> NameTransformations => this.nameTransformations;

            public IImmutableList<FilterModel> InterfaceFilters => this.interfaceFilters;
        }

        private sealed class TransformModel
        {
            private readonly string pattern;
            private readonly string replacement;

            public TransformModel(string pattern, string replacement)
            {
                this.pattern = pattern;
                this.replacement = replacement;
            }

            public string Pattern => this.pattern;

            public string Replacement => this.replacement;
        }

        private enum FilterType
        {
            Include,
            Exclude
        }

        private sealed class FilterModel
        {
            private readonly FilterType type;
            private readonly string pattern;

            public FilterModel(FilterType type, string pattern)
            {
                this.type = type;
                this.pattern = pattern;
            }

            public FilterType Type => this.type;

            public string Pattern => this.pattern;
        }
    }
}