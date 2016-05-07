namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;

    /// <summary>
    /// A plugin that generates appropriate default return values for any member that uses observable-based
    /// asynchrony.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This plugin generates default return specifications for properties and methods that use observable-based asynchrony.
    /// That is, they return an object of type <see cref="IObservable{T}"/>. For such members, a specification is generated
    /// such that an actual observable will be returned rather than returning <see langword="null"/>.
    /// </para>
    /// <para>
    /// The observable returned depends on the member type. For properties, an empty observable is returned (i.e.
    /// <c>Observable.Empty&lt;T&gt;</c>. For methods, an observable with a single default item is returned (i.e.
    /// <c>Observable.Return&lt;T&gt;(default(T))</c>.
    /// </para>
    /// <para>
    /// The idea of these defaults is to best-guess the semantics of the observable. Typically, observables returned from
    /// methods represent asynchronous operations, so the returned value represents the result of that operation. Observables
    /// returned by properties, on the other hand, will typically have collection semantics. That is, they represent zero or
    /// more asynchronously-received items.
    /// </para>
    /// <para>
    /// Members for which specifications cannot be generated are ignored. This of course includes members that do not use
    /// observable-based asynchrony, but also set-only properties, generic methods, and any members that return custom
    /// <see cref="IObservable{T}"/> subtypes.
    /// </para>
    /// </remarks>
    public sealed class ObservableBasedAsynchrony : IPlugin
    {
        private static readonly Type logSource = typeof(ObservableBasedAsynchrony);

        public string Name => "Observable-based Asynchrony";

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
                logSink.Warn(logSource, "Ignoring symbol '{0}' because its return type could not be determined (it's probably generic).", symbol);
                return null;
            }

            var observableInterfaceType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.IObservable`1");

            var observableType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Reactive.Linq.Observable");

            if (observableInterfaceType == null || observableType == null)
            {
                logSink.Warn(logSource, "Failed to resolve IObservable<T> interface or Observable class.");
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

            var isObservable = returnType.IsGenericType && returnType.ConstructedFrom == observableInterfaceType;

            if (!isObservable)
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because it does not return IObservable<T>.", symbol);
                return null;
            }

            var observableInnerType = returnType.TypeArguments[0];
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
                    //         .Return(Observable.Empty<T>());
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
                    //         .Return(Observable.Empty<T>());
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
                //         .Return(Observable.Return<T>(default(T)));
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

            SyntaxNode observableInvocation;

            if (propertySymbol != null)
            {
                // properties are given collection semantics by returning Observable.Empty<T>()
                observableInvocation = syntaxGenerator.InvocationExpression(
                    syntaxGenerator.WithTypeArguments(
                        syntaxGenerator.MemberAccessExpression(
                            syntaxGenerator.TypeExpression(observableType),
                            "Empty"),
                        syntaxGenerator.TypeExpression(observableInnerType)));
            }
            else
            {
                // methods are given async operation semantics by returning Observable.Return(default(T))
                observableInvocation = syntaxGenerator.InvocationExpression(
                    syntaxGenerator.WithTypeArguments(
                        syntaxGenerator.MemberAccessExpression(
                            syntaxGenerator.TypeExpression(observableType),
                            "Return"),
                        syntaxGenerator.TypeExpression(observableInnerType)),
                    arguments: new[]
                    {
                        syntaxGenerator.DefaultExpression(observableInnerType)
                    });
            }

            var result = syntaxGenerator.ExpressionStatement(
                syntaxGenerator.InvocationExpression(
                    syntaxGenerator.MemberAccessExpression(
                        whenInvocation,
                        syntaxGenerator.IdentifierName("Return")),
                    arguments: new[]
                    {
                        observableInvocation
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