namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using Logging;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;

    /// <summary>
    /// A plugin that generates appropriate default return values for any member that returns <see cref="IDisposable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This plugin generates return specifications for any member returning <see cref="IDisposable"/>. The returned
    /// value is an instance of <c>System.Reactive.Disposables.Disposable.Empty</c>. Since the <c>Disposable</c> type
    /// is defined by Reactive Extensions, the target code must have a reference in order for the specification to be
    /// generated.
    /// </para>
    /// <para>
    /// Members for which specifications cannot be generated are ignored. This of course includes members that do not
    /// return <see cref="IDisposable"/>, but also set-only properties, generic methods, and any members that return
    /// custom disposable subtypes.
    /// </para>
    /// </remarks>
    public sealed class Disposables : IPlugin
    {
        private static readonly Type logSource = typeof(Disposables);

        public string Name => "Disposables";

        /// <inheritdoc />
        public Compilation InitializeCompilation(Compilation compilation) =>
            compilation.AddReferences(MetadataReference.CreateFromFile(typeof(Disposable).Assembly.Location));

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

            var disposableInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.IDisposable");

            if (disposableInterfaceType == null)
            {
                logSink.Warn(logSource, "Failed to resolve IDisposable type.");
                return null;
            }

            if (returnType != disposableInterfaceType)
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because its return type is not IDisposable.");
                return null;
            }

            var disposableType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Reactive.Disposables.Disposable");

            if (disposableType == null)
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because Disposable type could not be resolved (probably a missing reference to System.Reactive.Core).");
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
                    //         .Return(Disposable.Empty);
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
                    //         .Return(Disposable.Empty);
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
                //         .Return(Disposable.Empty);
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
                        syntaxGenerator.MemberAccessExpression(
                            syntaxGenerator.TypeExpression(disposableType),
                            syntaxGenerator.IdentifierName("Empty"))
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
    }
}