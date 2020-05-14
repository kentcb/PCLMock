namespace PCLMock.CodeGeneration.Plugins
{
    using System;
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
        public string Name => "Disposables";

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
                        .WithSource(typeof(Disposables)));

            var isDisposable = typeSymbol.ToDisplayString() == "System.IDisposable";

            if (!isDisposable)
            {
                context
                    .LogSink
                    .Debug("Type is not IDisposable (it is '{0}').", typeSymbol);
                return null;
            }

            var disposableType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Reactive.Disposables.Disposable", preferredAssemblyNames: new[] { "System.Reactive.Core" });

            if (disposableType == null)
            {
                context
                    .LogSink
                    .Warn("The Disposable type could not be resolved (probably a missing reference to System.Reactive.Core).");
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

            context
                .LogSink
                .Debug("Generated a default value (used type '{0}' from assembly '{1}').", disposableType, disposableType.ContainingAssembly);

            return result;
        }
    }
}