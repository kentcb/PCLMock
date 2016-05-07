namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Linq;
    using Logging;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;

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
        public SyntaxNode GenerateConfigureBehavior(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol)
        {
            logSink.Debug(logSource, "Considering symbol '{0}'.", symbol);

            var propertySymbol = symbol as IPropertySymbol;
            var methodSymbol = symbol as IMethodSymbol;

            INamedTypeSymbol returnType = null;

            if (propertySymbol != null)
            {
                if (propertySymbol.GetMethod == null)
                {
                    logSink.Debug(logSource, "Ignoring symbol '{0}' because it is a write-only property.", symbol);
                    return null;
                }

                returnType = propertySymbol.GetMethod.ReturnType as INamedTypeSymbol;
            }
            else if (methodSymbol != null)
            {
                if (methodSymbol.AssociatedSymbol != null)
                {
                    logSink.Debug(logSource, "Ignoring symbol '{0}' because it is a method with an associated symbol.", symbol);
                    return null;
                }

                if (methodSymbol.IsGenericMethod)
                {
                    logSink.Debug(logSource, "Ignoring symbol '{0}' because it is a generic method.", symbol);
                    return null;
                }

                returnType = methodSymbol.ReturnType as INamedTypeSymbol;
            }
            else
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because it is neither a property nor a method.", symbol);
                return null;
            }

            if (returnType == null)
            {
                logSink.Warn(logSource, "Ignoring symbol '{0}' because its return type could not be determined (it's probably generic).", symbol);
                return null;
            }

            var taskBaseType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Threading.Tasks.Task");

            var genericTaskBaseType = semanticModel
                .Compilation
                .GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            if (taskBaseType == null || genericTaskBaseType == null)
            {
                logSink.Warn(logSource, "Failed to resolve Task classes.");
                return null;
            }

            var itType = semanticModel
                .Compilation
                .GetTypeByMetadataName("PCLMock.It");

            if (itType == null)
            {
                logSink.Error(logSource, "Failed to resolve It class.");
                return null;
            }

            var isAnyMethod = itType
                .GetMembers("IsAny")
                .Single();

            if (isAnyMethod == null)
            {
                logSink.Error(logSource, "Failed to resolve IsAny method.");
                return null;
            }

            var isGenericTask = returnType.IsGenericType && returnType.ConstructedFrom == genericTaskBaseType;
            var isTask = returnType == taskBaseType;

            if (!isTask && !isGenericTask)
            {
                logSink.Debug(logSource, "Ignoring symbol '{0}' because it does not return a Task or Task<T>.", symbol);
                return null;
            }

            ITypeSymbol taskType = semanticModel
                .Compilation
                .GetSpecialType(SpecialType.System_Boolean);

            if (isGenericTask)
            {
                taskType = returnType.TypeArguments[0];
            }

            var lambdaParameterName = symbol.GetUniqueName();

            SyntaxNode lambdaExpression;

            if (propertySymbol != null)
            {
                if (!propertySymbol.IsIndexer)
                {
                    // GENERATED CODE:
                    //
                    //     this
                    //         .When(x => x.SymbolName)
                    //         .Return(Task.FromResult(default(T)));
                    lambdaExpression = syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.IdentifierName(lambdaParameterName),
                        propertySymbol.Name);
                }
                else
                {
                    // GENERATED CODE:
                    //
                    //     this
                    //         .When(x => x[It.IsAny<P1>(), It.IsAny<P2>() ...)
                    //         .Return(Task.FromResult(default(T)));
                    var whenArguments = propertySymbol
                        .Parameters
                        .Select(
                            parameter =>
                                syntaxGenerator.InvocationExpression(
                                    syntaxGenerator.MemberAccessExpression(
                                        syntaxGenerator.TypeExpression(itType),
                                        syntaxGenerator.GenericName(
                                            "IsAny",
                                            typeArguments: new[]
                                            {
                                                parameter.Type
                                            }))));

                    lambdaExpression = syntaxGenerator.ElementAccessExpression(
                        syntaxGenerator.IdentifierName(lambdaParameterName),
                        arguments: whenArguments);
                }
            }
            else
            {
                // GENERATED CODE:
                //
                //     this
                //         .When(x => x.SymbolName(It.IsAny<P1>(), It.IsAny<P2>() ...)
                //         .Return(Task.FromResult(default(T)));
                var whenArguments = methodSymbol
                    .Parameters
                    .Select(
                        parameter =>
                            syntaxGenerator.InvocationExpression(
                                syntaxGenerator.MemberAccessExpression(
                                    syntaxGenerator.TypeExpression(itType),
                                    syntaxGenerator.GenericName(
                                        "IsAny",
                                        typeArguments: new[]
                                        {
                                            parameter.Type
                                        }))));

                lambdaExpression = syntaxGenerator.InvocationExpression(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.IdentifierName(lambdaParameterName),
                        methodSymbol.Name),
                    arguments: whenArguments);
            }

            var whenLambdaArgument = syntaxGenerator.ValueReturningLambdaExpression(
                lambdaParameterName,
                lambdaExpression);

            var whenInvocation = syntaxGenerator.InvocationExpression(
                syntaxGenerator.MemberAccessExpression(
                    syntaxGenerator.ThisExpression(),
                    syntaxGenerator.IdentifierName("When")),
                whenLambdaArgument);

            var fromResultInvocation = syntaxGenerator.InvocationExpression(
                syntaxGenerator.WithTypeArguments(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.TypeExpression(taskBaseType),
                        "FromResult"),
                    syntaxGenerator.TypeExpression(taskType)),
                arguments: new[]
                {
                    syntaxGenerator.DefaultExpression(taskType)
                });

            var result = syntaxGenerator.ExpressionStatement(
                syntaxGenerator.InvocationExpression(
                    syntaxGenerator.MemberAccessExpression(
                        whenInvocation,
                        syntaxGenerator.IdentifierName("Return")),
                    arguments: new[]
                    {
                        fromResultInvocation
                    }));

            return result;
        }

        /// <inheritdoc />
        public SyntaxNode GenerateConfigureLooseBehavior(
            ILogSink logSink,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol)
        {
            return null;
        }
    }
}