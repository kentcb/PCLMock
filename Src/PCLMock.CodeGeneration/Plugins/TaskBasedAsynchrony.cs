namespace PCLMock.CodeGeneration.Plugins
{
    using System.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// A plugin that generates appropriate default return values for any member that uses TPL-based asynchrony.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This plugin generates default return specifications for properties and methods that use TPL-based asynchrony.
    /// That is, they return an object of type <see cref="System.Threading.Tasks.Task"/> or
    /// <see cref="System.Threading.Tasks.Task{TResult}"/>. In either case, a specification is generated for the member
    /// such that a task with a default value will be returned rather than returning <see langword="null"/>.
    /// </para>
    /// <para>
    /// For members that return non-generic tasks, the specification will return <c>Task.FromResult(false)</c>. For
    /// members that return generic tasks, the specification will return <c>Task.FromResult(default(T))</c> where
    /// <c>T</c> is the task's result type.
    /// </para>
    /// <para>
    /// Members for which specifications cannot be generated are ignored. This of course includes members that do not use
    /// TPL-based asynchrony, but also set-only properties, generic methods, and any members that return custom
    /// <see cref="System.Threading.Tasks.Task"/> subclasses.
    /// </para>
    /// </remarks>
    public sealed class TaskBasedAsynchrony : IPlugin
    {
        public string Name => "Task-based Asynchrony";

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
                        .WithSource(typeof(TaskBasedAsynchrony)));

            if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol))
            {
                context
                    .LogSink
                    .Debug("Ignoring type '{0}' because it is not a named type symbol.", typeSymbol);
                return null;
            }

            var isTask = namedTypeSymbol.ToDisplayString() == "System.Threading.Tasks.Task";
            var isGenericTask = namedTypeSymbol.ConstructedFrom?.ToDisplayString() == "System.Threading.Tasks.Task<TResult>";

            if (!isTask && !isGenericTask)
            {
                context
                    .LogSink
                    .Debug("Type is not a task (it is '{0}').", namedTypeSymbol);
                return null;
            }

            var taskType = context
                .SemanticModel
                .Compilation
                .GetPreferredTypeByMetadataName("System.Threading.Tasks.Task", preferredAssemblyNames: new[] { "System.Runtime" });

            if (taskType == null)
            {
                context
                    .LogSink
                    .Debug("The Task type could not be resolved (probably a missing reference to System.Runtime).");
                return null;
            }

            ITypeSymbol taskInnerType = context
                .SemanticModel
                .Compilation
                .GetSpecialType(SpecialType.System_Boolean);

            if (isGenericTask)
            {
                taskInnerType = namedTypeSymbol.TypeArguments[0];
            }

            var fromResultInvocation = context
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
                                        .TypeExpression(taskType),
                                    "FromResult"),
                            context
                                .SyntaxGenerator
                                .TypeExpression(taskInnerType)),
                    arguments: new[]
                    {
                        GetDefaultRecursive(context, symbol, taskInnerType)
                    });

            context
                .LogSink
                .Debug("Generated a default value (used type '{0}' from assembly '{1}').", taskType, taskType.ContainingAssembly);

            return fromResultInvocation;
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