namespace PCLMock.ArgumentFilters.UnitTests
{
    using PCLMock.ArgumentFilters;
    using Xunit;

    public sealed class IsInArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_false_if_no_values_are_provided()
        {
            Assert.False(new IsInArgumentFilter<string>().Matches("foo"));
            Assert.False(new IsInArgumentFilter<string>().Matches(null));
        }

        [Fact]
        public void matches_returns_false_if_value_is_not_in_set_of_expected_values()
        {
            Assert.False(new IsInArgumentFilter<string>("hello", "world").Matches("Hello"));
            Assert.False(new IsInArgumentFilter<int>(1, 2, 3, 5, 8, 13).Matches(11));
            Assert.False(new IsInArgumentFilter<string>("hello", "world").Matches(null));
        }

        [Fact]
        public void matches_returns_true_if_value_is_in_set_of_expected_values()
        {
            Assert.True(new IsInArgumentFilter<string>("hello", "world").Matches("world"));
            Assert.True(new IsInArgumentFilter<int>(1, 2, 3, 5, 8, 13).Matches(5));
            Assert.True(new IsInArgumentFilter<string>("hello", null, "world").Matches(null));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            // being that a HashSet is used as the underlying storage mechanism, the order of items isn't really predictable here
            Assert.Equal("It.IsIn(\"hello\", \"world\")", new IsInArgumentFilter<string>("hello", "world").ToString());
            Assert.Equal("It.IsIn(3, 5, 10)", new IsInArgumentFilter<int>(3, 5, 10).ToString());
            Assert.Equal("It.IsIn(15.182M, 2.812M)", new IsInArgumentFilter<decimal>(15.182m, 2.812M).ToString());
            Assert.Equal("It.IsIn(\"foo\", null)", new IsInArgumentFilter<string>("foo", null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(new IsInArgumentFilter<string>("foo").Equals(new IsInArgumentFilter<int>(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_different_expected_values()
        {
            Assert.False(new IsInArgumentFilter<string>("foo").Equals(new IsInArgumentFilter<string>("bar")));
            Assert.False(new IsInArgumentFilter<string>("foo").Equals(new IsInArgumentFilter<string>("foo", "bar")));
            Assert.False(new IsInArgumentFilter<string>("foo").Equals(new IsInArgumentFilter<string>(null)));
            Assert.False(new IsInArgumentFilter<string>(null).Equals(new IsInArgumentFilter<string>("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_expected_values()
        {
            Assert.True(new IsInArgumentFilter<string>("foo").Equals(new IsInArgumentFilter<string>("foo")));
            Assert.True(new IsInArgumentFilter<string>(null).Equals(new IsInArgumentFilter<string>(null)));
            Assert.True(new IsInArgumentFilter<string>("foo", "bar").Equals(new IsInArgumentFilter<string>("bar", "foo")));
            Assert.True(new IsInArgumentFilter<string>("foo", null, "bar").Equals(new IsInArgumentFilter<string>(null, "bar", "foo")));
            Assert.True(new IsInArgumentFilter<int>(1, 0, 150, -29).Equals(new IsInArgumentFilter<int>(150, -29, 0, 1)));
        }
    }
}