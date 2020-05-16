namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// A plugin that generates appropriate default return values for any member that uses observable-based asynchrony.
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
        public string Name => "Observable-based Asynchrony";

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
                        .WithSource(typeof(ObservableBasedAsynchrony)));

            if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol))
            {
                context
                    .LogSink
                    .Debug("Ignoring type '{0}' because it is not a named type symbol.", typeSymbol);
                return null;
            }

            var isObservable = namedTypeSymbol.ConstructedFrom?.ToDisplayString() == "System.IObservable<T>";

            if (!isObservable)
            {
                context
                    .LogSink
                    .Debug("Type is not IObservable (it is '{0}').", namedTypeSymbol);
                return null;
            }

            var observableType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Reactive.Linq.Observable", preferredAssemblyNames: new[] { "System.Reactive.Linq" });

            if (observableType == null)
            {
                context
                    .LogSink
                    .Debug("The Observable type could not be resolved (probably a missing reference to System.Reactive.Linq).");
                return null;
            }

            var observableInnerType = namedTypeSymbol.TypeArguments[0];
            SyntaxNode observableInvocation;
            var propertySymbol = symbol as IPropertySymbol;

            if (propertySymbol != null)
            {
                // properties are given collection semantics by returning Observable.Empty<T>()
                observableInvocation = context
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
                                            .TypeExpression(observableType),
                                        "Empty"),
                                context
                                    .SyntaxGenerator
                                    .TypeExpression(observableInnerType)));
            }
            else
            {
                // methods are given async operation semantics by returning Observable.Return(...)
                observableInvocation = context
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
                                            .TypeExpression(observableType),
                                        "Return"),
                                context
                                    .SyntaxGenerator
                                    .TypeExpression(observableInnerType)),
                        arguments: new[]
                        {
                            GetDefaultRecursive(context, symbol, observableInnerType)
                        });
            }

            context
                .LogSink
                .Debug("Generated a default value (used type '{0}' from assembly '{1}').", observableType, observableType.ContainingAssembly);

            return observableInvocation;
        }

        private static SyntaxNode GetDefaultRecursive(
            Context context,
            ISymbol symbol,
            ITypeSymbol returnType) =>
                context
                    .Plugins
                    .Select(plugin => plugin.GetDefaultValueSyntax(context, symbol, returnType))
                    .Where(defaultValueSyntax => defaultValueSyntax != null)
                    .FirstOrDefault();
    }
}