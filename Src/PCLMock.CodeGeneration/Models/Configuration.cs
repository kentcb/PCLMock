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
        private static readonly Type logSource = typeof(Configuration);

        private readonly ILogSink logSink;
        private readonly IImmutableList<Transformation> namespaceTransformations;
        private readonly IImmutableList<Transformation> nameTransformations;
        private readonly IImmutableList<Filter> interfaceFilters;
        private readonly IImmutableList<Plugin> plugins;

        public Configuration(
            ILogSink logSink,
            IEnumerable<Transformation> namespaceTransformations,
            IEnumerable<Transformation> nameTransformations,
            IEnumerable<Filter> interfaceFilters,
            IEnumerable<Plugin> plugins)
        {
            this.logSink = logSink;
            this.namespaceTransformations = namespaceTransformations.ToImmutableList();
            this.nameTransformations = nameTransformations.ToImmutableList();
            this.interfaceFilters = interfaceFilters.ToImmutableList();
            this.plugins = plugins.ToImmutableList();

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
                var pluginsLog = this
                    .plugins
                    .Aggregate(
                        new StringBuilder(),
                        (sb, next) => sb.Append(" - Plugin with assembly-qualified name '").Append(next.AssemblyQualifiedName).Append("' will be applied."));
                logSink.Debug(logSource, $"Created configuration with the following rules:{Environment.NewLine}{namespaceTransformationsLog}{nameTransformationsLog}{interfaceFiltersLog}{pluginsLog}");
            }
        }

        public IImmutableList<Transformation> NamespaceTransformations => this.namespaceTransformations;

        public IImmutableList<Transformation> NameTransformations => this.nameTransformations;

        public IImmutableList<Filter> InterfaceFilters => this.interfaceFilters;

        public IImmutableList<Plugin> Plugins => this.plugins;

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
            var plugins = document
                .Root
                .XPathSelectElements("./Plugins/Plugin")
                .Select(x => new Plugin(x.Value));
            return new Configuration(logSink, namespaceTransformations, nameTransformations, interfaceFilters, plugins);
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

                this.logSink.Info(logSource, "Determining inclusivity of interface: '{0}'.", name);

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
                            logSource,
                            " - after {0} filter '{1}', it is {2}.",
                            filter.Type == FilterType.Include ? "inclusion" : "exclusion",
                            filter.Pattern,
                            include ? "included" : "excluded");
                    }
                }

                if (logSink.IsEnabled)
                {
                    this.logSink.Log(
                        logSource,
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

        public IImmutableList<IPlugin> GetPlugins()
        {
            var resolvedPlugins = new List<IPlugin>(this.plugins.Count);

            foreach (var plugin in this.plugins)
            {
                this.logSink.Info(logSource, "Attempting to resolve plugin from type name '{0}'.", plugin.AssemblyQualifiedName);
                var type = Type.GetType(plugin.AssemblyQualifiedName);

                if (type == null)
                {
                    this.logSink.Error(logSource, "Failed to resolve plugin from type name '{0}'.", plugin.AssemblyQualifiedName);
                    continue;
                }

                try
                {
                    var resolvedPlugin = Activator.CreateInstance(type);

                    if (!(resolvedPlugin is IPlugin))
                    {
                        this.logSink.Error(logSource, "Resolved plugin '{0}' does not implement '{1}'.", resolvedPlugin.GetType().AssemblyQualifiedName, typeof(IPlugin).AssemblyQualifiedName);
                        continue;
                    }

                    resolvedPlugins.Add((IPlugin)resolvedPlugin);
                }
                catch (Exception ex)
                {
                    this.logSink.Error(logSource, "Failed to create plugin from type name '{0}'. Exception was: {1}", plugin.AssemblyQualifiedName, ex);
                }
            }

            return resolvedPlugins.ToImmutableList();
        }

        private static string ApplyTransformations(ILogSink logSink, string type, string input, IImmutableList<Transformation> transformations)
        {
            logSink.Info(logSource, "Applying {0} transformations to input: '{1}'.", type, input);

            foreach (var transformation in transformations)
            {
                input = Regex.Replace(input, transformation.Pattern, transformation.Replacement);

                if (logSink.IsEnabled)
                {
                    logSink.Debug(
                        logSource,
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