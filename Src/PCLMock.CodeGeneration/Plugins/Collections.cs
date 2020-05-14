namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using Logging;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// A plugin that generates appropriate default return values for any member that returns a collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This plugin generates return specifications for any member returning a standard collection type. That is,
    /// it returns <c>IEnumerable&lt;T&gt;</c>, <c>ICollection&lt;T&gt;</c>, <c>IReadOnlyCollection&lt;T&gt;</c>,
    /// <c>IList&lt;T&gt;</c>, <c>IReadOnlyList&lt;T&gt;</c>, <c>IDictionary&lt;TKey, TValue&gt;</c>,
    /// <c>IReadOnlyDictionary&lt;TKey, TValue&gt;</c>, <c>ISet&lt;T&gt;</c> <c>IImmutableList&lt;T&gt;</c>,
    /// <c>IImmutableDictionary&lt;TKey, TValue&gt;</c>, <c>IImmutableQueue&lt;T&gt;</c>,
    /// <c>IImmutableSet&lt;T&gt;</c>, or <c>IImmutableStack&lt;T&gt;</c>.
    /// </para>
    /// <para>
    /// The generated specification simply returns a default instance of an appropriate type. This has the
    /// (usually desirable) effect of ensuring any collection-like member returns an empty collection by default
    /// rather than <see langword="null"/>.
    /// </para>
    /// <para>
    /// Members for which specifications cannot be generated are ignored. This of course includes members that do not
    /// return collections, but also set-only properties, generic methods, and any members that return custom
    /// collection subtypes.
    /// </para>
    /// </remarks>
    public sealed class Collections : IPlugin
    {
        public string Name => "Collections";

        /// <inheritdoc />
        public Compilation InitializeCompilation(ILogSink logSink, Compilation compilation) =>
            compilation;

        /// <inheritdoc />
        public SyntaxNode GetDefaultValueSyntax(
            Context context,
            ISymbol symbol,
            ITypeSymbol typeSymbol)
        {
            context = context
                .WithLogSink(
                    context
                        .LogSink
                        .WithSource(typeof(Collections)));

            if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol))
            {
                context
                    .LogSink
                    .Debug("Ignoring type '{0}' because it is not a named type symbol.", typeSymbol);
                return null;
            }
            
            if (!namedTypeSymbol.IsGenericType)
            {
                context
                    .LogSink
                    .Debug("Ignoring type '{0}' because its return type is not a generic type, so it cannot be one of the supported collection types.", namedTypeSymbol);
                return null;
            }

            SyntaxNode returnValueSyntax;

            if (!TryGetReturnValueSyntaxForEnumerableReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForCollectionReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForDictionaryReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForSetReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableListReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableDictionaryReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableQueueReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableSetReturnType(context, namedTypeSymbol, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableStackReturnType(context, namedTypeSymbol, out returnValueSyntax))
            {
                context
                    .LogSink
                    .Debug("Type '{0}' is not a supported collection type.", namedTypeSymbol);
                return null;
            }

            context
                .LogSink
                .Debug("Generated a default value for type '{0}'.", namedTypeSymbol);

            return returnValueSyntax;
        }

        private static bool TryGetReturnValueSyntaxForEnumerableReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isEnumerable = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>";

            if (!isEnumerable)
            {
                returnValueSyntax = null;
                return false;
            }

            var enumerableType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Linq.Enumerable", preferredAssemblyNames: new[] { "System.Linq" });

            if (enumerableType == null)
            {
                context
                    .LogSink
                    .Warn("The Enumerable type could not be resolved (probably a missing reference to System.Linq).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .InvocationExpression(
                    context
                        .SyntaxGenerator
                        .WithTypeArguments(
                            context
                                .SyntaxGenerator
                                .MemberAccessExpression(
                                    context
                                        .SyntaxGenerator
                                        .TypeExpression(enumerableType),
                                    "Empty"),
                                context
                                    .SyntaxGenerator
                                    .TypeExpression(typeSymbol.TypeArguments[0])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForCollectionReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isCollection = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.ICollection<T>";
            var isReadOnlyCollection = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.IReadOnlyCollection<T>";
            var isList = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.IList<T>";
            var isReadOnlyList = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.IReadOnlyList<T>";

            if (!(isCollection || isReadOnlyCollection || isList || isReadOnlyList))
            {
                returnValueSyntax = null;
                return false;
            }

            var listType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Generic.List`1", preferredAssemblyNames: new[] { "System.Collections" });

            if (listType == null)
            {
                context
                    .LogSink
                    .Warn("The List type could not be resolved (probably a missing reference to System.Collections).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                    .ObjectCreationExpression(
                        listType.Construct(typeSymbol.TypeArguments[0])).NormalizeWhitespace();
            return true;
        }

        private static bool TryGetReturnValueSyntaxForDictionaryReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isDictionary = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.IDictionary<TKey, TValue>";
            var isReadOnlyDictionary = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>";

            if (!(isDictionary || isReadOnlyDictionary))
            {
                returnValueSyntax = null;
                return false;
            }

            var dictionaryType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Generic.Dictionary`2", preferredAssemblyNames: new[] { "System.Collections" });

            if (dictionaryType == null)
            {
                context
                    .LogSink
                    .Warn("The Dictionary type could not be resolved (probably a missing reference to System.Collections).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .ObjectCreationExpression(
                    dictionaryType.Construct(typeSymbol.TypeArguments[0], typeSymbol.TypeArguments[1])).NormalizeWhitespace();
            return true;
        }

        private static bool TryGetReturnValueSyntaxForSetReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isSet = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Generic.ISet<T>";

            if (!isSet)
            {
                returnValueSyntax = null;
                return false;
            }

            var hashSetType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Generic.HashSet`1", preferredAssemblyNames: new[] { "System.Collections" });

            if (hashSetType == null)
            {
                context
                    .LogSink
                    .Warn("The HashSet type could not be resolved (probably a missing reference to System.Collections).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .ObjectCreationExpression(
                    hashSetType.Construct(typeSymbol.TypeArguments[0])).NormalizeWhitespace();
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableListReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isImmutableList = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Immutable.IImmutableList<T>";

            if (!isImmutableList)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableListType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Immutable.ImmutableList", preferredAssemblyNames: new[] { "System.Collections.Immutable" });

            if (immutableListType == null)
            {
                context
                    .LogSink
                    .Warn("The ImmutableList type could not be resolved (probably a missing reference to System.Collections.Immutable).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .MemberAccessExpression(
                    context
                        .SyntaxGenerator
                        .WithTypeArguments(
                            context
                                .SyntaxGenerator
                                .TypeExpression(immutableListType),
                            context
                                .SyntaxGenerator
                                .TypeExpression(typeSymbol.TypeArguments[0])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableDictionaryReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isImmutableDictionary = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Immutable.IImmutableDictionary<TKey, TValue>";

            if (!isImmutableDictionary)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableDictionaryType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary", preferredAssemblyNames: new[] { "System.Collections.Immutable" });

            if (immutableDictionaryType == null)
            {
                context
                    .LogSink
                    .Warn("The ImmutableDictionary type could not be resolved (probably a missing reference to System.Collections.Immutable).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .MemberAccessExpression(
                    context
                        .SyntaxGenerator
                        .WithTypeArguments(
                            context
                                .SyntaxGenerator
                                .TypeExpression(immutableDictionaryType),
                            context
                                .SyntaxGenerator
                                .TypeExpression(typeSymbol.TypeArguments[0]),
                            context
                                .SyntaxGenerator
                                .TypeExpression(typeSymbol.TypeArguments[1])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableQueueReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isImmutableQueue = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Immutable.IImmutableQueue<T>";

            if (!isImmutableQueue)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableQueueType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Immutable.ImmutableQueue", preferredAssemblyNames: new[] { "System.Collections.Immutable" });

            if (immutableQueueType == null)
            {
                context
                    .LogSink
                    .Warn("The ImmutableDictionary type could not be resolved (probably a missing reference to System.Collections.Immutable).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .MemberAccessExpression(
                    context
                        .SyntaxGenerator
                        .WithTypeArguments(
                            context
                                .SyntaxGenerator
                                .TypeExpression(immutableQueueType),
                            context
                                .SyntaxGenerator
                                .TypeExpression(typeSymbol.TypeArguments[0])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableSetReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isImmutableSet = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Immutable.IImmutableSet<T>";

            if (!isImmutableSet)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableHashSetType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet", preferredAssemblyNames: new[] { "System.Collections.Immutable" });

            if (immutableHashSetType == null)
            {
                context
                    .LogSink
                    .Warn("The ImmutableHashSet type could not be resolved (probably a missing reference to System.Collections.Immutable).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .MemberAccessExpression(
                    context
                        .SyntaxGenerator
                        .WithTypeArguments(
                            context
                                .SyntaxGenerator
                                .TypeExpression(immutableHashSetType),
                            context
                                .SyntaxGenerator
                                .TypeExpression(typeSymbol.TypeArguments[0])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableStackReturnType(
            Context context,
            INamedTypeSymbol typeSymbol,
            out SyntaxNode returnValueSyntax)
        {
            var isImmutableStack = typeSymbol.ConstructedFrom?.ToDisplayString() == "System.Collections.Immutable.IImmutableStack<T>";

            if (!isImmutableStack)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableStackType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Collections.Immutable.ImmutableStack", preferredAssemblyNames: new[] { "System.Collections.Immutable" });

            if (immutableStackType == null)
            {
                context
                    .LogSink
                    .Warn("The ImmutableStack type could not be resolved (probably a missing reference to System.Collections.Immutable).");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .MemberAccessExpression(
                    context
                        .SyntaxGenerator
                        .WithTypeArguments(
                            context
                                .SyntaxGenerator
                                .TypeExpression(immutableStackType),
                            context
                                .SyntaxGenerator
                                .TypeExpression(typeSymbol.TypeArguments[0])),
                    "Empty");
            return true;
        }
    }
}