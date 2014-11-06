namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsGreaterThanOrEqualToFixture
    {
        [Fact]
        public void filter_returns_true_if_value_is_equal_to_specified_value()
        {
            Assert.True(It.IsGreaterThanOrEqualToFilter(5).Matches(5));
            Assert.True(It.IsGreaterThanOrEqualToFilter(5.152m).Matches(5.152m));
            Assert.True(It.IsGreaterThanOrEqualToFilter<string>(null).Matches(null));
        }

        [Fact]
        public void filter_returns_true_if_value_is_greater_than_specified_value()
        {
            Assert.True(It.IsGreaterThanOrEqualToFilter(5).Matches(6));
            Assert.True(It.IsGreaterThanOrEqualToFilter(5.152m).Matches(5.153m));
            Assert.True(It.IsGreaterThanOrEqualToFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void filter_returns_false_if_value_is_less_than_specified_value()
        {
            Assert.False(It.IsGreaterThanOrEqualToFilter(5).Matches(4));
            Assert.False(It.IsGreaterThanOrEqualToFilter(5.142m).Matches(5.141m));
            Assert.False(It.IsGreaterThanOrEqualToFilter("foo").Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is greater than or equal to 10", It.IsGreaterThanOrEqualToFilter(10).ToString());
            Assert.Equal("Is greater than or equal to 15.182M", It.IsGreaterThanOrEqualToFilter(15.182m).ToString());
            Assert.Equal("Is greater than or equal to null", It.IsGreaterThanOrEqualToFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsGreaterThanOrEqualToFilter("foo").Equals(It.IsGreaterThanOrEqualToFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_specified_value()
        {
            Assert.False(It.IsGreaterThanOrEqualToFilter("foo").Equals(It.IsGreaterThanOrEqualToFilter("bar")));
            Assert.False(It.IsGreaterThanOrEqualToFilter("foo").Equals(It.IsGreaterThanOrEqualToFilter<string>(null)));
            Assert.False(It.IsGreaterThanOrEqualToFilter<string>(null).Equals(It.IsGreaterThanOrEqualToFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_specified_value()
        {
            Assert.True(It.IsGreaterThanOrEqualToFilter("foo").Equals(It.IsGreaterThanOrEqualToFilter("foo")));
            Assert.True(It.IsGreaterThanOrEqualToFilter(150).Equals(It.IsGreaterThanOrEqualToFilter(150)));
            Assert.True(It.IsGreaterThanOrEqualToFilter<string>(null).Equals(It.IsGreaterThanOrEqualToFilter<string>(null)));
        }
    }
}