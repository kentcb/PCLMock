namespace Kent.Boogaart.PCLMock.UnitTests
{
    using System;
    using System.Text.RegularExpressions;
    using Kent.Boogaart.PCLMock;
    using Xunit;
    using Xunit.Extensions;

    public sealed class MockBaseFixture
    {
        [Fact]
        public void indexer_properties_can_be_mocked()
        {
            var mock = new TestTargetMock();
            mock.When(x => x[1]).Return(3);

            Assert.Equal(3, mock[1]);
        }

        [Fact]
        public void indexer_properties_with_multiple_indexes_can_be_mocked()
        {
            var mock = new TestTargetMock();
            mock.When(x => x[1, 3]).Return(3);
            mock.When(x => x[3, 1]).Return(5);

            Assert.Equal(3, mock[1, 3]);
            Assert.Equal(5, mock[3, 1]);
        }
        
        [Fact]
        public void apply_throws_if_method_without_specifications_is_invoked_on_a_strict_mock()
        {
            var mock = new TestTargetMock();
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethod());
            Assert.Equal("Method 'SomeMethod', for which no specifications have been configured, was invoked on a strict mock. You must either configure specifications via calls to When on the mock, or use a loose mock by passing in MockBehavior.Loose to the mock's constructor.", ex.Message);
        }

        [Fact]
        public void apply_throws_if_property_without_specifications_is_invoked_on_a_strict_mock()
        {
            var mock = new TestTargetMock();
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeProperty);
            Assert.Equal("Property 'SomeProperty', for which no specifications have been configured, was invoked on a strict mock. You must either configure specifications via calls to When on the mock, or use a loose mock by passing in MockBehavior.Loose to the mock's constructor.", ex.Message);
        }

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
        public void when_throws_if_property_access_is_chained()
        {
            var mock = new TestTargetMock();
            var ex = Assert.Throws<InvalidOperationException>(() => mock.When(x => x.SomeComplexProperty.Name).Return("foo"));
            Assert.Equal("Specifications against properties cannot be chained: x.SomeComplexProperty.Name", ex.Message);
        }

        [Fact]
        public void when_throws_if_method_access_is_chained()
        {
            var mock = new TestTargetMock();
            var ex = Assert.Throws<InvalidOperationException>(() => mock.When(x => x.SomeComplexMethod().GetAge()).Return(35));
            Assert.Equal("Specifications against methods cannot be chained: x.SomeComplexMethod().GetAge()", ex.Message);
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
            mock.WhenPropertySet(x => x.SomeProperty).Do<int>((_) => called = true);

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
        public void when_property_set_can_be_used_to_specify_what_happens_when_a_property_is_set()
        {
            var mock = new TestTargetMock();
            var called = false;
            mock.WhenPropertySet(x => x.SomeProperty).Do(() => called = true);

            mock.SomeProperty = 12;
            Assert.True(called);
        }

        [Fact]
        public void when_property_set_can_filter_arguments()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            var called = false;
            mock.WhenPropertySet(x => x.SomeProperty, () => It.IsIn(1, 2, 3, 5, 8)).Do(() => called = true);

            mock.SomeProperty = 12;
            mock.SomeProperty = 11;
            Assert.False(called);

            mock.SomeProperty = 5;
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
        public void do_can_take_parameters_when_specifying_a_property_set()
        {
            var mock = new TestTargetMock();
            int capturedValue = 0;
            mock.WhenPropertySet(x => x.SomeProperty).Do((int x) => capturedValue = x);

            mock.SomeProperty = 35;
            Assert.Equal(35, capturedValue);
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

            mock.WhenPropertySet(x => x.SomeProperty).Do((int i, float f) => { });
            ex = Assert.Throws<InvalidOperationException>(() => mock.SomeProperty = 31);
            Assert.Equal("Could not execute the Do action associated with this mocked member due to a parameter mismatch. Expected: (System.Int32, System.Single) Received: (System.Int32)", ex.Message);
        }

        [Fact]
        public void do_application_throws_if_the_parameters_do_not_match_in_type()
        {
            var mock = new TestTargetMock();

            mock.WhenPropertySet(x => x.SomeProperty).Do((float f) => { });
            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeProperty = 31);
            Assert.Equal("Could not execute the Do action associated with this mocked member due to a parameter mismatch. Expected: (System.Single) Received: (System.Int32)", ex.Message);
        }

        [Fact]
        public void do_can_specify_an_action_with_a_parameter_that_matches_a_propertys_value_type()
        {
            var mock = new TestTargetMock();
            var value = 0;
            mock.WhenPropertySet(x => x.SomeProperty).Do<int>(x => value = x);

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
            mock.WhenPropertySet(x => x.SomeProperty).Do(() => called = true);

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
        public void throw_throws_an_invalid_operation_exception_by_default()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeProperty).Throw();
            mock.When(x => x.SomeMethodTakingString("abc")).Throw();

            var ex = Assert.Throws<InvalidOperationException>(() => mock.SomeProperty);
            Assert.Equal("Mock has been configured to throw when accessing SomeProperty.", ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => mock.SomeMethodTakingString("abc"));
            Assert.Equal("Mock has been configured to throw when accessing SomeMethodTakingString(It.Is(\"abc\")).", ex.Message);
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
        public void methods_with_ref_parameters_dont_require_a_parameter_assignment_when_configuring_specifications()
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

        [Fact]
        public void it_is_any_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsAny<int>(), It.IsAny<float>())).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(5, 1f));
        }

        [Fact]
        public void it_is_can_be_used_explicitly_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.Is(3), It.Is(5f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(3, 5f));
        }

        [Fact]
        public void it_is_can_be_used_implicitly_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(3, 5f)).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(3, 5f));
        }

        [Fact]
        public void it_is_not_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsNot(3), It.IsNot(5f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(2, 4f));
        }

        [Fact]
        public void it_is_null_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.IsNull<string>())).Return(35);

            Assert.Equal(35, mock.SomeMethodTakingStringWithReturnValue(null));
        }

        [Fact]
        public void it_is_not_null_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.IsNotNull<string>())).Return(35);

            Assert.Equal(35, mock.SomeMethodTakingStringWithReturnValue("foo"));
        }

        [Fact]
        public void it_is_less_than_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsLessThan(3), It.IsLessThan(5f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(2, 4f));
        }

        [Fact]
        public void it_is_less_than_or_equal_to_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsLessThanOrEqualTo(3), It.IsLessThanOrEqualTo(5f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(2, 4f));
        }

        [Fact]
        public void it_is_greater_than_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsGreaterThan(3), It.IsGreaterThan(5f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(4, 6f));
        }

        [Fact]
        public void it_is_greater_than_or_equal_to_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsGreaterThanOrEqualTo(3), It.IsGreaterThanOrEqualTo(5f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(4, 6f));
        }

        [Fact]
        public void it_is_between_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsBetween(3, 8), It.IsBetween(5f, 10f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(4, 9f));
        }

        [Fact]
        public void it_is_not_between_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodWithReturnValue(It.IsNotBetween(3, 8), It.IsNotBetween(5f, 10f))).Return(35);

            Assert.Equal(35, mock.SomeMethodWithReturnValue(2, 11f));
        }

        [Fact]
        public void it_is_of_type_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingObjectWithReturnValue(It.IsOfType<int>())).Return(30);
            mock.When(x => x.SomeMethodTakingObjectWithReturnValue(It.IsOfType<string>())).Return(35);

            Assert.Equal(30, mock.SomeMethodTakingObjectWithReturnValue(4));
            Assert.Equal(35, mock.SomeMethodTakingObjectWithReturnValue("foo"));
        }

        [Fact]
        public void it_is_not_of_type_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingObjectWithReturnValue(It.IsNotOfType<int>())).Return(30);
            mock.When(x => x.SomeMethodTakingObjectWithReturnValue(It.IsNotOfType<string>())).Return(35);

            Assert.Equal(35, mock.SomeMethodTakingObjectWithReturnValue(4));
            Assert.Equal(30, mock.SomeMethodTakingObjectWithReturnValue("foo"));
            Assert.Equal(35, mock.SomeMethodTakingObjectWithReturnValue(1f));
        }

        [Fact]
        public void it_is_in_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            mock.When(x => x.SomeMethodTakingObjectWithReturnValue(It.IsIn(1, 2, 3, 5, 8, 13))).Return(30);

            Assert.Equal(30, mock.SomeMethodTakingObjectWithReturnValue(2));
            Assert.Equal(30, mock.SomeMethodTakingObjectWithReturnValue(8));
            Assert.Equal(30, mock.SomeMethodTakingObjectWithReturnValue(13));
            Assert.Equal(0, mock.SomeMethodTakingObjectWithReturnValue(4));
        }

        [Fact]
        public void it_is_like_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.IsLike(@"[Hh]ello\s*?world."))).Return(30);

            Assert.Equal(30, mock.SomeMethodTakingStringWithReturnValue("hello world!"));
            Assert.Equal(30, mock.SomeMethodTakingStringWithReturnValue("Hello world!"));
            Assert.Equal(30, mock.SomeMethodTakingStringWithReturnValue("hello \t  world."));
        }

        [Fact]
        public void it_is_like_can_be_used_with_options_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.IsLike(@"[Hh]ello\s*?world.", RegexOptions.IgnoreCase))).Return(30);

            Assert.Equal(30, mock.SomeMethodTakingStringWithReturnValue("HELLO WoRlD!"));
        }

        [Fact]
        public void it_matches_can_be_used_to_specify_arguments()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.Matches<string>(y => y.StartsWith("K")))).Return(30);
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.Matches<string>(y => y.StartsWith("B")))).Return(29);

            Assert.Equal(30, mock.SomeMethodTakingStringWithReturnValue("Kent"));
            Assert.Equal(30, mock.SomeMethodTakingStringWithReturnValue("Kart"));
            Assert.Equal(29, mock.SomeMethodTakingStringWithReturnValue("Belinda"));
            Assert.Equal(29, mock.SomeMethodTakingStringWithReturnValue("Batman"));
        }

        [Fact]
        public void argument_filters_can_be_used_to_differentiate_property_set_invocations()
        {
            var mock = new TestTargetMock();
            var anyCalled = false;
            var specificCalled = false;
            mock.WhenPropertySet(x => x.SomeProperty).Do(() => anyCalled = true);
            mock.WhenPropertySet(x => x.SomeProperty, () => 3).Do(() => specificCalled = true);

            mock.SomeProperty = 30;
            Assert.False(specificCalled);
            Assert.True(anyCalled);

            mock.SomeProperty = 3;
            Assert.True(specificCalled);
        }

        [Fact]
        public void argument_filters_can_be_used_to_differentiate_method_invocations()
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.IsAny<string>())).Return(1);
            mock.When(x => x.SomeMethodTakingStringWithReturnValue(It.IsLike("[Bb].."))).Return(2);
            mock.When(x => x.SomeMethodTakingStringWithReturnValue("foo")).Return(3);
            mock.When(x => x.SomeMethodTakingStringWithReturnValue("bar")).Return(4);

            Assert.Equal(4, mock.SomeMethodTakingStringWithReturnValue("bar"));
            Assert.Equal(3, mock.SomeMethodTakingStringWithReturnValue("foo"));
            Assert.Equal(2, mock.SomeMethodTakingStringWithReturnValue("biz"));
            Assert.Equal(2, mock.SomeMethodTakingStringWithReturnValue("buz"));
            Assert.Equal(2, mock.SomeMethodTakingStringWithReturnValue("Biz"));
            Assert.Equal(1, mock.SomeMethodTakingStringWithReturnValue("whatever"));
            Assert.Equal(1, mock.SomeMethodTakingStringWithReturnValue(null));
        }

        [Fact]
        public void verify_was_not_called_does_not_throw_if_the_member_was_not_invoked()
        {
            var mock = new TestTargetMock();

            mock.Verify(x => x.SomeMethod()).WasNotCalled();
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasNotCalled();
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasNotCalled();
            mock.VerifyPropertySet(x => x.SomeProperty).WasNotCalled();
            mock.VerifyPropertySet(x => x.SomeProperty, () => It.IsAny<int>()).WasNotCalled();
        }

        [Theory]
        [InlineData(1, "Verification that SomeMethod() was not called failed because it was called 1 time.")]
        [InlineData(2, "Verification that SomeMethod() was not called failed because it was called 2 times.")]
        [InlineData(3, "Verification that SomeMethod() was not called failed because it was called 3 times.")]
        [InlineData(15, "Verification that SomeMethod() was not called failed because it was called 15 times.")]
        public void verify_was_not_called_throws_if_the_member_was_invoked(int callCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasNotCalled());
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verify_was_called_exactly_once_does_not_throw_if_the_member_was_called_once()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledExactlyOnce();
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledExactlyOnce();
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledExactlyOnce();
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledExactlyOnce();
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledExactlyOnce();
        }

        [Theory]
        [InlineData(0, "Verification that SomeMethod() was called exactly once failed because it was called 0 times.")]
        [InlineData(2, "Verification that SomeMethod() was called exactly once failed because it was called 2 times.")]
        [InlineData(3, "Verification that SomeMethod() was called exactly once failed because it was called 3 times.")]
        [InlineData(15, "Verification that SomeMethod() was called exactly once failed because it was called 15 times.")]
        public void verify_was_called_exactly_once_throws_if_the_member_was_not_invoked_exactly_once(int callCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasCalledExactlyOnce());
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verify_was_called_at_least_once_does_not_throw_if_the_member_was_called_one_or_more_times()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledAtLeastOnce();
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtLeastOnce();
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtLeastOnce();
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtLeastOnce();
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtLeastOnce();

            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;
            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledAtLeastOnce();
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtLeastOnce();
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtLeastOnce();
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtLeastOnce();
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtLeastOnce();
        }

        [Theory]
        [InlineData(0, "Verification that SomeMethod() was called at least once failed because it was called 0 times.")]
        public void verify_was_called_at_least_once_throws_if_the_member_was_not_invoked(int callCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasCalledAtLeastOnce());
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verify_was_called_at_most_once_does_not_throw_if_the_member_was_called_zero_or_one_times()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);

            mock.Verify(x => x.SomeMethod()).WasCalledAtMostOnce();
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtMostOnce();
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtMostOnce();
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtMostOnce();
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtMostOnce();

            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledAtMostOnce();
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtMostOnce();
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtMostOnce();
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtMostOnce();
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtMostOnce();
        }

        [Theory]
        [InlineData(2, "Verification that SomeMethod() was called at most once failed because it was called 2 times.")]
        [InlineData(3, "Verification that SomeMethod() was called at most once failed because it was called 3 times.")]
        [InlineData(15, "Verification that SomeMethod() was called at most once failed because it was called 15 times.")]
        public void verify_was_called_at_most_once_throws_if_the_member_was_invoked_more_than_once(int callCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasCalledAtMostOnce());
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verify_was_called_exactly_n_times_does_not_throw_if_the_member_was_called_the_correct_number_of_times()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledExactly(times: 1);
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledExactly(times: 2);
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledExactly(times: 2);
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledExactly(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledExactly(times: 1);
        }

        [Theory]
        [InlineData(2, 1, "Verification that SomeMethod() was called exactly 1 time failed because it was called 2 times.")]
        [InlineData(2, 3, "Verification that SomeMethod() was called exactly 3 times failed because it was called 2 times.")]
        [InlineData(3, 2, "Verification that SomeMethod() was called exactly 2 times failed because it was called 3 times.")]
        [InlineData(15, 20, "Verification that SomeMethod() was called exactly 20 times failed because it was called 15 times.")]
        [InlineData(20, 15, "Verification that SomeMethod() was called exactly 15 times failed because it was called 20 times.")]
        public void verify_was_called_exactly_n_times_throws_if_the_member_was_not_invoked_exactly_that_number_of_times(int callCount, int verifyCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasCalledExactly(times: verifyCount));
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verify_was_called_at_least_n_times_does_not_throw_if_the_member_was_called_that_number_of_times_or_more()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledAtLeast(times: 1);
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtLeast(times: 1);
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtLeast(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtLeast(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtLeast(times: 1);

            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;
            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledAtLeast(times: 2);
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtLeast(times: 3);
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtLeast(times: 2);
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtLeast(times: 3);
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtLeast(times: 2);
        }

        [Theory]
        [InlineData(0, 1, "Verification that SomeMethod() was called at least 1 time failed because it was called 0 times.")]
        [InlineData(1, 2, "Verification that SomeMethod() was called at least 2 times failed because it was called 1 time.")]
        [InlineData(2, 3, "Verification that SomeMethod() was called at least 3 times failed because it was called 2 times.")]
        [InlineData(3, 5, "Verification that SomeMethod() was called at least 5 times failed because it was called 3 times.")]
        [InlineData(15, 20, "Verification that SomeMethod() was called at least 20 times failed because it was called 15 times.")]
        public void verify_was_called_at_least_n_times_throws_if_the_member_was_not_invoked_that_number_of_times_or_more(int callCount, int verifyCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasCalledAtLeast(times: verifyCount));
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verify_was_called_at_most_n_times_does_not_throw_if_the_member_was_called_fewer_or_equal_to_that_number_of_times()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);

            mock.Verify(x => x.SomeMethod()).WasCalledAtMost(times: 1);
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtMost(times: 1);
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtMost(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtMost(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtMost(times: 1);

            mock.SomeMethod();
            mock.SomeMethodTakingString("foo");
            mock.SomeProperty = 30;

            mock.Verify(x => x.SomeMethod()).WasCalledAtMost(times: 1);
            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledAtMost(times: 1);
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledAtMost(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty).WasCalledAtMost(times: 1);
            mock.VerifyPropertySet(x => x.SomeProperty, () => 30).WasCalledAtMost(times: 1);
        }

        [Theory]
        [InlineData(1, 0, "Verification that SomeMethod() was called at most 0 times failed because it was called 1 time.")]
        [InlineData(2, 1, "Verification that SomeMethod() was called at most 1 time failed because it was called 2 times.")]
        [InlineData(3, 2, "Verification that SomeMethod() was called at most 2 times failed because it was called 3 times.")]
        [InlineData(20, 15, "Verification that SomeMethod() was called at most 15 times failed because it was called 20 times.")]
        public void verify_was_called_at_most_n_times_throws_if_the_member_was_invoked_more_than_that_number_of_times(int callCount, int verifyCount, string expectedExceptionMessage)
        {
            var mock = new TestTargetMock();
            mock.When(x => x.SomeMethod());

            for (var i = 0; i < callCount; ++i)
            {
                mock.SomeMethod();
            }

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethod()).WasCalledAtMost(times: verifyCount));
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void verification_failures_include_full_information_about_arguments_being_verified()
        {
            var mock = new TestTargetMock();

            var ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethodTakingString("abc")).WasCalledExactlyOnce());
            Assert.Equal("Verification that SomeMethodTakingString(It.Is(\"abc\")) was called exactly once failed because it was called 0 times.", ex.Message);

            ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledExactlyOnce());
            Assert.Equal("Verification that SomeMethodTakingString(It.IsAny<string>()) was called exactly once failed because it was called 0 times.", ex.Message);

            ex = Assert.Throws<VerificationException>(() => mock.Verify(x => x.SomeProperty).WasCalledExactlyOnce());
            Assert.Equal("Verification that SomeProperty was called exactly once failed because it was called 0 times.", ex.Message);
        }

        [Fact]
        public void verification_takes_into_account_any_argument_filters()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            mock.SomeMethodTakingString("foo");
            mock.SomeMethodTakingString("bar");
            mock.SomeMethodTakingString("bar");

            mock.Verify(x => x.SomeMethodTakingString("foo")).WasCalledExactlyOnce();
            mock.Verify(x => x.SomeMethodTakingString("bar")).WasCalledExactly(times: 2);
            mock.Verify(x => x.SomeMethodTakingString(It.IsAny<string>())).WasCalledExactly(times: 3);
        }

        [Fact]
        public void issue13_repro()
        {
            var mock = new TestTargetMock(MockBehavior.Loose);
            var subMock = new TestSubTargetMock(MockBehavior.Loose);
            subMock.When(x => x.Height).Return(3);

            mock.SomeMethodTakingComplexType(subMock);

            mock.Verify(x => x.SomeMethodTakingComplexType(It.Matches<ITestSubTarget>(y => y.Height.HasValue))).WasCalledExactlyOnce();
        }

        #region Supporting Members

        private interface ITestSubTarget
        {
            string Name
            {
                get;
                set;
            }

            int? Height
            {
                get;
            }

            int GetAge();
        }

        private interface ITestTarget
        {
            int SomeProperty
            {
                get;
                set;
            }

            int this[int index]
            {
                get;
                set;
            }

            int this[int first, int second]
            {
                get;
                set;
            }

            ITestSubTarget SomeComplexProperty
            {
                get;
            }

            void SomeMethod();

            ITestSubTarget SomeComplexMethod();

            void SomeMethod(int i, float f);

            int SomeMethodWithReturnValue();

            int SomeMethodWithReturnValue(int i, float f);

            void SomeMethodTakingComplexType(ITestSubTarget a);

            void SomeMethodTakingString(string s);

            int SomeMethodTakingStringWithReturnValue(string s);

            void SomeMethodTakingObject(object o);

            int SomeMethodTakingObjectWithReturnValue(object o);

            void SomeMethodWithOutParameter(out string s);

            void SomeMethodWithOutValueParameter(out int i);

            void SomeMethodWithRefParameter(ref string s);

            void SomeMethodWithRefValueParameter(ref int i);

            bool SomeMethodWithAMixtureOfParameterTypes(int i, string s, out int i2, out string s2, ref int i3, ref string s3);
        }

        private sealed class TestSubTargetMock : MockBase<ITestSubTarget>, ITestSubTarget
        {
            public TestSubTargetMock(MockBehavior behavior)
                : base(behavior)
            {
            }

            public int? Height => this.Apply(x => x.Height);

            public string Name
            {
                get { return this.Apply(x => x.Name); }
                set { this.ApplyPropertySet(x => x.Name, value); }
            }

            public int GetAge() => this.Apply(x => x.GetAge());
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
                set { this.ApplyPropertySet(x => x.SomeProperty, value); }
            }

            public int this[int index]
            {
                get { return this.Apply(x => x[index]); }
                set { this.ApplyPropertySet(x => x[index], value); }
            }

            public int this[int first, int second]
            {
                get { return this.Apply(x => x[first, second]); }
                set { this.ApplyPropertySet(x => x[first, second], value); }
            }

            public ITestSubTarget SomeComplexProperty
            {
                get { return this.Apply(x => x.SomeComplexProperty); }
            }

            public void SomeMethod()
            {
                this.Apply(x => x.SomeMethod());
            }

            public ITestSubTarget SomeComplexMethod()
            {
                return this.Apply(x => x.SomeComplexMethod());
            }

            public void SomeMethod(int i, float f)
            {
                this.Apply(x => x.SomeMethod(i, f));
            }

            public int SomeMethodWithReturnValue()
            {
                return this.Apply(x => x.SomeMethodWithReturnValue());
            }

            public int SomeMethodWithReturnValue(int i, float f)
            {
                return this.Apply(x => x.SomeMethodWithReturnValue(i, f));
            }

            public void SomeMethodTakingComplexType(ITestSubTarget a)
            {
                this.Apply(x => x.SomeMethodTakingComplexType(a));
            }

            public void SomeMethodTakingString(string s)
            {
                this.Apply(x => x.SomeMethodTakingString(s));
            }

            public int SomeMethodTakingStringWithReturnValue(string s)
            {
                return this.Apply(x => x.SomeMethodTakingStringWithReturnValue(s));
            }

            public void SomeMethodTakingObject(object o)
            {
                this.Apply(x => x.SomeMethodTakingObject(o));
            }

            public int SomeMethodTakingObjectWithReturnValue(object o)
            {
                return this.Apply(x => x.SomeMethodTakingObjectWithReturnValue(o));
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