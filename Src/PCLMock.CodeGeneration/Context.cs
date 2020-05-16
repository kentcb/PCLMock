namespace PCLMock.CodeGeneration
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;
    using PCLMock.CodeGeneration.Logging;

    /// <summary>
    /// Contains contextual information for code generation.
    /// </summary>
    public sealed class Context
    {
        private readonly ILogSink logSink;
        private readonly Language language;
        private readonly IImmutableList<IPlugin> plugins;
        private readonly SyntaxGenerator syntaxGenerator;
        private readonly SemanticModel semanticModel;

        /// <summary>
        /// Creates a new instance of the <c>Context</c> class.
        /// </summary>
        /// <param name="logSink">
        /// The log sink.
        /// </param>
        /// <param name="language">
        /// The language in which code is being generated.
        /// </param>
        /// <param name="plugins">
        /// A list of all plugins.
        /// </param>
        /// <param name="syntaxGenerator">
        /// The syntax generator.
        /// </param>
        /// <param name="semanticModel">
        /// The semantic model.
        /// </param>
        public Context(
            ILogSink logSink,
            Language language,
            IImmutableList<IPlugin> plugins,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel)
        {
            this.logSink = logSink;
            this.language = language;
            this.plugins = plugins;
            this.syntaxGenerator = syntaxGenerator;
            this.semanticModel = semanticModel;
        }

        /// <summary>
        /// Gets the log sink.
        /// </summary>
        public ILogSink LogSink => this.logSink;

        /// <summary>
        /// Gets the language.
        /// </summary>
        public Language Language => this.language;

        /// <summary>
        /// Gets a list of all plugins.
        /// </summary>
        public IImmutableList<IPlugin> Plugins => this.plugins;

        /// <summary>
        /// Gets the syntax generator.
        /// </summary>
        public SyntaxGenerator SyntaxGenerator => this.syntaxGenerator;

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        public SemanticModel SemanticModel => this.semanticModel;

        /// <summary>
        /// Replaces this context's log sink with <paramref name="logSink"/>.
        /// </summary>
        /// <param name="logSink">
        /// The new log sink.
        /// </param>
        /// <returns>
        /// The new context.
        /// </returns>
        public Context WithLogSink(ILogSink logSink) =>
            new Context(
                logSink,
                this.language,
                this.plugins,
                this.syntaxGenerator,
                this.semanticModel);
    }
}