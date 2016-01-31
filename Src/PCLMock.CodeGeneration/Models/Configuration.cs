namespace PCLMock.CodeGeneration.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Microsoft.CodeAnalysis;

    public sealed class Configuration
    {
        private readonly IImmutableList<Transformation> namespaceTransformations;
        private readonly IImmutableList<Transformation> nameTransformations;
        private readonly IImmutableList<Filter> interfaceFilters;

        public Configuration(
            IEnumerable<Transformation> namespaceTransformations,
            IEnumerable<Transformation> nameTransformations,
            IEnumerable<Filter> interfaceFilters)
        {
            this.namespaceTransformations = namespaceTransformations.ToImmutableList();
            this.nameTransformations = nameTransformations.ToImmutableList();
            this.interfaceFilters = interfaceFilters.ToImmutableList();
        }

        public IImmutableList<Transformation> NamespaceTransformations => this.namespaceTransformations;

        public IImmutableList<Transformation> NameTransformations => this.nameTransformations;

        public IImmutableList<Filter> InterfaceFilters => this.interfaceFilters;

        public static Configuration FromXDocument(XDocument document)
        {
            var namespaceTransformations = document
                .Root
                .XPathSelectElements("./NamespaceTransformations/Transformation")
                .Select(x => new Transformation(x.Element("Pattern").Value, x.Element("Replacement").Value));
            var nameTransformations = document
                .Root
                .XPathSelectElements("./NameTransformations/Transformation")
                .Select(x => new Transformation(x.Element("Pattern").Value, x.Element("Replacement").Value));
            var interfaceFilters = document
                .Root
                .XPathSelectElements("./Interfaces")
                .Single()
                .Nodes()
                .OfType<XElement>()
                .Select(x => new Filter(ParseFilterType(x.Name.LocalName), x.Element("Pattern").Value));
            return new Configuration(namespaceTransformations, nameTransformations, interfaceFilters);
        }

        public Func<INamedTypeSymbol, bool> GetInterfacePredicate()
        {
            return symbol =>
            {
                var name = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}, {1}",
                    symbol.ToDisplayString(),
                    symbol.ContainingAssembly.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                var include = false;

                foreach (var filter in this.InterfaceFilters)
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

        public Func<INamedTypeSymbol, string> GetNamespaceSelector()
        {
            return symbol => ApplyTransformations(symbol.ContainingNamespace.ToString(), this.NamespaceTransformations);
        }

        public Func<INamedTypeSymbol, string> GetNameSelector()
        {
            return symbol => ApplyTransformations(symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), this.NameTransformations);
        }

        private static string ApplyTransformations(string input, IImmutableList<Transformation> transformations)
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
    }
}