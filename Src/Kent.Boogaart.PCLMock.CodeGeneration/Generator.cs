namespace Kent.Boogaart.PCLMock.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.MSBuild;

    public static class Generator
    {
        public async static Task<IImmutableList<SyntaxNode>> GenerateMocksAsync(
            string solutionPath,
            Func<INamedTypeSymbol, bool> interfacePredicate,
            Func<INamedTypeSymbol, string> mockNamespaceSelector,
            Func<INamedTypeSymbol, string> mockNameSelector,
            Language language)
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionPath);
            var compilations = await Task.WhenAll(
                solution
                    .Projects
                    .Select(async x =>
                        {
                            var compilation = await x.GetCompilationAsync();
                            // make sure the compilation has a reference to PCLMock
                            compilation = compilation.AddReferences(MetadataReference.CreateFromAssembly(typeof(MockBase<>).Assembly));
                            return new { Project = x, Compilation = compilation };
                        }));

            return compilations
                .SelectMany(x =>
                    x
                        .Compilation
                        .SyntaxTrees
                        .Select(y =>
                            new
                            {
                                Project = x.Project,
                                Compilation = x.Compilation,
                                SyntaxTree = y,
                                SemanticModel = x.Compilation.GetSemanticModel(y)
                            }))
                .SelectMany(
                    x => x
                        .SyntaxTree
                        .GetRoot()
                        .DescendantNodes()
                        .Where(y => y is InterfaceDeclarationSyntax)
                        .Select(y =>
                            new
                            {
                                Project = x.Project,
                                Compilation = x.Compilation,
                                SyntaxTree = x.SyntaxTree,
                                SemanticModel = x.SemanticModel,
                                InterfaceSymbol = (INamedTypeSymbol)x.SemanticModel.GetDeclaredSymbol(y)
                            }))
                .Where(x => interfacePredicate == null || interfacePredicate(x.InterfaceSymbol))
                .Distinct()
                .Select(x => GenerateMock(x.Project, x.SemanticModel, x.InterfaceSymbol, mockNamespaceSelector(x.InterfaceSymbol), mockNameSelector(x.InterfaceSymbol), language))
                .ToImmutableList();
        }

        private static SyntaxNode GenerateMock(
            Project project,
            SemanticModel semanticModel,
            INamedTypeSymbol interfaceSymbol,
            string mockNamespace,
            string mockName,
            Language language)
        {
            var syntaxGenerator = SyntaxGenerator.GetGenerator(project.Solution.Workspace, language.ToSyntaxGeneratorLanguageName());
            var namespaceSyntax = GetNamespaceDeclarationSyntax(syntaxGenerator, semanticModel, mockNamespace);
            var classSyntax = GetClassDeclarationSyntax(syntaxGenerator, semanticModel, mockName, interfaceSymbol);

            classSyntax = syntaxGenerator
                .AddAttributes(classSyntax, GetClassAttributesSyntax(syntaxGenerator, semanticModel));
            classSyntax = syntaxGenerator
                .AddMembers(classSyntax, GetMemberDeclarations(syntaxGenerator, semanticModel, mockName, interfaceSymbol));
            namespaceSyntax = syntaxGenerator
                .AddMembers(namespaceSyntax, classSyntax);

            return syntaxGenerator
                .CompilationUnit(namespaceSyntax)
                .NormalizeWhitespace();
        }

        private static SyntaxNode GetNamespaceDeclarationSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            string @namespace)
        {
            return syntaxGenerator.NamespaceDeclaration(@namespace);
        }

        private static SyntaxNode GetClassDeclarationSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            string name,
            INamedTypeSymbol interfaceSymbol)
        {
            var interfaceType = syntaxGenerator.TypeExpression(interfaceSymbol);
            var mockBaseType = semanticModel
                .Compilation
                .GetTypeByMetadataName("Kent.Boogaart.PCLMock.MockBase`1");

            if (mockBaseType == null)
            {
                throw new InvalidOperationException("Failed to find type in PCLMock assembly. Are you sure this project has a reference to Kent.Boogaart.PCLMock?");
            }

            var baseType = syntaxGenerator.TypeExpression(
                mockBaseType
                    .Construct(interfaceSymbol));

            var classDeclaration = syntaxGenerator.ClassDeclaration(
                name,
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Partial,
                typeParameters: interfaceSymbol.TypeParameters.Select(x => x.Name),
                baseType: baseType,
                interfaceTypes: new[] { interfaceType });

            // TODO: tidy this up once this issue is rectified: https://github.com/dotnet/roslyn/issues/1658
            foreach (var typeParameter in interfaceSymbol.TypeParameters)
            {
                if (typeParameter.HasConstructorConstraint ||
                    typeParameter.HasReferenceTypeConstraint ||
                    typeParameter.HasValueTypeConstraint ||
                    typeParameter.ConstraintTypes.Length > 0)
                {
                    var kinds = (typeParameter.HasConstructorConstraint ? SpecialTypeConstraintKind.Constructor : SpecialTypeConstraintKind.None) |
                                (typeParameter.HasReferenceTypeConstraint ? SpecialTypeConstraintKind.ReferenceType : SpecialTypeConstraintKind.None) |
                                (typeParameter.HasValueTypeConstraint ? SpecialTypeConstraintKind.ValueType : SpecialTypeConstraintKind.None);

                    classDeclaration = syntaxGenerator.WithTypeConstraint(
                        classDeclaration,
                        typeParameter.Name,
                        kinds: kinds,
                        types: typeParameter.ConstraintTypes.Select(t => syntaxGenerator.TypeExpression(t)));
                }
            }

            return classDeclaration;
        }

        private static IEnumerable<SyntaxNode> GetClassAttributesSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel)
        {
            // GENERATED CODE:
            //
            //     [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "[version]")]
            //     [global::System.Runtime.CompilerServices.CompilerGenerated)]
            yield return syntaxGenerator
                .Attribute(
                    "global::System.CodeDom.Compiler.GeneratedCode",
                    syntaxGenerator.LiteralExpression("PCLMock"),
                    syntaxGenerator.LiteralExpression(typeof(MockBase<>).Assembly.GetName().Version.ToString()));
            yield return syntaxGenerator
                .Attribute(
                    "global::System.Runtime.CompilerServices.CompilerGenerated");
        }

        private static SyntaxNode GetConstructorDeclarationSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            string name)
        {
            // GENERATED CODE:
            //
            //     public Name(MockBehavior behavior = MockBehavior.Strict)
            //         : base(behavior)
            //     {
            //         if (behavior == MockBehavior.Loose)
            //         {
            //             ConfigureLooseBehavior();
            //         }
            //     }
            var mockBehaviorType = syntaxGenerator
                .TypeExpression(
                    semanticModel
                        .Compilation
                        .GetTypeByMetadataName("Kent.Boogaart.PCLMock.MockBehavior"));

            return syntaxGenerator
                .ConstructorDeclaration(
                    name,
                    parameters: new[]
                    {
                        syntaxGenerator
                            .ParameterDeclaration(
                                "behavior",
                                mockBehaviorType,
                                initializer: syntaxGenerator.MemberAccessExpression(mockBehaviorType, "Strict"))
                    },
                    accessibility: Accessibility.Public,
                    baseConstructorArguments: new[] { syntaxGenerator.IdentifierName("behavior") },
                    statements: new[]
                    {
                        syntaxGenerator.IfStatement(
                            syntaxGenerator.ValueEqualsExpression(
                                syntaxGenerator.IdentifierName("behavior"),
                                syntaxGenerator.MemberAccessExpression(mockBehaviorType, "Loose")),
                                new[]
                                {
                                    syntaxGenerator.InvocationExpression(syntaxGenerator.IdentifierName("ConfigureLooseBehavior"))
                                })
                    });
        }

        private static SyntaxNode GetInitializationMethodSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel)
        {
            // GENERATED CODE:
            //
            //     partial void ConfigureLooseBehavior();
            return syntaxGenerator.MethodDeclaration(
                "ConfigureLooseBehavior",
                modifiers: DeclarationModifiers.Partial);
        }

        private static IEnumerable<SyntaxNode> GetMemberDeclarations(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            string name,
            INamedTypeSymbol interfaceSymbol)
        {
            return
                new SyntaxNode[]
                {
                    GetConstructorDeclarationSyntax(syntaxGenerator, semanticModel, name),
                    GetInitializationMethodSyntax(syntaxGenerator, semanticModel)
                }
                .Concat(
                    GetMembersRecursive(interfaceSymbol)
                        .Select(x => GetMemberDeclarationSyntax(syntaxGenerator, semanticModel, x))
                        .Where(x => x != null));
        }

        private static IEnumerable<ISymbol> GetMembersRecursive(INamedTypeSymbol interfaceSymbol)
        {
            foreach (var member in interfaceSymbol.GetMembers())
            {
                yield return member;
            }

            foreach (var implementedInterface in interfaceSymbol.Interfaces)
            {
                foreach (var member in GetMembersRecursive(implementedInterface))
                {
                    yield return member;
                }
            }
        }

        private static SyntaxNode GetMemberDeclarationSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            ISymbol symbol)
        {
            var propertySymbol = symbol as IPropertySymbol;

            if (propertySymbol != null)
            {
                return GetPropertyDeclarationSyntax(syntaxGenerator, semanticModel, propertySymbol);
            }

            var methodSymbol = symbol as IMethodSymbol;

            if (methodSymbol != null)
            {
                return GetMethodDeclarationSyntax(syntaxGenerator, semanticModel, methodSymbol);
            }

            // unsupported symbol type, but we don't error - the user can supplement our code as necessary because it's a partial class
            return null;
        }

        private static SyntaxNode GetPropertyDeclarationSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            IPropertySymbol propertySymbol)
        {
            var getAccessorStatements = GetPropertyGetAccessorsSyntax(syntaxGenerator, semanticModel, propertySymbol).ToList();
            var setAccessorStatements = GetPropertySetAccessorsSyntax(syntaxGenerator, semanticModel, propertySymbol).ToList();
            var declarationModifiers = DeclarationModifiers.None;

            // TODO: requires Roslyn RC2
            //if (getAccessorStatements.Count == 0)
            //{
            //    declarationModifiers = declarationModifiers.WithIsWriteOnly(true);
            //}

            if (setAccessorStatements.Count == 0)
            {
                declarationModifiers = declarationModifiers.WithIsReadOnly(true);
            }

            if (!propertySymbol.IsIndexer)
            {
                return syntaxGenerator
                    .PropertyDeclaration(
                        propertySymbol.Name,
                        syntaxGenerator.TypeExpression(propertySymbol.Type),
                        accessibility: Accessibility.Public,
                        modifiers: declarationModifiers,
                        getAccessorStatements: getAccessorStatements,
                        setAccessorStatements: setAccessorStatements);
            }
            else
            {
                var parameters = propertySymbol
                    .Parameters
                    .Select(x => syntaxGenerator.ParameterDeclaration(x.Name, syntaxGenerator.TypeExpression(x.Type)))
                    .ToList();

                return syntaxGenerator
                    .IndexerDeclaration(
                        parameters,
                        syntaxGenerator.TypeExpression(propertySymbol.Type),
                        accessibility: Accessibility.Public,
                        getAccessorStatements: getAccessorStatements,
                        setAccessorStatements: setAccessorStatements);
            }
        }

        private static IEnumerable<SyntaxNode> GetPropertyGetAccessorsSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            IPropertySymbol propertySymbol)
        {
            if (propertySymbol.GetMethod == null)
            {
                yield break;
            }

            if (!propertySymbol.IsIndexer)
            {
                // GENERATED CODE:
                //
                //     return this.Apply(x => x.PropertyName);
                yield return syntaxGenerator
                    .ReturnStatement(
                        syntaxGenerator.InvocationExpression(
                            syntaxGenerator.MemberAccessExpression(
                                syntaxGenerator.ThisExpression(),
                                "Apply"),
                            syntaxGenerator.ValueReturningLambdaExpression(
                                "x",
                                syntaxGenerator.MemberAccessExpression(
                                    syntaxGenerator.IdentifierName("x"),
                                    syntaxGenerator.IdentifierName(propertySymbol.Name)))));
            }
            else
            {
                // GENERATED CODE:
                //
                //     return this.Apply(x => x[first, second]);
                var arguments = propertySymbol
                    .Parameters
                    .Select(x => syntaxGenerator.Argument(syntaxGenerator.IdentifierName(x.Name)))
                    .ToList();

                yield return syntaxGenerator
                    .ReturnStatement(
                        syntaxGenerator.InvocationExpression(
                            syntaxGenerator.MemberAccessExpression(
                                syntaxGenerator.ThisExpression(),
                                "Apply"),
                            syntaxGenerator.ValueReturningLambdaExpression(
                                "x",
                                syntaxGenerator.ElementAccessExpression(
                                    syntaxGenerator.IdentifierName("x"),
                                    arguments))));
            }
        }

        private static IEnumerable<SyntaxNode> GetPropertySetAccessorsSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            IPropertySymbol propertySymbol)
        {
            if (propertySymbol.SetMethod == null)
            {
                yield break;
            }

            if (!propertySymbol.IsIndexer)
            {
                // GENERATED CODE:
                //
                //     this.ApplyPropertySet(x => x.PropertyName, value);
                yield return syntaxGenerator
                    .InvocationExpression(
                        syntaxGenerator.MemberAccessExpression(
                            syntaxGenerator.ThisExpression(),
                            "ApplyPropertySet"),
                        syntaxGenerator.ValueReturningLambdaExpression(
                            "x",
                            syntaxGenerator.MemberAccessExpression(
                                syntaxGenerator.IdentifierName("x"),
                                syntaxGenerator.IdentifierName(propertySymbol.Name))),
                        syntaxGenerator.IdentifierName("value"));
            }
            else
            {
                // GENERATED CODE:
                //
                //     this.ApplyPropertySet(x => x[first, second], value);
                var arguments = propertySymbol
                    .Parameters
                    .Select(x => syntaxGenerator.Argument(syntaxGenerator.IdentifierName(x.Name)))
                    .ToList();

                yield return syntaxGenerator
                    .InvocationExpression(
                        syntaxGenerator.MemberAccessExpression(
                            syntaxGenerator.ThisExpression(),
                            "ApplyPropertySet"),
                        syntaxGenerator.ValueReturningLambdaExpression(
                            "x",
                            syntaxGenerator.ElementAccessExpression(
                                syntaxGenerator.IdentifierName("x"),
                                arguments)),
                        syntaxGenerator.IdentifierName("value"));
            }
        }

        private static SyntaxNode GetMethodDeclarationSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind != MethodKind.Ordinary)
            {
                return null;
            }

            var methodDeclaration = syntaxGenerator
                .MethodDeclaration(methodSymbol);
            methodDeclaration = syntaxGenerator
                .WithModifiers(
                    methodDeclaration,
                    syntaxGenerator
                        .GetModifiers(methodDeclaration)
                        .WithIsAbstract(false));
            methodDeclaration = syntaxGenerator
                .WithStatements(
                    methodDeclaration,
                    GetMethodStatementsSyntax(syntaxGenerator, semanticModel, methodSymbol));

            var csharpMethodDeclaration = methodDeclaration as MethodDeclarationSyntax;

            if (csharpMethodDeclaration != null)
            {
                // remove trailing semi-colon from the declaration
                methodDeclaration = csharpMethodDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
            }

            return methodDeclaration;
        }

        private static IEnumerable<SyntaxNode> GetMethodStatementsSyntax(
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel,
            IMethodSymbol methodSymbol)
        {
            // GENERATED CODE (for every ref or out parameter):
            //
            //     string someOutParameter;
            //     var someRefParameter = default(int);
            for (var i = 0; i < methodSymbol.Parameters.Length; ++i)
            {
                var parameter = methodSymbol.Parameters[i];

                if (parameter.RefKind == RefKind.Out)
                {
                    yield return syntaxGenerator
                        .LocalDeclarationStatement(
                            syntaxGenerator.TypeExpression(parameter.Type),
                            GetNameForParameter(parameter));
                }
                else if (parameter.RefKind == RefKind.Ref)
                {
                    yield return syntaxGenerator
                        .LocalDeclarationStatement(
                            GetNameForParameter(parameter),
                            initializer: syntaxGenerator.DefaultExpression(syntaxGenerator.TypeExpression(parameter.Type)));
                }
            }

            var arguments = methodSymbol
                .Parameters
                .Select(x =>
                    syntaxGenerator
                        .Argument(
                            x.RefKind,
                            syntaxGenerator.IdentifierName(GetNameForParameter(x))))
                .ToList();

            var typeArguments = methodSymbol
                .TypeArguments
                .Select(x => syntaxGenerator.TypeExpression(x))
                .ToList();

            var lambdaInvocation = syntaxGenerator
                .MemberAccessExpression(
                    syntaxGenerator.IdentifierName("x"),
                    methodSymbol.Name);

            if (typeArguments.Count > 0)
            {
                lambdaInvocation = syntaxGenerator
                    .WithTypeArguments(
                        lambdaInvocation,
                        typeArguments);
            }

            // GENERATED CODE (for every ref or out parameter):
            //
            //     someOutParameter = this.GetOutParameterValue<string>(x => x.TheMethod(out someOutParameter), parameterIndex: 0);
            //     someRefParameter = this.GetRefParameterValue<int>(x => x.TheMethod(ref someRefParameter), parameterIndex: 0);
            for (var i = 0; i < methodSymbol.Parameters.Length; ++i)
            {
                var parameter = methodSymbol.Parameters[i];

                if (parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref)
                {
                    var nameOfMethodToCall = parameter.RefKind == RefKind.Out ? "GetOutParameterValue" : "GetRefParameterValue";

                    yield return syntaxGenerator
                        .AssignmentStatement(
                            syntaxGenerator.IdentifierName(parameter.Name),
                            syntaxGenerator
                            .InvocationExpression(
                                syntaxGenerator.MemberAccessExpression(
                                    syntaxGenerator.ThisExpression(),
                                    syntaxGenerator.GenericName(
                                        nameOfMethodToCall,
                                        typeArguments: syntaxGenerator.TypeExpression(parameter.Type))),
                                        arguments: new[]
                                        {
                                            syntaxGenerator.ValueReturningLambdaExpression(
                                                "x",
                                                syntaxGenerator.InvocationExpression(
                                                    lambdaInvocation,
                                                    arguments: arguments)),
                                                syntaxGenerator.LiteralExpression(i)
                                        }));
                }
            }

            // GENERATED CODE:
            //
            //     [return] this.Apply(x => x.SomeMethod(param1, param2));
            var applyInvocation = syntaxGenerator
                .InvocationExpression(
                    syntaxGenerator.MemberAccessExpression(
                        syntaxGenerator.ThisExpression(),
                        "Apply"),
                    syntaxGenerator.ValueReturningLambdaExpression(
                        "x",
                        syntaxGenerator.InvocationExpression(
                            lambdaInvocation,
                            arguments: arguments)));

            if (!methodSymbol.ReturnsVoid)
            {
                applyInvocation = syntaxGenerator.ReturnStatement(applyInvocation);
            }

            yield return applyInvocation;
        }

        private static string GetNameForParameter(IParameterSymbol parameterSymbol)
        {
            switch (parameterSymbol.RefKind)
            {
                case RefKind.None:
                    return parameterSymbol.Name;
                case RefKind.Ref:
                    return "_" + parameterSymbol.Name + "Ref";
                case RefKind.Out:
                    return "_" + parameterSymbol.Name + "Out";
                default:
                    throw new NotSupportedException("Unknown parameter ref kind: " + parameterSymbol.RefKind);
            }
        }
    }
}