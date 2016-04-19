namespace PCLMock.CodeGeneration.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Logging;
    using Microsoft.CodeAnalysis;

    public sealed class Configuration
    {
        private readonly ILogSink logSink;
        private readonly IImmutableList<Transformation> namespaceTransformations;
        private readonly IImmutableList<Transformation> nameTransformations;
        private readonly IImmutableList<Filter> interfaceFilters;

        public Configuration(
            ILogSink logSink,
            IEnumerable<Transformation> namespaceTransformations,
            IEnumerable<Transformation> nameTransformations,
            IEnumerable<Filter> interfaceFilters)
        {
            this.logSink = logSink;
            this.namespaceTransformations = namespaceTransformations.ToImmutableList();
            this.nameTransformations = nameTransformations.ToImmutableList();
            this.interfaceFilters = interfaceFilters.ToImmutableList();

            if (logSink.IsEnabled)
            {
                var namespaceTransformationsLog = this
                    .namespaceTransformations
                    .Aggregate(
                        new StringBuilder(),
                        (sb, next) => sb.Append(" - Namespaces matching '").Append(next.Pattern).Append("' will be replaced with '").Append(next.Replacement).AppendLine("'."), sb => sb.ToString());
                var nameTransformationsLog = this
                    .nameTransformations
                    .Aggregate(
                        new StringBuilder(),
                        (sb, next) => sb.Append(" - Names matching '").Append(next.Pattern).Append("' will be replaced with '").Append(next.Replacement).AppendLine("'."), sb => sb.ToString());
                var interfaceFiltersLog = this
                    .interfaceFilters
                    .Aggregate(
                        new StringBuilder(),
                        (sb, next) => sb.Append(" - Interfaces matching '").Append(next.Pattern).Append("' will be '").Append(next.Type == FilterType.Include ? "included" : "excluded").AppendLine("."), sb => sb.ToString());
                logSink.Debug($"Created configuration with the following rules:{Environment.NewLine}{namespaceTransformationsLog}{nameTransformationsLog}{interfaceFiltersLog}");
            }
        }

        public IImmutableList<Transformation> NamespaceTransformations => this.namespaceTransformations;

        public IImmutableList<Transformation> NameTransformations => this.nameTransformations;

        public IImmutableList<Filter> InterfaceFilters => this.interfaceFilters;

        public static Configuration FromXDocument(ILogSink logSink, XDocument document)
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
            return new Configuration(logSink, namespaceTransformations, nameTransformations, interfaceFilters);
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

                this.logSink.Info("Determining inclusivity of interface: '{0}'.", name);

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

                    if (this.logSink.IsEnabled)
                    {
                        this.logSink.Debug(
                            " - after {0} filter '{1}', it is {2}.",
                            filter.Type == FilterType.Include ? "inclusion" : "exclusion",
                            filter.Pattern,
                            include ? "included" : "excluded");
                    }
                }

                if (logSink.IsEnabled)
                {
                    this.logSink.Log(
                        include ? LogLevel.Positive : LogLevel.Negative,
                        "'{0}' has been {1}.",
                        name,
                        include ? "included" : "excluded");
                }

                return include;
            };
        }

        public Func<INamedTypeSymbol, string> GetNamespaceSelector()
        {
            return symbol => ApplyTransformations(this.logSink, "namespace", symbol.ContainingNamespace.ToString(), this.NamespaceTransformations);
        }

        public Func<INamedTypeSymbol, string> GetNameSelector()
        {
            return symbol => ApplyTransformations(this.logSink, "name", symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), this.NameTransformations);
        }

        private static string ApplyTransformations(ILogSink logSink, string type, string input, IImmutableList<Transformation> transformations)
        {
            logSink.Info("Applying {0} transformations to input: '{1}'.", type, input);

            foreach (var transformation in transformations)
            {
                input = Regex.Replace(input, transformation.Pattern, transformation.Replacement);

                if (logSink.IsEnabled)
                {
                    logSink.Debug(
                        " - after transformation '{0}' -> '{1}', input is now '{2}'.",
                        transformation.Pattern,
                        transformation.Replacement,
                        input);
                }
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