namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsBetweenFixture
    {
        [Fact]
        public void filter_returns_true_if_value_is_equal_to_the_minimum()
        {
            Assert.True(It.IsBetweenFilter(5, 10).Matches(5));
            Assert.True(It.IsBetweenFilter(5.152m, 10.3m).Matches(5.152m));
            Assert.True(It.IsBetweenFilter<string>(null, "abc").Matches(null));
        }

        [Fact]
        public void filter_returns_true_if_value_is_equal_to_the_maximum()
        {
            Assert.True(It.IsBetweenFilter(5, 10).Matches(10));
            Assert.True(It.IsBetweenFilter(5.152m, 10.3m).Matches(10.3m));
            Assert.True(It.IsBetweenFilter<string>(null, "abc").Matches("abc"));
        }

        [Fact]
        public void filter_returns_false_if_value_is_less_than_the_minimum()
        {
            Assert.False(It.IsBetweenFilter(5, 10).Matches(4));
            Assert.False(It.IsBetweenFilter(5.152m, 10.3m).Matches(5.151m));
        }

        [Fact]
        public void filter_returns_false_if_value_is_greater_than_the_maximum()
        {
            Assert.False(It.IsBetweenFilter(5, 10).Matches(11));
            Assert.False(It.IsBetweenFilter(5.152m, 10.3m).Matches(10.301m));
        }

        [Fact]
        public void filter_returns_true_if_value_is_within_the_minimum_and_maximum()
        {
            Assert.True(It.IsBetweenFilter(5, 10).Matches(6));
            Assert.True(It.IsBetweenFilter(5.152m, 10.3m).Matches(6.1m));
            Assert.True(It.IsBetweenFilter<string>(null, "abc").Matches("a"));
            Assert.True(It.IsBetweenFilter(5, 5).Matches(5));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is between 5 and 10", It.IsBetweenFilter(5, 10).ToString());
            Assert.Equal("Is between 12.36M and 15.182M", It.IsBetweenFilter(12.36m, 15.182m).ToString());
            Assert.Equal("Is between null and \"abc\"", It.IsBetweenFilter<string>(null, "abc").ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsBetweenFilter(null, "foo").Equals(It.IsBetweenFilter(1, 10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_range()
        {
            Assert.False(It.IsBetweenFilter(5, 10).Equals(It.IsBetweenFilter(6, 10)));
            Assert.False(It.IsBetweenFilter(5.2387m, 10.127m).Equals(It.IsBetweenFilter(5.2387m, 10.1271m)));
            Assert.False(It.IsBetweenFilter("foo", "bar").Equals(It.IsBetweenFilter("foo", null)));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_range()
        {
            Assert.True(It.IsBetweenFilter(5, 10).Equals(It.IsBetweenFilter(5, 10)));
            Assert.True(It.IsBetweenFilter(5.2387m, 10.127m).Equals(It.IsBetweenFilter(5.2387m, 10.127m)));
            Assert.True(It.IsBetweenFilter("foo", "bar").Equals(It.IsBetweenFilter("foo", "bar")));
        }

        [Fact]
        public void ranges_are_automatically_inverted_as_necessary()
        {
            Assert.True(It.IsBetweenFilter(5, 10).Equals(It.IsBetweenFilter(10, 5)));
            Assert.True(It.IsBetweenFilter(5.2387m, 10.127m).Equals(It.IsBetweenFilter(10.127m, 5.2387m)));
            Assert.True(It.IsBetweenFilter("foo", "bar").Equals(It.IsBetweenFilter("bar", "foo")));
        }
    }
}