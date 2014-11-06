namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsInFixture
    {
        [Fact]
        public void filter_returns_false_if_no_values_are_provided()
        {
            Assert.False(It.IsInFilter<string>().Matches("foo"));
            Assert.False(It.IsInFilter<string>().Matches(null));
        }

        [Fact]
        public void filter_returns_false_if_value_is_not_in_set_of_expected_values()
        {
            Assert.False(It.IsInFilter("hello", "world").Matches("Hello"));
            Assert.False(It.IsInFilter(1, 2, 3, 5, 8, 13).Matches(11));
            Assert.False(It.IsInFilter("hello", "world").Matches(null));
        }

        [Fact]
        public void filter_returns_true_if_value_is_in_set_of_expected_values()
        {
            Assert.True(It.IsInFilter("hello", "world").Matches("world"));
            Assert.True(It.IsInFilter(1, 2, 3, 5, 8, 13).Matches(5));
            Assert.True(It.IsInFilter("hello", null, "world").Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            // being that a HashSet is used as the underlying storage mechanism, the order of items isn't really predictable here
            Assert.Equal("Is in [\"hello\", \"world\"]", It.IsInFilter("hello", "world").ToString());
            Assert.Equal("Is in [3, 5, 10]", It.IsInFilter(3, 5, 10).ToString());
            Assert.Equal("Is in [15.182M, 2.812M]", It.IsInFilter(15.182m, 2.812M).ToString());
            Assert.Equal("Is in [\"foo\", null]", It.IsInFilter<string>("foo", null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsInFilter("foo").Equals(It.IsInFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_different_expected_values()
        {
            Assert.False(It.IsInFilter("foo").Equals(It.IsInFilter("bar")));
            Assert.False(It.IsInFilter("foo").Equals(It.IsInFilter("foo", "bar")));
            Assert.False(It.IsInFilter("foo").Equals(It.IsInFilter<string>(null)));
            Assert.False(It.IsInFilter<string>(null).Equals(It.IsInFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_expected_values()
        {
            Assert.True(It.IsInFilter("foo").Equals(It.IsInFilter("foo")));
            Assert.True(It.IsInFilter<string>(null).Equals(It.IsInFilter<string>(null)));
            Assert.True(It.IsInFilter("foo", "bar").Equals(It.IsInFilter("bar", "foo")));
            Assert.True(It.IsInFilter("foo", null, "bar").Equals(It.IsInFilter(null, "bar", "foo")));
            Assert.True(It.IsInFilter(1, 0, 150, -29).Equals(It.IsInFilter(150, -29, 0, 1)));
        }
    }
}