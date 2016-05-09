namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

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
        public SyntaxNode GenerateConfigureBehavior(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol)
        {
            logSink.Debug(logSource, "Considering symbol '{0}'.", symbol);

            var propertySymbol = symbol as IPropertySymbol;
            var methodSymbol = symbol as IMethodSymbol;

            INamedTypeSymbol returnType = null;

            if (propertySymbol != null)
            {
                if (propertySymbol.GetMethod == null)
                {
                    logSink.Debug(logSource, "Ignoring symbol '{0}' because it is a write-only property.", symbol);
                    return null;
                }

                returnType = propertySymbol.GetMethod.ReturnType as INamedTypeSymbol;
            }
            else if (methodSymbol != null)
            {
                if (methodSymbol.AssociatedSymbol != null)
                {
                    logSink.Debug(logSource, "Ignoring symbol '{0}' because it is a method with an associated symbol.", symbol);
                    return null;
                }

                if (methodSymbol.IsGenericMethod)
                {
                    logSink.Debug(logSource, "Ignoring symbol '{0}' because it is a generic method.", symbol);
                    return null;
                }

                returnType = methodSymbol.ReturnType as INamedTypeSymbol;
            }
            else
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because it is neither a property nor a method.", symbol);
                return null;
            }

            if (returnType == null)
            {
                logSink.Warn(logSource, "Ignoring symbol '{0}' because its return type could not be determined (it's probably a generic).", symbol);
                return null;
            }

            if (!returnType.IsGenericType)
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because its return type is not a generic type, so it cannot be one of the supported collection types.");
                return null;
            }

            SyntaxNode returnValueSyntax;

            if (!TryGetReturnValueSyntaxForEnumerableReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForCollectionReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForDictionaryReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForSetReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableListReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableDictionaryReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableQueueReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableSetReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax) &&
                !TryGetReturnValueSyntaxForImmutableStackReturnType(logSink, syntaxGenerator, semanticModel, returnType, out returnValueSyntax))
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because it does not return a supported collection type.", symbol);
                return null;
            }

            var itType = semanticModel
                .Compilation
                .GetTypeByMetadataName("PCLMock.It");

            if (itType == null)
            {
                logSink.Error(logSource, "Failed to resolve It class.");
                return null;
            }

            var isAnyMethod = itType
                .GetMembers("IsAny")
                .Single();

            if (isAnyMethod == null)
            {
                logSink.Error(logSource, "Failed to resolve IsAny method.");
                return null;
            }

            var lambdaParameterName = symbol.GetUniqueName();

            SyntaxNode lambdaExpression;

            if (propertySymbol != null)
            {
                if (!propertySymbol.IsIndexer)
                {
                    // GENERATED CODE:
                    //
                    //     this
                    //         .When(x => x.SymbolName)
                    //         .Return(returnValueSyntax);
                    lambdaExpression = syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.IdentifierName(lambdaParameterName),
                        propertySymbol.Name);
                }
                else
                {
                    // GENERATED CODE:
                    //
                    //     this
                    //         .When(x => x[It.IsAny<P1>(), It.IsAny<P2>() ...)
                    //         .Return(returnValueSyntax);
                    var whenArguments = propertySymbol
                        .Parameters
                        .Select(
                            parameter =>
                                syntaxGenerator.InvocationExpression(
                                    syntaxGenerator.MemberAccessExpression(
                                        syntaxGenerator.TypeExpression(itType),
                                        syntaxGenerator.GenericName(
                                            "IsAny",
                                            typeArguments: new[]
                                            {
                                                parameter.Type
                                            }))));

                    lambdaExpression = syntaxGenerator.ElementAccessExpression(
                        syntaxGenerator.IdentifierName(lambdaParameterName),
                        arguments: whenArguments);
                }
            }
            else
            {
                // GENERATED CODE:
                //
                //     this
                //         .When(x => x.SymbolName(It.IsAny<P1>(), It.IsAny<P2>() ...)
                //         .Return(returnValueSyntax);
                var whenArguments = methodSymbol
                    .Parameters
                    .Select(
                        parameter =>
                            syntaxGenerator.InvocationExpression(
                                syntaxGenerator.MemberAccessExpression(
                                    syntaxGenerator.TypeExpression(itType),
                                    syntaxGenerator.GenericName(
                                        "IsAny",
                                        typeArguments: new[]
                                        {
                                            parameter.Type
                                        }))));

                lambdaExpression = syntaxGenerator.InvocationExpression(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.IdentifierName(lambdaParameterName),
                        methodSymbol.Name),
                    arguments: whenArguments);
            }

            var whenLambdaArgument = syntaxGenerator.ValueReturningLambdaExpression(
                lambdaParameterName,
                lambdaExpression);

            var whenInvocation = syntaxGenerator.InvocationExpression(
                syntaxGenerator.MemberAccessExpression(
                    syntaxGenerator.ThisExpression(),
                    syntaxGenerator.IdentifierName("When")),
                whenLambdaArgument);

            var result = syntaxGenerator.ExpressionStatement(
                syntaxGenerator.InvocationExpression(
                    syntaxGenerator.MemberAccessExpression(
                        whenInvocation,
                        syntaxGenerator.IdentifierName("Return")),
                    arguments: new[]
                    {
                        returnValueSyntax
                    }));

            return result;
        }

        /// <inheritdoc />
        public SyntaxNode GenerateConfigureLooseBehavior(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol)
        {
            return null;
        }

        private static bool TryGetReturnValueSyntaxForEnumerableReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var enumerableInterfaceType = semanticModel
               .Compilation
               .GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");

            if (returnType.ConstructedFrom != enumerableInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var enumerableType = semanticModel
                    .Compilation
                    .GetTypeByMetadataName("System.Linq.Enumerable");

            if (enumerableType == null)
            {
                logSink.Warn(logSource, "Failed to resolve Enumerable class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(enumerableType),
                        "Empty"),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[0])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForCollectionReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var collectionInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.ICollection`1");

            var readOnlyCollectionInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");

            var listInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IList`1");

            var readOnlyListInterfaceType = semanticModel
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

            var listType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.List`1");

            if (listType == null)
            {
                logSink.Warn(logSource, "Failed to resolve List<T> class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.ObjectCreationExpression(
                    listType.Construct(returnType.TypeArguments[0])).WithAdditionalAnnotations(Formatter.Annotation);

            // TODO: I can't believe this is necessary (along with the WithAdditionalAnnotations call above) just to
            // get a space between 'new' and the type name, but see https://github.com/dotnet/roslyn/issues/3960
            returnValueSyntax = Formatter.Format(returnValueSyntax, Formatter.Annotation, new AdhocWorkspace());
            return true;
        }

        private static bool TryGetReturnValueSyntaxForDictionaryReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var dictionaryInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

            var readOnlyDictionaryInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");

            if (returnType.ConstructedFrom != dictionaryInterfaceType &&
                returnType.ConstructedFrom != readOnlyDictionaryInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var dictionaryType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.Dictionary`2");

            if (dictionaryType == null)
            {
                logSink.Warn(logSource, "Failed to resolve Dictionary<TKey, TValue> class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.ObjectCreationExpression(
                    dictionaryType.Construct(returnType.TypeArguments[0], returnType.TypeArguments[1])).WithAdditionalAnnotations(Formatter.Annotation);

            // TODO: I can't believe this is necessary (along with the WithAdditionalAnnotations call above) just to
            // get a space between 'new' and the type name, but see https://github.com/dotnet/roslyn/issues/3960
            returnValueSyntax = Formatter.Format(returnValueSyntax, Formatter.Annotation, new AdhocWorkspace());
            return true;
        }

        private static bool TryGetReturnValueSyntaxForSetReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var setInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.ISet`1");

            if (returnType.ConstructedFrom != setInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var dictionaryType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Generic.HashSet`1");

            if (dictionaryType == null)
            {
                logSink.Warn(logSource, "Failed to resolve HashSet<T> class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.ObjectCreationExpression(
                    dictionaryType.Construct(returnType.TypeArguments[0])).WithAdditionalAnnotations(Formatter.Annotation);

            // TODO: I can't believe this is necessary (along with the WithAdditionalAnnotations call above) just to
            // get a space between 'new' and the type name, but see https://github.com/dotnet/roslyn/issues/3960
            returnValueSyntax = Formatter.Format(returnValueSyntax, Formatter.Annotation, new AdhocWorkspace());
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableListReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableListInterfaceType = semanticModel
               .Compilation
               .GetTypeByMetadataName("System.Collections.Immutable.IImmutableList`1");

            if (returnType.ConstructedFrom != immutableListInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableListType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableList");

            if (immutableListType == null)
            {
                logSink.Warn(logSource, "Failed to resolve ImmutableList class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(immutableListType),
                        "Empty"),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[0])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableDictionaryReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableDictionaryInterfaceType = semanticModel
               .Compilation
               .GetTypeByMetadataName("System.Collections.Immutable.IImmutableDictionary`2");

            if (returnType.ConstructedFrom != immutableDictionaryInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableDictionaryType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary");

            if (immutableDictionaryType == null)
            {
                logSink.Warn(logSource, "Failed to resolve ImmutableDictionary class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(immutableDictionaryType),
                        "Create"),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[0]),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[1])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableQueueReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableQueueInterfaceType = semanticModel
               .Compilation
               .GetTypeByMetadataName("System.Collections.Immutable.IImmutableQueue`1");

            if (returnType.ConstructedFrom != immutableQueueInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableQueueType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableQueue");

            if (immutableQueueType == null)
            {
                logSink.Warn(logSource, "Failed to resolve ImmutableQueue class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(immutableQueueType),
                        "Empty"),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[0])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableSetReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableSetInterfaceType = semanticModel
               .Compilation
               .GetTypeByMetadataName("System.Collections.Immutable.IImmutableSet`1");

            if (returnType.ConstructedFrom != immutableSetInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableSetType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet");

            if (immutableSetType == null)
            {
                logSink.Warn(logSource, "Failed to resolve ImmutableSet class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(immutableSetType),
                        "Empty"),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[0])));
            return true;
        }

        private static bool TryGetReturnValueSyntaxForImmutableStackReturnType(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            INamedTypeSymbol returnType,
            out SyntaxNode returnValueSyntax)
        {
            var immutableStackInterfaceType = semanticModel
               .Compilation
               .GetTypeByMetadataName("System.Collections.Immutable.IImmutableStack`1");

            if (returnType.ConstructedFrom != immutableStackInterfaceType)
            {
                returnValueSyntax = null;
                return false;
            }

            var immutableStackType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Collections.Immutable.ImmutableStack");

            if (immutableStackType == null)
            {
                logSink.Warn(logSource, "Failed to resolve ImmutableStack class.");
                returnValueSyntax = null;
                return false;
            }

            returnValueSyntax = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(immutableStackType),
                        "Empty"),
                    syntaxGenerator.TypeExpression(returnType.TypeArguments[0])));
            return true;
        }
    }
}