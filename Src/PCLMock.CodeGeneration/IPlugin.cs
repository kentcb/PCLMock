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
        /// Called to generate any behavior that is always applicable to an instance of the mock.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The code generation engine calls this method for each discovered symbol (property/method). Any returned syntax
        /// will be incorporated into the mock's <c>ConfigureBehaviorGenerated</c> method. This method is always executed
        /// by the mock's constructor regardless of whether the mock is loose or strict.
        /// </para>
        /// </remarks>
        /// <param name="logSink">
        /// A log sink.
        /// </param>
        /// <param name="syntaxGenerator">
        /// The syntax generator.
        /// </param>
        /// <param name="semanticModel">
        /// The semantic model.
        /// </param>
        /// <param name="symbol">
        /// The symbol.
        /// </param>
        /// <returns>
        /// An instance of <see cref="SyntaxNode"/> containing the desired code, or <see langword="null"/> if no code should
        /// be generated for the symbol.
        /// </returns>
        SyntaxNode GenerateConfigureBehavior(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol);

        /// <summary>
        /// Called to generate any behavior that is applicable to an instance of the mock when it is loose.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The code generation engine calls this method for each discovered symbol (property/method). Any returned syntax
        /// will be incorporated into the mock's <c>ConfigureLooseBehaviorGenerated</c> method. This method is executed
        /// by the mock's constructor if the mock is loose.
        /// </para>
        /// </remarks>
        /// <param name="logSink">
        /// A log sink.
        /// </param>
        /// <param name="syntaxGenerator">
        /// The syntax generator.
        /// </param>
        /// <param name="semanticModel">
        /// The semantic model.
        /// </param>
        /// <param name="symbol">
        /// The symbol.
        /// </param>
        /// <returns>
        /// An instance of <see cref="SyntaxNode"/> containing the desired code, or <see langword="null"/> if no code should
        /// be generated for the symbol.
        /// </returns>
        SyntaxNode GenerateConfigureLooseBehavior(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol);
    }
}