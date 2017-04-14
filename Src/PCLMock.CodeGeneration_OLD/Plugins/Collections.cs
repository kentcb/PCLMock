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
        private static readonly Type logSource = typeof(Collections);

        public string Name => "Collections";

        /// <inheritdoc />
        public Compilation InitializeCompilation(Compilation compilation) =>
            compilation;

        /// <inheritdoc />
        public SyntaxNode GetDefaultValueSyntax(
            Context context,
            MockBehavior behavior,
            ISymbol symbol,
            INamedTypeSymbol returnType)
        {
            if (behavior == MockBehavior.Loose)
            {
                return null;
            }

            if (!returnType.IsGenericType)
            {
                context
                    .LogSink
                    .Debug(logSource, "Ignoring symbol '{0}' because its return type is not a generic type, so it cannot be one of the supported collection types.");
                return null;
            }

            SyntaxNode returnValueSyntax;

            if (!TryGetReturnValueSyntaxForEnumerableReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForCollectionReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForDictionaryReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForSetReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableListReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableDictionaryReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableQueueReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableSetReturnType(context, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableStackReturnType(context, returnType, out returnValueSyntax))
            {
                context
                    .LogSink
                    .Debug(logSource, "Ignoring symbol '{0}' because it does not return a supported collection type.", symbol);
                return null;
            }

            return returnValueSyntax;
        }

        private static bool TryGetReturnValueSyntaxForEnumerableReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var enumerableInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");

            if (returnType.ConstructedFrom != enumerableInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var enumerableType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Linq.Enumerable");

            if (enumerableType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve Enumerable class.");
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
                                    .TypeExpression(returnType.TypeArguments[0])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForCollectionReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var collectionInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.ICollection`1");

            var readOnlyCollectionInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");

            var listInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IList`1");

            var readOnlyListInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");

            if (returnType.ConstructedFrom != collectionInterfaceType &&
                returnType.ConstructedFrom != readOnlyCollectionInterfaceType &&
                returnType.ConstructedFrom != listInterfaceType &&
                returnType.ConstructedFrom != readOnlyListInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var listType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.List`1");

            if (listType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve List<T> class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                    .ObjectCreationExpression(
                        listType.Construct(returnType.TypeArguments[0])).NormalizeWhitespace();
            return true;
        }

        private static bool TryGetReturnValueSyntaxForDictionaryReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var dictionaryInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

            var readOnlyDictionaryInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");

            if (returnType.ConstructedFrom != dictionaryInterfaceType &&
                returnType.ConstructedFrom != readOnlyDictionaryInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var dictionaryType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.Dictionary`2");

            if (dictionaryType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve Dictionary<TKey, TValue> class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .ObjectCreationExpression(
                    dictionaryType.Construct(returnType.TypeArguments[0], returnType.TypeArguments[1])).NormalizeWhitespace();
            return true;
        }

        private static bool TryGetReturnValueSyntaxForSetReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var setInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.ISet`1");

            if (returnType.ConstructedFrom != setInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var dictionaryType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.HashSet`1");

            if (dictionaryType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve HashSet<T> class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = context
                .SyntaxGenerator
                .ObjectCreationExpression(
                    dictionaryType.Construct(returnType.TypeArguments[0])).NormalizeWhitespace();
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableListReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableListInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.IImmutableList`1");

            if (returnType.ConstructedFrom != immutableListInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableListType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableList");

            if (immutableListType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve ImmutableList class.");
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
                                .TypeExpression(returnType.TypeArguments[0])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableDictionaryReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableDictionaryInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.IImmutableDictionary`2");

            if (returnType.ConstructedFrom != immutableDictionaryInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableDictionaryType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary");

            if (immutableDictionaryType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve ImmutableDictionary class.");
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
                                .TypeExpression(returnType.TypeArguments[0]),
                            context
                                .SyntaxGenerator
                                .TypeExpression(returnType.TypeArguments[1])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableQueueReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableQueueInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.IImmutableQueue`1");

            if (returnType.ConstructedFrom != immutableQueueInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableQueueType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableQueue");

            if (immutableQueueType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve ImmutableQueue class.");
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
                                .TypeExpression(returnType.TypeArguments[0])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableSetReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableSetInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.IImmutableSet`1");

            if (returnType.ConstructedFrom != immutableSetInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableSetType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet");

            if (immutableSetType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve ImmutableSet class.");
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
                                .TypeExpression(immutableSetType),
                            context
                                .SyntaxGenerator
                                .TypeExpression(returnType.TypeArguments[0])),
                    "Empty");
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableStackReturnType(
            Context context,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableStackInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.IImmutableStack`1");

            if (returnType.ConstructedFrom != immutableStackInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableStackType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableStack");

            if (immutableStackType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve ImmutableStack class.");
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
                                .TypeExpression(returnType.TypeArguments[0])),
                    "Empty");
            return true;
        }
    }
}