namespace Kent.Boogaart.PCLMock.UnitTests
{
    using System;
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class MockBaseFixture
    {
        [Fact]
        public void mocked_object_throws_if_the_mocked_object_cannot_be_automatically_determined()
        {
            var mock = new InvalidMockedObject();
            var ex = Assert.Throws<InvalidOperationException>(() => mock.MockedObject);
            var expectedMessage = @"The default implementation of MockBase<UnsealedClass> is unable to automatically determine an instance of type UnsealedClass to be used as the mocked object. You should override MockedObject in InvalidMockedObject and return the mocked object.
Full mock type name: Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture+InvalidMockedObject
Full mocked object type name: Kent.Boogaart.PCLMock.UnitTests.MockBaseFixture+UnsealedClass";

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void mocked_object_attempts_to_cast_the_mock_to_the_mocked_type_by_default()
        {
            var mock = new TestTargetMock();
            Assert.Same(mock, mock.MockedObject);
        }

        [Fact]
        public void when_can_be_used_to_specify_result_of_property_getter()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeProperty).Return(6);

            Assert.Equal(6, mock.SomeProperty);
        }

        [Fact]
        public void when_can_be_used_to_specify_what_happens_when_a_property_setter_is_accessed()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeProperty).Do<int>((_) => called = true);

            mock.SomeProperty = 1;
            Assert.True(called);
        }

        [Fact]
        public void when_can_be_used_to_specify_what_happens_when_a_void_parameterless_method_is_invoked()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeMethod()).Do(() => called = true);

            mock.SomeMethod();
            Assert.True(called);
        }

        [Fact]
        public void when_can_be_used_to_specify_what_happens_when_a_void_method_is_invoked()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<float>())).Do<int, float>((_, __) => called = true);

            mock.SomeMethod(1, 1f);
            Assert.True(called);
        }

        [Fact]
        public void when_can_be_used_to_specify_what_happens_when_a_parameterless_method_is_invoked()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeMethodWithReturnValue()).Do(() => called = true);

            mock.SomeMethodWithReturnValue();
            Assert.True(called);
        }

        [Fact]
        public void when_can_be_used_to_specify_what_happens_when_a_method_is_invoked()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeMethodWithReturnValue(It.IsAny<int>(), It.IsAny<float>())).Do<int, float>((_, __) => called = true);

            mock.SomeMethodWithReturnValue(1, 1f);
            Assert.True(called);
        }

        [Fact]
        public void do_can_specify_an_action_without_parameters()
        {
            var mock = new TestTargetMock();
            var counter = 0;
            mock.When(x => x.SomeMethod()).Do(() => ++counter);
            mock.When(x => x.SomeMethodWithReturnValue()).Do(() => ++counter);

            mock.SomeMethod();
            mock.SomeMethodWithReturnValue();

            Assert.Equal(2, counter);
        }

        [Fact]
        public void do_application_does_not_require_any_parameters_be_present_in_the_action()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<float>())).Do(() => called = true);

            mock.SomeMethod(1, 1f);

            Assert.True(called);
        }

        [Fact]
        public void do_application_throws_if_the_parameters_do_not_match_in_number()
        {
            var mock = new TestTargetMock();

            mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<float>())).Do((int i) => { });
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethod(1, 1f));
            Assert.Equal("Could not execute the Do action associated with this mocked member due to a parameter mismatch. Expected: (System.Int32) Received: (System.Int32, System.Single)", ex.Message);

            mock.When(x => x.SomeProperty).Do((int i, float f) => { });
            ex = Assert.Throws<InvalidOperationException>(() => mock.SomeProperty = 31);
            Assert.Equal("Could not execute the Do action associated with this mocked member due to a parameter mismatch. Expected: (System.Int32, System.Single) Received: (System.Int32)", ex.Message);
        }

        [Fact]
        public void do_application_throws_if_the_parameters_do_not_match_in_type()
        {
            var mock = new TestTargetMock();

            mock.When(x => x.SomeProperty).Do((float f) => { });
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeProperty = 31);
            Assert.Equal("Could not execute the Do action associated with this mocked member due to a parameter mismatch. Expected: (System.Single) Received: (System.Int32)", ex.Message);
        }

        [Fact]
        public void do_can_specify_an_action_with_a_parameter_that_matches_a_propertys_value_type()
        {
            var mock = new TestTargetMock();
            var value = 0;
            mock.When(x => x.SomeProperty).Do<int>(x => value = x);

            mock.SomeProperty = 31;

            Assert.Equal(31, value);
        }

        [Fact]
        public void do_can_specify_an_action_with_parameters_that_match_a_methods_argument_types()
        {
            var mock = new TestTargetMock();
            var iValue = 0;
            var fValue = 0f;
            mock
                .When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<float>()))
                .Do<int, float>(
                    (i, f) =>
                    {
                        iValue = i;
                        fValue = f;
                    });

            mock.SomeMethod(21, 43f);

            Assert.Equal(21, iValue);
            Assert.Equal(43f, fValue);
        }

        [Fact]
        public void return_can_be_used_to_specify_a_constant_return_value_for_property_getter()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeProperty).Return(108);

            Assert.Equal(108, mock.SomeProperty);
        }

        [Fact]
        public void return_can_be_used_to_specify_a_constant_return_value_for_a_parameterless_method()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue()).Return(108);

            Assert.Equal(108, mock.SomeMethodWithReturnValue());
        }

        [Fact]
        public void return_can_be_used_to_specify_a_constant_return_value_for_a_method_with_parameters()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsAny<int>(), It.IsAny<float>())).Return(108);

            Assert.Equal(108, mock.SomeMethodWithReturnValue(1, 1f));
        }

        [Fact]
        public void return_can_specify_an_action_without_parameters()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue()).Return(() => 2 + 8);

            Assert.Equal(10, mock.SomeMethodWithReturnValue());
        }

        [Fact]
        public void return_application_does_not_require_any_parameters_be_present_in_the_action()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.When(x => x.SomeProperty).Do(() => called = true);

            mock.SomeProperty = 21;

            Assert.True(called);
        }

        [Fact]
        public void return_application_throws_if_the_parameters_do_not_match_in_number()
        {
            var mock = new TestTargetMock();

            mock.When(x => x.SomeMethodWithReturnValue()).Return((int i) => 3);
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodWithReturnValue());
            Assert.Equal("Could not execute the Return action associated with this mocked member due to a parameter mismatch. Expected: (System.Int32) Received: ()", ex.Message);

            mock.When(x => x.SomeMethodWithReturnValue()).Return((int i, float f) => 31);
            ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodWithReturnValue());
            Assert.Equal("Could not execute the Return action associated with this mocked member due to a parameter mismatch. Expected: (System.Int32, System.Single) Received: ()", ex.Message);
        }

        [Fact]
        public void return_application_throws_if_the_parameters_do_not_match_in_type()
        {
            var mock = new TestTargetMock();

            mock.When(x => x.SomeMethodWithReturnValue(It.IsAny<int>(), It.IsAny<float>())).Return((string s, int i) => 31);
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodWithReturnValue(1, 1f));
            Assert.Equal("Could not execute the Return action associated with this mocked member due to a parameter mismatch. Expected: (System.String, System.Int32) Received: (System.Int32, System.Single)", ex.Message);
        }

        [Fact]
        public void return_can_specify_an_action_with_parameters_that_match_a_methods_argument_types()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsAny<int>(), It.IsAny<float>())).Return((int i, float f) => i + (int)f);

            Assert.Equal(11, mock.SomeMethodWithReturnValue(3, 8.1f));
        }

        [Fact]
        public void throw_can_specify_an_exception_to_throw_on_property_access()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeProperty).Throw(new InvalidOperationException("Problem!"));

            var ex = Assert.Throws<InvalidOperationException>(() => Console.Write(mock.SomeProperty));
            Assert.Equal("Problem!", ex.Message);
        }

        [Fact]
        public void throw_can_specify_an_exception_to_throw_on_parameterless_method_access()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod()).Throw(new InvalidOperationException("Problem!"));

            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethod());
            Assert.Equal("Problem!", ex.Message);
        }

        [Fact]
        public void throw_can_specify_an_exception_to_throw_on_parameterized_method_access()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod(It.IsAny<int>(), It.IsAny<float>())).Throw(new InvalidOperationException("Problem!"));

            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethod(1, 1f));
            Assert.Equal("Problem!", ex.Message);
        }

        [Fact]
        public void actions_can_accept_base_types_in_place_of_specific_types()
        {
            var mock = new TestTargetMock();
            object value = null;
            mock.When(x => x.SomeMethodTakingString(It.IsAny<string>())).Do((object o) => value = o);

            mock.SomeMethodTakingString("whatever");
            Assert.Equal("whatever", value);
        }

        [Fact]
        public void methods_with_out_parameters_require_that_the_out_parameter_be_assigned()
        {
            var mock = new TestTargetMock();
            var called = false;
            string s;
            mock.When(x => x.SomeMethodWithOutParameter(out s)).Do(() => called = true);

            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodWithOutParameter(out s));
            Assert.Equal("No value for out parameter at index 0 has been specified.", ex.Message);
            Assert.False(called);
        }

        [Fact]
        public void methods_with_out_parameters_require_that_the_out_parameter_value_be_the_correct_type()
        {
            var mock = new TestTargetMock();
            var called = false;
            string s;
            mock.When(x => x.SomeMethodWithOutParameter(out s)).AssignOutOrRefParameter(0, 1).Do(() => called = true);

            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodWithOutParameter(out s));
            Assert.Equal("Out parameter at index 0 has a value of type 'System.Int32' but type 'System.String' was expected.", ex.Message);
            Assert.False(called);
        }

        [Fact]
        public void methods_with_value_type_out_parameters_cannot_have_null_assigned_as_the_value()
        {
            var mock = new TestTargetMock();
            var called = false;
            int i;
            mock.When(x => x.SomeMethodWithOutValueParameter(out i)).AssignOutOrRefParameter(0, null).Do(() => called = true);

            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodWithOutValueParameter(out i));
            Assert.Equal("Out parameter at index 0 has a null value specified but it is a value type ('System.Int32') so cannot be null.", ex.Message);
            Assert.False(called);
        }

        [Fact]
        public void methods_with_out_parameters_of_reference_type_can_be_mocked()
        {
            var mock = new TestTargetMock();
            string s;
            mock.When(x => x.SomeMethodWithOutParameter(out s)).AssignOutOrRefParameter(0, "the value");

            mock.SomeMethodWithOutParameter(out s);
            Assert.Equal("the value", s);
        }

        [Fact]
        public void methods_with_out_parameters_of_value_type_can_be_mocked()
        {
            var mock = new TestTargetMock();
            int i;
            mock.When(x => x.SomeMethodWithOutValueParameter(out i)).AssignOutOrRefParameter(0, 38);

            mock.SomeMethodWithOutValueParameter(out i);
            Assert.Equal(38, i);
        }

        [Fact]
        public void methods_with_ref_parameters_of_reference_type_can_be_mocked()
        {
            var mock = new TestTargetMock();
            string s = null;
            mock.When(x => x.SomeMethodWithRefParameter(ref s)).AssignOutOrRefParameter(0, "the value");

            mock.SomeMethodWithRefParameter(ref s);
            Assert.Equal("the value", s);
        }

        [Fact]
        public void methods_with_ref_parameters_of_value_type_can_be_mocked()
        {
            var mock = new TestTargetMock();
            int i = 0;
            mock.When(x => x.SomeMethodWithRefValueParameter(ref i)).AssignOutOrRefParameter(0, 38);

            mock.SomeMethodWithRefValueParameter(ref i);
            Assert.Equal(38, i);
        }

        [Fact]
        public void methods_with_ref_parameters_dont_require_a_parameter_assignment_when_setting_expectations()
        {
            var mock = new TestTargetMock();
            var called = false;
            int i = 0;
            mock.When(x => x.SomeMethodWithRefValueParameter(ref i)).Do(() => called = true);

            mock.SomeMethodWithRefValueParameter(ref i);
            Assert.True(called);
        }

        [Fact]
        public void methods_with_a_mixture_of_out_and_ref_parameters_can_be_mocked()
        {
            var mock = new TestTargetMock();
            var called = false;
            int i1;
            int i2 = 0;
            string s1;
            string s2 = null;
            mock.When(x => x.SomeMethodWithAMixtureOfParameterTypes(0, null, out i1, out s1, ref i2, ref s2))
                .AssignOutOrRefParameter(2, 38)
                .AssignOutOrRefParameter(3, "hello")
                .AssignOutOrRefParameter(4, 14)
                .AssignOutOrRefParameter(5, "world")
                .Do(() => called = true);

            mock.SomeMethodWithAMixtureOfParameterTypes(0, null, out i1, out s1, ref i2, ref s2);
            Assert.True(called);
            Assert.Equal(38, i1);
            Assert.Equal("hello", s1);
            Assert.Equal(14, i2);
            Assert.Equal("world", s2);
        }

        [Fact]
        public void virtual_members_in_unsealed_classes_can_be_mocked()
        {
            var mock = new UnsealedClassMock();
            mock.When(x => x.DoSomething()).Return(true);

            Assert.True(mock.MockedObject.DoSomething());
        }

        #region Supporting Members

        private interface ITestTarget
        {
            int SomeProperty
            {
                get;
                set;
            }

            void SomeMethod();

            void SomeMethod(int i, float f);

            int SomeMethodWithReturnValue();

            int SomeMethodWithReturnValue(int i, float f);

            void SomeMethodTakingString(string s);

            void SomeMethodWithOutParameter(out string s);

            void SomeMethodWithOutValueParameter(out int i);

            void SomeMethodWithRefParameter(ref string s);

            void SomeMethodWithRefValueParameter(ref int i);

            bool SomeMethodWithAMixtureOfParameterTypes(int i, string s, out int i2, out string s2, ref int i3, ref string s3);
        }

        private sealed class TestTargetMock : MockBase<ITestTarget>, ITestTarget
        {
            public TestTargetMock(MockBehavior behavior = MockBehavior.Strict)
                : base(behavior)
            {
            }
                
            public int SomeProperty
            {
                get { return this.Apply(x => x.SomeProperty); }
                set { this.Apply(x => x.SomeProperty, value); }
            }

            public void SomeMethod()
            {
                this.Apply(x => x.SomeMethod());
            }

            public void SomeMethod(int i, float f)
            {
                this.Apply(x => x.SomeMethod(i, f), i, f);
            }

            public int SomeMethodWithReturnValue()
            {
                return this.Apply(x => x.SomeMethodWithReturnValue());
            }

            public int SomeMethodWithReturnValue(int i, float f)
            {
                return this.Apply(x => x.SomeMethodWithReturnValue(i, f), i, f);
            }

            public void SomeMethodTakingString(string s)
            {
                this.Apply(x => x.SomeMethodTakingString(s), s);
            }

            public void SomeMethodWithOutParameter(out string s)
            {
                string sOut;
                s = this.GetOutParameterValue<string>(x => x.SomeMethodWithOutParameter(out sOut), 0);
                this.Apply(x => x.SomeMethodWithOutParameter(out sOut));
            }

            public void SomeMethodWithOutValueParameter(out int i)
            {
                int iOut;
                i = this.GetOutParameterValue<int>(x => x.SomeMethodWithOutValueParameter(out iOut), 0);
                this.Apply(x => x.SomeMethodWithOutValueParameter(out iOut));
            }

            public void SomeMethodWithRefParameter(ref string s)
            {
                var sRef = default(string);
                s = this.GetRefParameterValue<string>(x => x.SomeMethodWithRefParameter(ref sRef), 0);
                this.Apply(x => x.SomeMethodWithRefParameter(ref sRef));
            }

            public void SomeMethodWithRefValueParameter(ref int i)
            {
                var iRef = default(int);
                i = this.GetRefParameterValue<int>(x => x.SomeMethodWithRefValueParameter(ref iRef), 0);
                this.Apply(x => x.SomeMethodWithRefValueParameter(ref iRef));
            }

            public bool SomeMethodWithAMixtureOfParameterTypes(int i, string s, out int i2, out string s2, ref int i3, ref string s3)
            {
                int i2Out;
                string s2Out;
                var i3Ref = default(int);
                var s3Ref = default(string);
                i2 = this.GetOutParameterValue<int>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out i2Out, out s2Out, ref i3Ref, ref s3Ref), 2);
                s2 = this.GetOutParameterValue<string>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out i2Out, out s2Out, ref i3Ref, ref s3Ref), 3);
                i3 = this.GetRefParameterValue<int>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out i2Out, out s2Out, ref i3Ref, ref s3Ref), 4);
                s3 = this.GetRefParameterValue<string>(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out i2Out, out s2Out, ref i3Ref, ref s3Ref), 5);
                return this.Apply(x => x.SomeMethodWithAMixtureOfParameterTypes(i, s, out i2Out, out s2Out, ref i3Ref, ref s3Ref));
            }
        }

        private class UnsealedClass
        {
            public virtual bool DoSomething()
            {
                return false;
            }
        }

        private class UnsealedClassMock : MockBase<UnsealedClass>
        {
            private readonly UnsealedClass mockedObject;

            public UnsealedClassMock(MockBehavior behavior = MockBehavior.Strict)
                : base(behavior)
            {
                this.mockedObject = new UnsealedClassSubclass(this);
            }

            public override UnsealedClass MockedObject
            {
                get { return this.mockedObject; }
            }

            private sealed class UnsealedClassSubclass : UnsealedClass
            {
                private readonly UnsealedClassMock owner;

                public UnsealedClassSubclass(UnsealedClassMock owner)
                {
                    this.owner = owner;
                }

                public override bool DoSomething()
                {
                    return this.owner.Apply(x => x.DoSomething());
                }
            }
        }

        private class InvalidMockedObject : MockBase<UnsealedClass>
        {
            public InvalidMockedObject(MockBehavior behavior = MockBehavior.Strict)
                : base(behavior)
            {
            }
        }

        #endregion
    }
}