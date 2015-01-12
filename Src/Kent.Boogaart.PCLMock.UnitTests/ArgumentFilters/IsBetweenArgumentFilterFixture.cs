namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;
    using Kent.Boogaart.PCLMock.ArgumentFilters;

    public sealed class IsBetweenArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_true_if_value_is_equal_to_the_minimum()
        {
            Assert.True(new IsBetweenArgumentFilter<int>(5, 10).Matches(5));
            Assert.True(new IsBetweenArgumentFilter<decimal>(5.152m, 10.3m).Matches(5.152m));
            Assert.True(new IsBetweenArgumentFilter<string>(null, "abc").Matches(null));
        }

        [Fact]
        public void matches_returns_true_if_value_is_equal_to_the_maximum()
        {
            Assert.True(new IsBetweenArgumentFilter<int>(5, 10).Matches(10));
            Assert.True(new IsBetweenArgumentFilter<decimal>(5.152m, 10.3m).Matches(10.3m));
            Assert.True(new IsBetweenArgumentFilter<string>(null, "abc").Matches("abc"));
        }

        [Fact]
        public void matches_returns_false_if_value_is_less_than_the_minimum()
        {
            Assert.False(new IsBetweenArgumentFilter<int>(5, 10).Matches(4));
            Assert.False(new IsBetweenArgumentFilter<decimal>(5.152m, 10.3m).Matches(5.151m));
        }

        [Fact]
        public void matches_returns_false_if_value_is_greater_than_the_maximum()
        {
            Assert.False(new IsBetweenArgumentFilter<int>(5, 10).Matches(11));
            Assert.False(new IsBetweenArgumentFilter<decimal>(5.152m, 10.3m).Matches(10.301m));
        }

        [Fact]
        public void matches_returns_true_if_value_is_within_the_minimum_and_maximum()
        {
            Assert.True(new IsBetweenArgumentFilter<int>(5, 10).Matches(6));
            Assert.True(new IsBetweenArgumentFilter<decimal>(5.152m, 10.3m).Matches(6.1m));
            Assert.True(new IsBetweenArgumentFilter<string>(null, "abc").Matches("a"));
            Assert.True(new IsBetweenArgumentFilter<int>(5, 5).Matches(5));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.IsBetween(5, 10)", new IsBetweenArgumentFilter<int>(5, 10).ToString());
            Assert.Equal("It.IsBetween(12.36M, 15.182M)", new IsBetweenArgumentFilter<decimal>(12.36m, 15.182m).ToString());
            Assert.Equal("It.IsBetween(null, \"abc\")", new IsBetweenArgumentFilter<string>(null, "abc").ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(new IsBetweenArgumentFilter<string>(null, "foo").Equals(new IsBetweenArgumentFilter<int>(1, 10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_range()
        {
            Assert.False(new IsBetweenArgumentFilter<int>(5, 10).Equals(new IsBetweenArgumentFilter<int>(6, 10)));
            Assert.False(new IsBetweenArgumentFilter<decimal>(5.2387m, 10.127m).Equals(new IsBetweenArgumentFilter<decimal>(5.2387m, 10.1271m)));
            Assert.False(new IsBetweenArgumentFilter<string>("foo", "bar").Equals(new IsBetweenArgumentFilter<string>("foo", null)));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_range()
        {
            Assert.True(new IsBetweenArgumentFilter<int>(5, 10).Equals(new IsBetweenArgumentFilter<int>(5, 10)));
            Assert.True(new IsBetweenArgumentFilter<decimal>(5.2387m, 10.127m).Equals(new IsBetweenArgumentFilter<decimal>(5.2387m, 10.127m)));
            Assert.True(new IsBetweenArgumentFilter<string>("foo", "bar").Equals(new IsBetweenArgumentFilter<string>("foo", "bar")));
        }

        [Fact]
        public void ranges_are_automatically_inverted_as_necessary()
        {
            Assert.True(new IsBetweenArgumentFilter<int>(5, 10).Equals(new IsBetweenArgumentFilter<int>(10, 5)));
            Assert.True(new IsBetweenArgumentFilter<decimal>(5.2387m, 10.127m).Equals(new IsBetweenArgumentFilter<decimal>(10.127m, 5.2387m)));
            Assert.True(new IsBetweenArgumentFilter<string>("foo", "bar").Equals(new IsBetweenArgumentFilter<string>("bar", "foo")));
        }
    }
}