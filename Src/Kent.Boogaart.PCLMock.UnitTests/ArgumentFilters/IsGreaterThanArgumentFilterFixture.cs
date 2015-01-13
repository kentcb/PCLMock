namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Xunit;

    public sealed class IsGreaterThanArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_false_if_value_is_equal_to_specified_value()
        {
            Assert.False(new IsGreaterThanArgumentFilter<int>(5).Matches(5));
            Assert.False(new IsGreaterThanArgumentFilter<decimal>(5.152m).Matches(5.152m));
            Assert.False(new IsGreaterThanArgumentFilter<string>(null).Matches(null));
        }

        [Fact]
        public void matches_returns_true_if_value_is_greater_than_specified_value()
        {
            Assert.True(new IsGreaterThanArgumentFilter<int>(5).Matches(6));
            Assert.True(new IsGreaterThanArgumentFilter<decimal>(5.152m).Matches(5.153m));
            Assert.True(new IsGreaterThanArgumentFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void matches_returns_false_if_value_is_less_than_specified_value()
        {
            Assert.False(new IsGreaterThanArgumentFilter<int>(5).Matches(4));
            Assert.False(new IsGreaterThanArgumentFilter<decimal>(5.142m).Matches(5.141m));
            Assert.False(new IsGreaterThanArgumentFilter<string>("foo").Matches(null));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.IsGreaterThan(10)", new IsGreaterThanArgumentFilter<int>(10).ToString());
            Assert.Equal("It.IsGreaterThan(15.182M)", new IsGreaterThanArgumentFilter<decimal>(15.182m).ToString());
            Assert.Equal("It.IsGreaterThan(null)", new IsGreaterThanArgumentFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(new IsGreaterThanArgumentFilter<string>("foo").Equals(new IsGreaterThanArgumentFilter<int>(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_specified_value()
        {
            Assert.False(new IsGreaterThanArgumentFilter<string>("foo").Equals(new IsGreaterThanArgumentFilter<string>("bar")));
            Assert.False(new IsGreaterThanArgumentFilter<string>("foo").Equals(new IsGreaterThanArgumentFilter<string>(null)));
            Assert.False(new IsGreaterThanArgumentFilter<string>(null).Equals(new IsGreaterThanArgumentFilter<string>("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_specified_value()
        {
            Assert.True(new IsGreaterThanArgumentFilter<string>("foo").Equals(new IsGreaterThanArgumentFilter<string>("foo")));
            Assert.True(new IsGreaterThanArgumentFilter<int>(150).Equals(new IsGreaterThanArgumentFilter<int>(150)));
            Assert.True(new IsGreaterThanArgumentFilter<string>(null).Equals(new IsGreaterThanArgumentFilter<string>(null)));
        }
    }
}