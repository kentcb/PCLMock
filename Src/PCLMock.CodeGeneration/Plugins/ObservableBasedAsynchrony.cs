namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using Logging;
    using Microsoft.CodeAnalysis;

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
        public Compilation InitializeCompilation(Compilation compilation) =>
            compilation
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(IObservable<>).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Observable).GetTypeInfo().Assembly.Location));

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

            var observableInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.IObservable`1");

            var observableType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Reactive.Linq.Observable");

            if (observableInterfaceType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve System.IObservable<T>.");
                return null;
            }

            if (observableType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve System.Reactive.Linq.Observable.");
                return null;
            }

            var isObservable = returnType.IsGenericType && returnType.ConstructedFrom == observableInterfaceType;

            if (!isObservable)
            {
                context
                    .LogSink
                    .Debug(logSource, "Ignoring symbol '{0}' because it does not return IObservable<T>.", symbol);
                return null;
            }

            var observableInnerType = returnType.TypeArguments[0];
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
                            GetDefaultRecursive(context, behavior, symbol, observableInnerType)
                        });
            }

            return observableInvocation;
        }

        private static SyntaxNode GetDefaultRecursive(
            Context context,
            MockBehavior behavior,
            ISymbol symbol,
            ITypeSymbol returnType)
        {
            var namedTypeSymbol = returnType as INamedTypeSymbol;

            if (namedTypeSymbol != null)
            {
                var recursiveDefault = context
                    .Plugins
                    .Select(plugin => plugin.GetDefaultValueSyntax(context, behavior, symbol, namedTypeSymbol))
                    .Where(defaultValueSyntax => defaultValueSyntax != null)
                    .FirstOrDefault();

                if (recursiveDefault != null)
                {
                    return recursiveDefault;
                }
            }

            // recursive resolution not possible, so fallback to default(T)
            return context.SyntaxGenerator.DefaultExpression(returnType);
        }
    }
}