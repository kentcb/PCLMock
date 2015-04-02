namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock.CodeGeneration;
    using Xunit;
    using Xunit.Extensions;

    public sealed class GeneratorFixture
    {
        [Theory(Skip = "Waiting for Roslyn RC2")]
        [InlineData(nameof(ISimpleInterface), Language.CSharp, ExpectedCSharpISimpleInterface)]
        [InlineData(nameof(IInterfaceWithGenericMethods), Language.CSharp, ExpectedCSharpIInterfaceWithGenericMethods)]
        [InlineData("IGenericInterface", Language.CSharp, ExpectedCSharpIGenericInterface)]
        [InlineData(nameof(IInterfaceWithNonMockableMembers), Language.CSharp, ExpectedCSharpIInterfaceWithNonMockableMembers)]
        [InlineData(nameof(IPartialInterface), Language.CSharp, ExpectedCSharpIPartialInterface)]
        [InlineData(nameof(IInheritingInterface), Language.CSharp, ExpectedCSharpIInheritingInterface)]
        [InlineData(nameof(ISimpleInterface), Language.VisualBasic, ExpectedVisualBasicISimpleInterface)]
        [InlineData(nameof(IInterfaceWithGenericMethods), Language.VisualBasic, ExpectedVisualBasicIInterfaceWithGenericMethods)]
        [InlineData("IGenericInterface", Language.VisualBasic, ExpectedVisualBasicIGenericInterface)]
        [InlineData(nameof(IInterfaceWithNonMockableMembers), Language.VisualBasic, ExpectedVisualBasicIInterfaceWithNonMockableMembers)]
        [InlineData(nameof(IPartialInterface), Language.VisualBasic, ExpectedVisualBasicIPartialInterface)]
        [InlineData(nameof(IInheritingInterface), Language.VisualBasic, ExpectedVisualBasicIInheritingInterface)]
        public async Task can_generate_simple_mock(string interfaceName, Language language, string expectedGeneratedCode)
        {
            var result =
                (await Generator.GenerateMocksAsync(
                    @"..\..\..\PCLMock.sln",
                    x => x.Name == interfaceName,
                    x => "The.Namespace",
                    x => "Mock",
                    language))
                .Single();

            var resultAsString = result.ToFullString();
            Assert.Equal(expectedGeneratedCode, resultAsString);
        }

        #region ISimpleInterface

        private interface ISimpleInterface
        {
            int GetProperty
            {
                get;
            }

            int SetProperty
            {
                set;
            }

            int GetSetProperty
            {
                get;
                set;
            }

            void VoidMethod();

            void VoidMethodWithArguments(int i, string s);

            string NonVoidMethod();

            string NonVoidMethodWithArguments(int i, string s);
        }

        private const string ExpectedCSharpISimpleInterface = @"namespace The.Namespace
{
    [global::System.CodeDom.Compiler.GeneratedCode(""PCLMock"", ""1.0.0.0"")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class Mock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.ISimpleInterface>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.ISimpleInterface
    {
        public SimpleMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior();

        public System.Int32 GetProperty
        {
            get
            {
                return this.Apply(x => x.GetProperty);
            }
        }

        public System.Int32 SetProperty
        {
            set
            {
                this.ApplyPropertySet(x => x.SetProperty, value);
            }
        }

        public System.Int32 GetSetProperty
        {
            get
            {
                return this.Apply(x => x.GetSetProperty);
            }

            set
            {
                this.ApplyPropertySet(x => x.GetSetProperty, value);
            }
        }

        public void VoidMethod()
        {
            this.Apply(x => x.VoidMethod());
        }

        public void VoidMethodWithArguments(System.Int32 i, System.String s)
        {
            this.Apply(x => x.VoidMethodWithArguments(i, s));
        }

        public System.String NonVoidMethod()
        {
            return this.Apply(x => x.NonVoidMethod());
        }

        public System.String NonVoidMethodWithArguments(System.Int32 i, System.String s)
        {
            return this.Apply(x => x.NonVoidMethodWithArguments(i, s));
        }
    }
}";

        private const string ExpectedVisualBasicISimpleInterface = @"";

        #endregion

        #region IInterfaceWithGenericMethods

        private interface IInterfaceWithGenericMethods
        {
            void VoidMethodWithGenericParameter<T>();

            T NonVoidMethodWithGenericParameter<T>();

            void VoidMethodWithGenericArguments<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third, string somethingElse);

            TSecond NonVoidMethodWithGenericArguments<TFirst, TSecond>(TFirst input);

            TSecond MethodWithTypeConstraints<TFirst, TSecond>(TFirst input, int option)
                where TFirst : IComparable<TFirst>, new()
                where TSecond : struct;
        }

        private const string ExpectedCSharpIInterfaceWithGenericMethods = @"namespace The.Namespace
{
    [global::System.CodeDom.Compiler.GeneratedCode(""PCLMock"", ""1.0.0.0"")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class Mock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithGenericMethods>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithGenericMethods
    {
        public Mock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior();

        public void VoidMethodWithGenericParameter<T>()
        {
            this.Apply(x => x.VoidMethodWithGenericParameter<T>());
        }

        public T NonVoidMethodWithGenericParameter<T>()
        {
            return this.Apply(x => x.NonVoidMethodWithGenericParameter<T>());
        }

        public void VoidMethodWithGenericArguments<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third, System.String somethingElse)
        {
            this.Apply(x => x.VoidMethodWithGenericArguments<TFirst, TSecond, TThird>(first, second, third, somethingElse));
        }

        public TSecond NonVoidMethodWithGenericArguments<TFirst, TSecond>(TFirst input)
        {
            return this.Apply(x => x.NonVoidMethodWithGenericArguments<TFirst, TSecond>(input));
        }

        public TSecond MethodWithTypeConstraints<TFirst, TSecond>(TFirst input, System.Int32 option)where TFirst : global::System.IComparable<TFirst>, new ()where TSecond : struct
        {
            return this.Apply(x => x.MethodWithTypeConstraints<TFirst, TSecond>(input, option));
        }
    }
}";

        private const string ExpectedVisualBasicIInterfaceWithGenericMethods = @"";

        #endregion

        #region IGenericInterface

        private interface IGenericInterface<TFirst, in TSecond>
            where TFirst : IComparable<TSecond>, new()
            where TSecond : struct
        {
            TFirst SomeProperty
            {
                get;
                set;
            }

            TFirst DoSomething(TSecond input);
        }

        private const string ExpectedCSharpIGenericInterface = @"namespace The.Namespace
{
    [global::System.CodeDom.Compiler.GeneratedCode(""PCLMock"", ""1.0.0.0"")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class Mock<TFirst, TSecond> : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IGenericInterface<TFirst, TSecond>>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IGenericInterface<TFirst, TSecond> where TFirst : global::System.IComparable<TSecond>, new ()where TSecond : struct
    {
        public Mock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior();

        public TFirst SomeProperty
        {
            get
            {
                return this.Apply(x => x.SomeProperty);
            }

            set
            {
                this.ApplyPropertySet(x => x.SomeProperty, value);
            }
        }

        public TFirst DoSomething(TSecond input)
        {
            return this.Apply(x => x.DoSomething(input));
        }
    }
}";

        private const string ExpectedVisualBasicIGenericInterface = @"";

        #endregion

        #region IInterfaceWithNonMockableMembers

        private interface IInterfaceWithNonMockableMembers
        {
            int SomeProperty
            {
                get;
                set;
            }

            // PCLMock does not yet support mocking events (and possibly never will)
            event EventHandler<EventArgs> SomeEvent;
        }

        private const string ExpectedCSharpIInterfaceWithNonMockableMembers = @"namespace The.Namespace
{
    [global::System.CodeDom.Compiler.GeneratedCode(""PCLMock"", ""1.0.0.0"")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class Mock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithNonMockableMembers>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithNonMockableMembers
    {
        public Mock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior();

        public System.Int32 SomeProperty
        {
            get
            {
                return this.Apply(x => x.SomeProperty);
            }

            set
            {
                this.ApplyPropertySet(x => x.SomeProperty, value);
            }
        }
    }
}";

        private const string ExpectedVisualBasicIInterfaceWithNonMockableMembers = @"";

        #endregion

        #region IPartialInterface

        private partial interface IPartialInterface
        {
            int Property1
            {
                get;
            }
        }

        private partial interface IPartialInterface
        {
            int Property2
            {
                get;
            }
        }

        private const string ExpectedCSharpIPartialInterface = @"namespace The.Namespace
{
    [global::System.CodeDom.Compiler.GeneratedCode(""PCLMock"", ""1.0.0.0"")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class Mock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IPartialInterface>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IPartialInterface
    {
        public Mock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior();

        public System.Int32 Property1
        {
            get
            {
                return this.Apply(x => x.Property1);
            }
        }

        public System.Int32 Property2
        {
            get
            {
                return this.Apply(x => x.Property2);
            }
        }
    }
}";

        private const string ExpectedVisualBasicIPartialInterface = @"";

        #endregion

        #region IInheritingInterface

        private interface IInheritingInterface : ICloneable
        {
            int SomeProperty
            {
                get;
            }
        }

        private const string ExpectedCSharpIInheritingInterface = @"namespace The.Namespace
{
    [global::System.CodeDom.Compiler.GeneratedCode(""PCLMock"", ""1.0.0.0"")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class Mock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInheritingInterface>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInheritingInterface
    {
        public Mock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior();

        public System.Int32 SomeProperty
        {
            get
            {
                return this.Apply(x => x.SomeProperty);
            }
        }

        public System.Object Clone()
        {
            return this.Apply(x => x.Clone());
        }
    }
}";

        private const string ExpectedVisualBasicIInheritingInterface = @"";

        #endregion
    }
}