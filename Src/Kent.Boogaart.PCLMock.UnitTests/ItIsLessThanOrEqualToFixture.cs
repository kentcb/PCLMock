namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsLessThanOrEqualToFixture
    {
        [Fact]
        public void filter_returns_true_if_value_is_equal_to_specified_value()
        {
            Assert.True(It.IsLessThanOrEqualToFilter(5).Matches(5));
            Assert.True(It.IsLessThanOrEqualToFilter(5.152m).Matches(5.152m));
            Assert.True(It.IsLessThanOrEqualToFilter<string>(null).Matches(null));
        }

        [Fact]
        public void filter_returns_false_if_value_is_greater_than_specified_value()
        {
            Assert.False(It.IsLessThanOrEqualToFilter(5).Matches(6));
            Assert.False(It.IsLessThanOrEqualToFilter(5.152m).Matches(5.153m));
            Assert.False(It.IsLessThanOrEqualToFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void filter_returns_true_if_value_is_less_than_specified_value()
        {
            Assert.True(It.IsLessThanOrEqualToFilter(5).Matches(4));
            Assert.True(It.IsLessThanOrEqualToFilter(5.142m).Matches(5.141m));
            Assert.True(It.IsLessThanOrEqualToFilter("foo").Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is less than or equal to 10", It.IsLessThanOrEqualToFilter(10).ToString());
            Assert.Equal("Is less than or equal to 15.182M", It.IsLessThanOrEqualToFilter(15.182m).ToString());
            Assert.Equal("Is less than or equal to null", It.IsLessThanOrEqualToFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsLessThanOrEqualToFilter("foo").Equals(It.IsLessThanOrEqualToFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_specified_value()
        {
            Assert.False(It.IsLessThanOrEqualToFilter("foo").Equals(It.IsLessThanOrEqualToFilter("bar")));
            Assert.False(It.IsLessThanOrEqualToFilter("foo").Equals(It.IsLessThanOrEqualToFilter<string>(null)));
            Assert.False(It.IsLessThanOrEqualToFilter<string>(null).Equals(It.IsLessThanOrEqualToFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_specified_value()
        {
            Assert.True(It.IsLessThanOrEqualToFilter("foo").Equals(It.IsLessThanOrEqualToFilter("foo")));
            Assert.True(It.IsLessThanOrEqualToFilter(150).Equals(It.IsLessThanOrEqualToFilter(150)));
            Assert.True(It.IsLessThanOrEqualToFilter<string>(null).Equals(It.IsLessThanOrEqualToFilter<string>(null)));
        }
    }
}