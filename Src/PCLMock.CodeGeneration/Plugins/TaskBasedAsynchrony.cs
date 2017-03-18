namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using Logging;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// A plugin that generates appropriate default return values for any member that uses TPL-based
    /// asynchrony.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This plugin generates default return specifications for properties and methods that use TPL-based asynchrony.
    /// SThat is, they return an object of type <see cref="System.Threading.Tasks.Task"/> or
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
        private static readonly Type logSource = typeof(TaskBasedAsynchrony);

        public string Name => "Task-based Asynchrony";

        /// <inheritdoc />
        public Compilation InitializeCompilation(Compilation compilation) =>
            compilation;

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

            var taskBaseType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Threading.Tasks.Task");

            var genericTaskBaseType = context
                .SemanticModel
                .Compilation
                .GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            if (taskBaseType == null || genericTaskBaseType == null)
            {
                context
                    .LogSink
                    .Warn(logSource, "Failed to resolve Task classes.");
                return null;
            }

            var isGenericTask = returnType.IsGenericType && returnType.ConstructedFrom == genericTaskBaseType;
            var isTask = returnType == taskBaseType;

            if (!isTask && !isGenericTask)
            {
                context
                    .LogSink
                    .Debug(logSource, "Ignoring symbol '{0}' because it does not return a Task or Task<T>.", symbol);
                return null;
            }

            ITypeSymbol taskType = context
                .SemanticModel
                .Compilation
                .GetSpecialType(SpecialType.System_Boolean);

            if (isGenericTask)
            {
                taskType = returnType.TypeArguments[0];
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
                                        .TypeExpression(taskBaseType),
                                    "FromResult"),
                            context
                                .SyntaxGenerator
                                .TypeExpression(taskType)),
                    arguments: new[]
                    {
                        context
                            .SyntaxGenerator
                            .DefaultExpression(taskType)
                    });

            return fromResultInvocation;
        }
    }
}