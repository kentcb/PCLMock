namespace PCLMock.UnitTests.Visitors
{
    using System;
    using System.Linq.Expressions;
    using PCLMock.ArgumentFilters;
    using PCLMock.Utility;
    using PCLMock.Visitors;
    using Xunit;

    public sealed class ArgumentFiltersVisitorFixture
    {
        [Fact]
        public void can_find_argument_filters_in_a_method_call()
        {
            var argumentFilters = ArgumentFiltersVisitor.FindArgumentFiltersWithin(GetExpression(() => Console.WriteLine(It.IsAny<string>(), 3, It.IsIn("foo", "bar"))));
            Assert.NotNull(argumentFilters);
            Assert.Equal(3, argumentFilters.Count);
            Assert.True(argumentFilters[0].Matches("foo"));
            Assert.True(argumentFilters[0].Matches("bar"));
            Assert.True(argumentFilters[0].Matches(null));
            Assert.True(argumentFilters[1].Matches(3));
            Assert.False(argumentFilters[1].Matches(2));
            Assert.True(argumentFilters[2].Matches("foo"));
            Assert.True(argumentFilters[2].Matches("bar"));
            Assert.False(argumentFilters[2].Matches("baz"));
            Assert.False(argumentFilters[2].Matches(3));
        }

        [Fact]
        public void out_and_ref_arguments_always_result_in_a_non_discriminatory_filter()
        {
            int i = 0;
            string s;
            var argumentFilters = ArgumentFiltersVisitor.FindArgumentFiltersWithin(GetExpression(() => this.SomeMethod(ref i, out s)));
            Assert.NotNull(argumentFilters);
            Assert.Equal(2, argumentFilters.Count);
            Assert.IsType<IsAnyArgumentFilter<object>>(argumentFilters[0]);
            Assert.IsType<IsAnyArgumentFilter<object>>(argumentFilters[1]);
        }

        [Fact]
        public void cannot_find_argument_filters_if_there_is_a_method_call_but_it_is_not_at_root_level()
        {
            ArgumentFilterCollection argumentFilters;
            Assert.False(ArgumentFiltersVisitor.TryFindArgumentFiltersWithin(GetExpression(() => new[] { Tuple.Create(1, 2) }), out argumentFilters));
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

        private void SomeMethod(ref int i, out string s)
        {
            s = null;
        }

        #endregion
    }
}
