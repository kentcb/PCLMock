namespace PCLMock.CodeGeneration
{
    using Logging;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;

    /// <summary>
    /// Defines the interface for a plugin.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plugins are able to participate in generating code at specific points in the mock code generation process.
    /// For example, they might generate a default return value for any method returning a <c>Task</c>.
    /// </para>
    /// <para>
    /// The participating plugins are configured by adding them to the <see cref="Models.Configuration"/>
    /// instance passed into <see cref="Generator"/>.
    /// </para>
    /// </remarks>
    public interface IPlugin
    {
        /// <summary>
        /// Gets a human-friendly name for the plugin, for identification in debug output.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Perform any initialization against the compilation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called once per compilation and per plugin, before any code is generated. If no initialization is required,
        /// plugins should simply return the provided compilation.
        /// </para>
        /// </remarks>
        /// <param name="compilation">
        /// The compilation.
        /// </param>
        /// <returns>
        /// The initialized compilation.
        /// </returns>
        Compilation InitializeCompilation(Compilation compilation);

        /// <summary>
        /// Called to generate the default value for a given symbol.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The code generation engine calls this method for each discovered symbol (property/method) that returns a value.
        /// The first plugin to return a value will have that syntax incorporated into the mock's <c>ConfigureBehaviorGenerated</c>
        /// or <c>ConfigureLooseBehaviorGenerated</c> method, depending on the value of <param name="behavior"/>.
        /// </para>
        /// </remarks>
        /// <param name="behavior">
        /// Indicates whether the default value is being generated for strict or loose behavioral semantics.
        /// </param>
        /// <param name="syntaxGenerator">
        /// The syntax generator.
        /// </param>
        /// <param name="logSink">
        /// A log sink.
        /// </param>
        /// <param name="semanticModel">
        /// The semantic model.
        /// </param>
        /// <param name="symbol">
        /// The symbol.
        /// </param>
        /// <param name="returnType">
        /// The symbol's return type.
        /// </param>
        /// <returns>
        /// An instance of <see cref="SyntaxNode"/> containing the default value, or <see langword="null"/> if no default value is
        /// relevant.
        /// </returns>
        SyntaxNode GetDefaultValueSyntax(
            ILogSink logSink,
            MockBehavior behavior,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol,
            INamedTypeSymbol returnType);
    }
}