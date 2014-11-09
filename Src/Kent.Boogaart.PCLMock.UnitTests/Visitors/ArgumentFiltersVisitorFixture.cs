namespace Kent.Boogaart.PCLMock.UnitTests.Visitors
{
    using Kent.Boogaart.PCLMock.Visitors;
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public sealed class ArgumentFiltersVisitorFixture
    {
        [Fact]
        public void can_find_argument_filters_in_a_method_call()
        {
            var argumentFilters = ArgumentFiltersVisitor.FindArgumentFiltersWithin((Expression<Action>)(() => Console.WriteLine(It.IsAny<string>(), 3, It.IsIn("foo", "bar"))));
            Assert.NotNull(argumentFilters);
            Assert.Equal(3, argumentFilters.Length);
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
            var argumentFilters = ArgumentFiltersVisitor.FindArgumentFiltersWithin((Expression<Action>)(() => this.SomeMethod(ref i, out s)));
            Assert.NotNull(argumentFilters);
            Assert.Equal(2, argumentFilters.Length);
            Assert.IsType<It.IsAnyArgumentFilter<object>>(argumentFilters[0]);
            Assert.IsType<It.IsAnyArgumentFilter<object>>(argumentFilters[1]);
        }

        #region Supporting Members

        private void SomeMethod(ref int i, out string s)
        {
            s = null;
        }

        #endregion
    }
}
