namespace PCLMock.CodeGeneration.Plugins
{
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration.Logging;
    using CSharp = Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// A plugin that generates default return values for any member that returns a value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This plugin provides fallback behavior for any member that has not already had a default value generated. It should typically
    /// be included last in the configured list of plugins.
    /// </para>
    /// </remarks>
    internal sealed class Default : IPlugin
    {
        public static readonly Default Instance = new Default();

        private Default()
        {
        }

        public string Name => "Default";

        public Compilation InitializeCompilation(ILogSink logSink, Compilation compilation) =>
            compilation;

        public SyntaxNode GetDefaultValueSyntax(
            Context context,
            ISymbol symbol,
            ITypeSymbol typeSymbol)
        {
            if (typeSymbol.SpecialType == SpecialType.System_Void)
            {
                return null;
            }

            // HACK: TODO: Due to https://github.com/dotnet/roslyn/issues/43945, I am unable to use the below commented code.
            // This is because it will produce `null` instead of `default(T)` for value types in referenced assemblies.
            // Therefore, I generate a rather clunky `(T)default` instead. However, this is not supported by SyntaxGenerator,
            // so I'm using the C#-specific SyntaxFactor to achieve this. 

            //return context.SyntaxGenerator.CastExpression(
            //    typeSymbol,
            //    context.SyntaxGenerator.DefaultExpression(typeSymbol));

            // The exact incantation was not easy to figure out here. I eventually stumbled on the answer via this SO answer:
            // https://stackoverflow.com/a/46262867/5380
            var defaultKeyword = CSharp.SyntaxFactory.IdentifierName(
                CSharp.SyntaxFactory.Identifier(
                    CSharp.SyntaxFactory.TriviaList(),
                    CSharp.SyntaxKind.DefaultKeyword,
                    "default",
                    "default",
                    CSharp.SyntaxFactory.TriviaList()));
            var castExpression = context.SyntaxGenerator.CastExpression(typeSymbol, defaultKeyword);

            return castExpression;
        }
    }
}
