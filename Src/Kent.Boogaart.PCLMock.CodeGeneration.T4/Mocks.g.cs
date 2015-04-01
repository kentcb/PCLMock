namespace Kent.Boogaart.PCLMock.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class ArgumentFilterMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.IArgumentFilter>, global::Kent.Boogaart.PCLMock.IArgumentFilter
    {
        public ArgumentFilterMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

        public System.Boolean Matches(System.Object argument)
        {
            return this.Apply(x => x.Matches(argument));
        }
    }
}
namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class SimpleInterfaceMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.ISimpleInterface>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.ISimpleInterface
    {
        public SimpleInterfaceMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

        public System.Int32 GetProperty
        {
            get
            {
                return this.Apply(x => x.GetProperty);
            }
        }

        public System.Int32 SetProperty
        {
            get
            {
            }

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
}
namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class InterfaceWithGenericMethodsMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithGenericMethods>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithGenericMethods
    {
        public InterfaceWithGenericMethodsMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

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
}
namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class GenericInterface<TFirst, TSecond>Mock<TFirst, TSecond> : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IGenericInterface<TFirst, TSecond>>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IGenericInterface<TFirst, TSecond> where TFirst : global::System.IComparable<TSecond>, new ()where TSecond : struct
    {
        public GenericInterface<TFirst, TSecond>Mock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

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
}
namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class InterfaceWithNonMockableMembersMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithNonMockableMembers>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInterfaceWithNonMockableMembers
    {
        public InterfaceWithNonMockableMembersMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

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
}
namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class PartialInterfaceMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IPartialInterface>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IPartialInterface
    {
        public PartialInterfaceMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

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
}
namespace Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class InheritingInterfaceMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInheritingInterface>, global::Kent.Boogaart.PCLMock.UnitTests.CodeGeneration.GeneratorFixture.IInheritingInterface
    {
        public InheritingInterfaceMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

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
}
namespace Kent.Boogaart.PCLMock.UnitTests.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class TestSubTargetMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture.ITestSubTarget>, global::Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture.ITestSubTarget
    {
        public TestSubTargetMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

        public System.String Name
        {
            get
            {
                return this.Apply(x => x.Name);
            }

            set
            {
                this.ApplyPropertySet(x => x.Name, value);
            }
        }

        public System.Int32 GetAge()
        {
            return this.Apply(x => x.GetAge());
        }
    }
}
namespace Kent.Boogaart.PCLMock.UnitTests.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class TestTargetMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture.ITestTarget>, global::Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture.ITestTarget
    {
        public TestTargetMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

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

        public System.Int32 this[System.Int32 index]
        {
            get
            {
                return this.Apply(x => x[index]);
            }

            set
            {
                this.ApplyPropertySet(x => x[index], value);
            }
        }

        public System.Int32 this[System.Int32 first, System.Int32 second]
        {
            get
            {
                return this.Apply(x => x[first, second]);
            }

            set
            {
                this.ApplyPropertySet(x => x[first, second], value);
            }
        }

        public global::Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture.ITestSubTarget SomeComplexProperty
        {
            get
            {
                return this.Apply(x => x.SomeComplexProperty);
            }
        }

        public void SomeMethod()
        {
            this.Apply(x => x.SomeMethod());
        }

        public global::Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture.ITestSubTarget SomeComplexMethod()
        {
            return this.Apply(x => x.SomeComplexMethod());
        }

        public void SomeMethod(System.Int32 i, System.Single f)
        {
            this.Apply(x => x.SomeMethod(i, f));
        }

        public System.Int32 SomeMethodWithReturnValue()
        {
            return this.Apply(x => x.SomeMethodWithReturnValue());
        }

        public System.Int32 SomeMethodWithReturnValue(System.Int32 i, System.Single f)
        {
            return this.Apply(x => x.SomeMethodWithReturnValue(i, f));
        }

        public void SomeMethodTakingString(System.String s)
        {
            this.Apply(x => x.SomeMethodTakingString(s));
        }

        public System.Int32 SomeMethodTakingStringWithReturnValue(System.String s)
        {
            return this.Apply(x => x.SomeMethodTakingStringWithReturnValue(s));
        }

        public void SomeMethodTakingObject(System.Object o)
        {
            this.Apply(x => x.SomeMethodTakingObject(o));
        }

        public System.Int32 SomeMethodTakingObjectWithReturnValue(System.Object o)
        {
            return this.Apply(x => x.SomeMethodTakingObjectWithReturnValue(o));
        }

        public void SomeMethodWithOutParameter(out System.String s)
        {
            System.String _sOut;
            s = (this.GetOutParameterValue<System.String>(x => x.SomeMethodWithOutParameter(out _sOut), 0));
            this.Apply(x => x.SomeMethodWithOutParameter(out _sOut));
        }

        public void SomeMethodWithOutValueParameter(out System.Int32 i)
        {
            System.Int32 _iOut;
            i = (this.GetOutParameterValue<System.Int32>(x => x.SomeMethodWithOutValueParameter(out _iOut), 0));
            this.Apply(x => x.SomeMethodWithOutValueParameter(out _iOut));
        }

        public void SomeMethodWithRefParameter(ref System.String s)
        {
            var _sRef = default (System.String);
            s = (this.GetRefParameterValue<System.String>(x => x.SomeMethodWithRefParameter(ref _sRef), 0));
            this.Apply(x => x.SomeMethodWithRefParameter(ref _sRef));
        }

        public void SomeMethodWithRefValueParameter(ref System.Int32 i)
        {
            var _iRef = default (System.Int32);
            i = (this.GetRefParameterValue<System.Int32>(x => x.SomeMethodWithRefValueParameter(ref _iRef), 0));
            this.Apply(x => x.SomeMethodWithRefValueParameter(ref _iRef));
        }

        public System.Boolean SomeMethodWithAMixtureOfParameterTypes(System.Int32 i, System.String s, out System.Int32 i2, out System.String s2, ref System.Int32 i3, ref System.String s3)
        {
            System.Int32 _i2Out;
            System.String _s2Out;
            var _i3Ref = default (System.Int32);
            var _s3Ref = default (System.String);
            i2 = (this.GetOutParameterValue<System.Int32>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out _i2Out, out _s2Out, ref _i3Ref, ref _s3Ref), 2));
            s2 = (this.GetOutParameterValue<System.String>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out _i2Out, out _s2Out, ref _i3Ref, ref _s3Ref), 3));
            i3 = (this.GetRefParameterValue<System.Int32>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out _i2Out, out _s2Out, ref _i3Ref, ref _s3Ref), 4));
            s3 = (this.GetRefParameterValue<System.String>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out _i2Out, out _s2Out, ref _i3Ref, ref _s3Ref), 5));
            return this.Apply(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out _i2Out, out _s2Out, ref _i3Ref, ref _s3Ref));
        }
    }
}
namespace Kent.Boogaart.PCLMock.UnitTests.Visitors.Mocks
{
    [global::System.CodeDom.Compiler.GeneratedCode("PCLMock", "1.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class TestTargetMock : global::Kent.Boogaart.PCLMock.MockBase<global::Kent.Boogaart.PCLMock.UnitTests.Visitors.SelectorStringVisitorFixture.ITestTarget>, global::Kent.Boogaart.PCLMock.UnitTests.Visitors.SelectorStringVisitorFixture.ITestTarget
    {
        public TestTargetMock(global::Kent.Boogaart.PCLMock.MockBehavior behavior = global::Kent.Boogaart.PCLMock.MockBehavior.Strict): base (behavior)
        {
            if ((behavior) == (global::Kent.Boogaart.PCLMock.MockBehavior.Loose))
            {
                ConfigureLooseBehavior();
            }
        }

        partial void ConfigureLooseBehavior()
        {
        }

        public System.Int32 ReadOnlyProperty
        {
            get
            {
                return this.Apply(x => x.ReadOnlyProperty);
            }
        }

        public System.Int32 ReadWriteProperty
        {
            get
            {
                return this.Apply(x => x.ReadWriteProperty);
            }

            set
            {
                this.ApplyPropertySet(x => x.ReadWriteProperty, value);
            }
        }

        public System.Int32 MethodWithoutArguments()
        {
            return this.Apply(x => x.MethodWithoutArguments());
        }

        public System.Int32 MethodWithArguments(System.Int32 i, System.String s, System.Object o)
        {
            return this.Apply(x => x.MethodWithArguments(i, s, o));
        }
    }
}

