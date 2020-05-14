namespace PCLMock.CodeGeneration
{
    using Microsoft.CodeAnalysis;
    using PCLMock.CodeGeneration.Logging;

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
        /// <param name="logSink">
        /// The current log sink.
        /// </param>
        /// <param name="compilation">
        /// The compilation.
        /// </param>
        /// <returns>
        /// The initialized compilation.
        /// </returns>
        Compilation InitializeCompilation(ILogSink logSink, Compilation compilation);

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
        /// <param name="context">
        /// A context for the operation.
        /// </param>
        /// <param name="symbol">
        /// The symbol that returns a value of type <paramref name="typeSymbol"/>.
        /// </param>
        /// <param name="typeSymbol">
        /// The type for which default value syntax should be generated.
        /// </param>
        /// <returns>
        /// An instance of <see cref="SyntaxNode"/> representing the default value, or <see langword="null"/> if no default value could
        /// be determined.
        /// </returns>
        SyntaxNode GetDefaultValueSyntax(
            Context context,
            ISymbol symbol,
            ITypeSymbol typeSymbol);
    }
}