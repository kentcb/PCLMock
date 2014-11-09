namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;
    using Kent.Boogaart.PCLMock.ArgumentFilters;

    public sealed class IsGreaterThanOrEqualToArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_true_if_value_is_equal_to_specified_value()
        {
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<int>(5).Matches(5));
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<decimal>(5.152m).Matches(5.152m));
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<string>(null).Matches(null));
        }

        [Fact]
        public void matches_returns_true_if_value_is_greater_than_specified_value()
        {
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<int>(5).Matches(6));
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<decimal>(5.152m).Matches(5.153m));
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void matches_returns_false_if_value_is_less_than_specified_value()
        {
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<int>(5).Matches(4));
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<decimal>(5.142m).Matches(5.141m));
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<string>("foo").Matches(null));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("Is greater than or equal to 10", new IsGreaterThanOrEqualToArgumentFilter<int>(10).ToString());
            Assert.Equal("Is greater than or equal to 15.182M", new IsGreaterThanOrEqualToArgumentFilter<decimal>(15.182m).ToString());
            Assert.Equal("Is greater than or equal to null", new IsGreaterThanOrEqualToArgumentFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<string>("foo").Equals(new IsGreaterThanOrEqualToArgumentFilter<int>(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_specified_value()
        {
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<string>("foo").Equals(new IsGreaterThanOrEqualToArgumentFilter<string>("bar")));
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<string>("foo").Equals(new IsGreaterThanOrEqualToArgumentFilter<string>(null)));
            Assert.False(new IsGreaterThanOrEqualToArgumentFilter<string>(null).Equals(new IsGreaterThanOrEqualToArgumentFilter<string>("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_specified_value()
        {
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<string>("foo").Equals(new IsGreaterThanOrEqualToArgumentFilter<string>("foo")));
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<int>(150).Equals(new IsGreaterThanOrEqualToArgumentFilter<int>(150)));
            Assert.True(new IsGreaterThanOrEqualToArgumentFilter<string>(null).Equals(new IsGreaterThanOrEqualToArgumentFilter<string>(null)));
        }
    }
}