namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsNotBetweenFixture
    {
        [Fact]
        public void filter_returns_false_if_value_is_equal_to_the_minimum()
        {
            Assert.False(It.IsNotBetweenFilter(5, 10).Matches(5));
            Assert.False(It.IsNotBetweenFilter(5.152m, 10.3m).Matches(5.152m));
            Assert.False(It.IsNotBetweenFilter<string>(null, "abc").Matches(null));
        }

        [Fact]
        public void filter_returns_false_if_value_is_equal_to_the_maximum()
        {
            Assert.False(It.IsNotBetweenFilter(5, 10).Matches(10));
            Assert.False(It.IsNotBetweenFilter(5.152m, 10.3m).Matches(10.3m));
            Assert.False(It.IsNotBetweenFilter<string>(null, "abc").Matches("abc"));
        }

        [Fact]
        public void filter_returns_true_if_value_is_less_than_the_minimum()
        {
            Assert.True(It.IsNotBetweenFilter(5, 10).Matches(4));
            Assert.True(It.IsNotBetweenFilter(5.152m, 10.3m).Matches(5.151m));
        }

        [Fact]
        public void filter_returns_true_if_value_is_greater_than_the_maximum()
        {
            Assert.True(It.IsNotBetweenFilter(5, 10).Matches(11));
            Assert.True(It.IsNotBetweenFilter(5.152m, 10.3m).Matches(10.301m));
        }

        [Fact]
        public void filter_returns_false_if_value_is_within_the_minimum_and_maximum()
        {
            Assert.False(It.IsNotBetweenFilter(5, 10).Matches(6));
            Assert.False(It.IsNotBetweenFilter(5.152m, 10.3m).Matches(6.1m));
            Assert.False(It.IsNotBetweenFilter<string>(null, "abc").Matches("a"));
            Assert.False(It.IsNotBetweenFilter(5, 5).Matches(5));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is not between 5 and 10", It.IsNotBetweenFilter(5, 10).ToString());
            Assert.Equal("Is not between 12.36M and 15.182M", It.IsNotBetweenFilter(12.36m, 15.182m).ToString());
            Assert.Equal("Is not between null and \"abc\"", It.IsNotBetweenFilter<string>(null, "abc").ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsNotBetweenFilter(null, "foo").Equals(It.IsNotBetweenFilter(1, 10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_range()
        {
            Assert.False(It.IsNotBetweenFilter(5, 10).Equals(It.IsNotBetweenFilter(6, 10)));
            Assert.False(It.IsNotBetweenFilter(5.2387m, 10.127m).Equals(It.IsNotBetweenFilter(5.2387m, 10.1271m)));
            Assert.False(It.IsNotBetweenFilter("foo", "bar").Equals(It.IsNotBetweenFilter("foo", null)));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_range()
        {
            Assert.True(It.IsNotBetweenFilter(5, 10).Equals(It.IsNotBetweenFilter(5, 10)));
            Assert.True(It.IsNotBetweenFilter(5.2387m, 10.127m).Equals(It.IsNotBetweenFilter(5.2387m, 10.127m)));
            Assert.True(It.IsNotBetweenFilter("foo", "bar").Equals(It.IsNotBetweenFilter("foo", "bar")));
        }

        [Fact]
        public void ranges_are_automatically_inverted_as_necessary()
        {
            Assert.True(It.IsNotBetweenFilter(5, 10).Equals(It.IsNotBetweenFilter(10, 5)));
            Assert.True(It.IsNotBetweenFilter(5.2387m, 10.127m).Equals(It.IsNotBetweenFilter(10.127m, 5.2387m)));
            Assert.True(It.IsNotBetweenFilter("foo", "bar").Equals(It.IsNotBetweenFilter("bar", "foo")));
        }
    }
}