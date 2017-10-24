namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Reactive.Disposables;
    using System.Reflection;
    using Logging;
    using Microsoft.CodeAnalysis;

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
            compilation
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(IDisposable).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Disposable).GetTypeInfo().Assembly.Location));

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

            var disposableInterfaceType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.IDisposable");

            if (disposableInterfaceType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve IDisposable type.");
                return null;
            }

            if (returnType != disposableInterfaceType)
            {
                context
                    .LogSink
                    .Debug(logSource, "Ignoring symbol '{0}' because its return type is not IDisposable.");
                return null;
            }

            var disposableType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Reactive.Disposables.Disposable");

            if (disposableType == null)
            {
                context
                    .LogSink
                    .Debug(logSource, "Ignoring symbol '{0}' because Disposable type could not be resolved (probably a missing reference to System.Reactive.Core).");
                return null;
            }

            var result = context
                .SyntaxGenerator
                .MemberAccessExpression(
                    context
                        .SyntaxGenerator
                        .TypeExpression(disposableType),
                    context
                        .SyntaxGenerator
                        .IdentifierName("Empty"));

            return result;
        }
    }
}