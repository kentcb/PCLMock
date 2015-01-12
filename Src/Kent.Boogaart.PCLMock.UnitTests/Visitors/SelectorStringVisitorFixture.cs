namespace Kent.Boogaart.PCLMock.UnitTests.Visitors
{
    using Kent.Boogaart.PCLMock.Visitors;
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public sealed class SelectorStringVisitorFixture
    {
        [Fact]
        public void accessing_a_read_only_property_yields_the_correct_string_representation()
        {
            var visitor = new SelectorStringVisitor();
            var expression = (Expression<Func<ITestTarget<int>, int>>)(x => x.ReadOnlyProperty);
            
            visitor.Visit(expression);

            Assert.Equal("ReadOnlyProperty", visitor.ToString());
        }

        [Fact]
        public void accessing_a_read_write_property_yields_the_correct_string_representation()
        {
            var visitor = new SelectorStringVisitor();
            var expression = (Expression<Func<ITestTarget<int>, int>>)(x => x.ReadWriteProperty);

            visitor.Visit(expression);

            Assert.Equal("ReadWriteProperty", visitor.ToString());
        }

        [Fact]
        public void accessing_a_method_without_arguments_yields_the_correct_string_representation()
        {
            var visitor = new SelectorStringVisitor();
            var expression = (Expression<Action<ITestTarget<int>>>)(x => x.MethodWithoutArguments());

            visitor.Visit(expression);

            Assert.Equal("MethodWithoutArguments()", visitor.ToString());

            visitor = new SelectorStringVisitor();
            expression = (Expression<Action<ITestTarget<int>>>)(x => x.MethodWithoutArguments());

            visitor.Visit(expression);

            Assert.Equal("MethodWithoutArguments()", visitor.ToString());
        }

        [Fact]
        public void accessing_a_method_with_arguments_yields_the_correct_string_representation()
        {
            var visitor = new SelectorStringVisitor();
            var expression = (Expression<Action<ITestTarget<int>>>)(x => x.MethodWithArguments(3, It.IsAny<string>(), null));

            visitor.Visit(expression);

            Assert.Equal("MethodWithArguments(It.Is(3), It.IsAny<string>(), It.Is(null))", visitor.ToString());

            visitor = new SelectorStringVisitor();
            expression = (Expression<Action<ITestTarget<int>>>)(x => x.MethodWithArguments(3, It.IsAny<string>(), "abc"));

            visitor.Visit(expression);

            Assert.Equal("MethodWithArguments(It.Is(3), It.IsAny<string>(), It.Is(\"abc\"))", visitor.ToString());

            visitor = new SelectorStringVisitor();
            var value = TimeSpan.FromSeconds(1);
            expression = (Expression<Action<ITestTarget<int>>>)(x => x.MethodWithArguments(3, It.IsAny<string>(), value));

            visitor.Visit(expression);

            Assert.Equal("MethodWithArguments(It.Is(3), It.IsAny<string>(), It.Is(00:00:01 [System.TimeSpan]))", visitor.ToString());
        }

        #region Supporting Members

        private interface ITestTarget<T>
        {
            T ReadOnlyProperty
            {
                get;
            }

            T ReadWriteProperty
            {
                get;
                set;
            }

            T MethodWithoutArguments();

            T MethodWithArguments(int i, string s, object o);
        }

        #endregion
    }
}