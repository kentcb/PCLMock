namespace Kent.Boogaart.PCLMock.UnitTests.Visitors
{
    using System;
    using System.Linq.Expressions;
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Kent.Boogaart.PCLMock.Visitors;
    using Xunit;
    using Xunit.Extensions;

    public sealed class ArgumentFilterVisitorFixture
    {
        [Fact]
        public void can_find_argument_filter_in_method_call_against_it_class()
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => It.IsAny<string>()));
            Assert.NotNull(argumentFilter);
            Assert.True(argumentFilter.Matches("foo"));
            Assert.True(argumentFilter.Matches("bar"));
            Assert.True(argumentFilter.Matches(null));

            argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => It.Is("foo")));
            Assert.NotNull(argumentFilter);
            Assert.True(argumentFilter.Matches("foo"));
            Assert.False(argumentFilter.Matches("bar"));

            argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => It.IsIn(1, 2, 3)));
            Assert.NotNull(argumentFilter);
            Assert.True(argumentFilter.Matches(1));
            Assert.True(argumentFilter.Matches(3));
            Assert.False(argumentFilter.Matches(4));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(35)]
        [InlineData(3235)]
        [InlineData("foo")]
        [InlineData("bar")]
        [InlineData("baz")]
        [InlineData(1f)]
        [InlineData(13.1f)]
        [InlineData(1d)]
        [InlineData(13.1d)]
        public void a_constant_being_returned_is_translated_to_an_is_argument_filter(object value)
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => value));
            Assert.NotNull(argumentFilter);
            Assert.IsType<IsArgumentFilter>(argumentFilter);
            Assert.True(argumentFilter.Matches(value));
        }

        [Fact]
        public void expressions_are_evaluated_and_translated_to_an_is_argument_filter()
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => decimal.MinValue));
            Assert.NotNull(argumentFilter);
            Assert.IsType<IsArgumentFilter>(argumentFilter);
            Assert.True(argumentFilter.Matches(decimal.MinValue));

            argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => Tuple.Create(1, "one")));
            Assert.NotNull(argumentFilter);
            Assert.IsType<IsArgumentFilter>(argumentFilter);
            Assert.True(argumentFilter.Matches(Tuple.Create(1, "one")));
        }

        [Fact]
        public void cannot_find_argument_filter_in_method_call_with_no_arguments()
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin(GetExpression(() => Console.WriteLine()));
            Assert.Null(argumentFilter);
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
