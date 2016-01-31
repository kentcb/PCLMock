namespace PCLMock.UnitTests.Visitors
{
    using System;
    using System.Linq.Expressions;
    using PCLMock.Visitors;
    using Xunit;
    using Xunit.Extensions;

    public sealed class ValueExtractorFixture
    {
        [Fact]
        public void can_find_null_in_an_expression()
        {
            Assert.Null(ValueExtractor.FindValueWithin(GetExpression(() => null)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(52367)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(Math.PI)]
        [InlineData(1L)]
        [InlineData(5L)]
        [InlineData(52367L)]
        [InlineData("foo")]
        [InlineData("bar")]
        [InlineData(12f)]
        [InlineData(12.1d)]
        public void can_find_value_in_an_expression_that_simply_returns_a_constant(object value)
        {
            Assert.Equal(value, ValueExtractor.FindValueWithin(GetExpression(() => value)));
        }

        [Fact]
        public void can_find_value_in_an_expression_that_returns_a_dereferenced_value()
        {
            Assert.Equal(DateTime.MinValue, ValueExtractor.FindValueWithin(GetExpression(() => DateTime.MinValue)));
            Assert.Equal(decimal.MinValue, ValueExtractor.FindValueWithin(GetExpression(() => decimal.MinValue)));
            Assert.Equal(decimal.MaxValue, ValueExtractor.FindValueWithin(GetExpression(() => decimal.MaxValue)));
            Assert.Equal(decimal.MinusOne, ValueExtractor.FindValueWithin(GetExpression(() => decimal.MinusOne)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(19)]
        [InlineData(3471)]
        [InlineData(1L)]
        [InlineData(5L)]
        [InlineData(52367L)]
        [InlineData("foo")]
        [InlineData("bar")]
        [InlineData(12f)]
        [InlineData(12.1d)]
        public void can_find_value_in_an_expression_that_returns_a_local_value(object value)
        {
            var local = value;
            Assert.Equal(value, ValueExtractor.FindValueWithin(GetExpression(() => local)));
        }

        [Theory]
        [InlineData(new int[] {})]
        [InlineData(new [] { 1, 2, 3 })]
        [InlineData(new [] { 1, 2, 3, 4, 5 })]
        [InlineData(new [] { 1d, 10d, 2937d })]
        public void can_find_value_in_an_expression_that_creates_an_array(object array)
        {
            Assert.Equal(array, ValueExtractor.FindValueWithin(GetExpression(() => array)));
        }

        [Fact]
        public void can_find_value_in_an_expression_that_news_up_an_object()
        {
            Assert.Equal(new DateTime(1979, 10, 26), ValueExtractor.FindValueWithin(GetExpression(() => new DateTime(1979, 10, 26))));
            Assert.Equal(Tuple.Create("one", 1), ValueExtractor.FindValueWithin(GetExpression(() => new Tuple<string, int>("one", 1))));
        }

        [Theory]
        [InlineData(1L)]
        [InlineData(2L)]
        [InlineData(1347L)]
        [InlineData(-23897L)]
        public void can_find_value_in_an_expression_that_requires_conversion(long value)
        {
            Assert.Equal((int)value, ValueExtractor.FindValueWithin(GetExpression(() => (int)value)));
        }

        [Fact]
        public void can_find_value_in_an_expression_that_includes_a_lambda()
        {
            Expression<Func<int, bool>> expression = x => x % 2 == 0;
            var value = ValueExtractor.FindValueWithin(expression);

            Assert.NotNull(value);
            var castValue = Assert.IsType<Func<int, bool>>(value);

            Assert.True(castValue(0));
            Assert.False(castValue(1));
            Assert.True(castValue(2));
            Assert.False(castValue(3));
        }

        [Fact]
        public void can_find_value_in_a_method_invocation()
        {
            Assert.Equal(Tuple.Create("one", 1), ValueExtractor.FindValueWithin(GetExpression(() => Tuple.Create("one", 1))));
            Assert.Equal(TimeSpan.FromSeconds(1), ValueExtractor.FindValueWithin(GetExpression(() => TimeSpan.FromSeconds(1))));
        }

        [Fact]
        public void can_find_value_in_a_binary_expression()
        {
            Assert.Equal(7, ValueExtractor.FindValueWithin(GetExpression(() => 3 + 1 + 3)));
            Assert.Equal(TimeSpan.FromSeconds(4), ValueExtractor.FindValueWithin(GetExpression(() => TimeSpan.FromSeconds(1) + TimeSpan.FromSeconds(3))));
        }

        [Fact]
        public void cannot_find_value_in_a_method_invocation_if_the_method_has_no_return_value()
        {
            object value;
            Assert.False(ValueExtractor.TryFindValueWithin(GetExpression(() => Console.WriteLine()), out value));
        }

        #region Supporting Members

        private static Expression GetExpression(Expression<Action> root)
        {
            return root.Body;
        }

        private static Expression GetExpression(Expression<Func<object>> root)
        {
            return root.Body;
        }

        #endregion
    }
}