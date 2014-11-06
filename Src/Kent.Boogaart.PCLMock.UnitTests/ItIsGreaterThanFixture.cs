namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsGreaterThanFixture
    {
        [Fact]
        public void filter_returns_false_if_value_is_equal_to_specified_value()
        {
            Assert.False(It.IsGreaterThanFilter(5).Matches(5));
            Assert.False(It.IsGreaterThanFilter(5.152m).Matches(5.152m));
            Assert.False(It.IsGreaterThanFilter<string>(null).Matches(null));
        }

        [Fact]
        public void filter_returns_true_if_value_is_greater_than_specified_value()
        {
            Assert.True(It.IsGreaterThanFilter(5).Matches(6));
            Assert.True(It.IsGreaterThanFilter(5.152m).Matches(5.153m));
            Assert.True(It.IsGreaterThanFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void filter_returns_false_if_value_is_less_than_specified_value()
        {
            Assert.False(It.IsGreaterThanFilter(5).Matches(4));
            Assert.False(It.IsGreaterThanFilter(5.142m).Matches(5.141m));
            Assert.False(It.IsGreaterThanFilter("foo").Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is greater than 10", It.IsGreaterThanFilter(10).ToString());
            Assert.Equal("Is greater than 15.182M", It.IsGreaterThanFilter(15.182m).ToString());
            Assert.Equal("Is greater than null", It.IsGreaterThanFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsGreaterThanFilter("foo").Equals(It.IsGreaterThanFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_specified_value()
        {
            Assert.False(It.IsGreaterThanFilter("foo").Equals(It.IsGreaterThanFilter("bar")));
            Assert.False(It.IsGreaterThanFilter("foo").Equals(It.IsGreaterThanFilter<string>(null)));
            Assert.False(It.IsGreaterThanFilter<string>(null).Equals(It.IsGreaterThanFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_specified_value()
        {
            Assert.True(It.IsGreaterThanFilter("foo").Equals(It.IsGreaterThanFilter("foo")));
            Assert.True(It.IsGreaterThanFilter(150).Equals(It.IsGreaterThanFilter(150)));
            Assert.True(It.IsGreaterThanFilter<string>(null).Equals(It.IsGreaterThanFilter<string>(null)));
        }
    }
}