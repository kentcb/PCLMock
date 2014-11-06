namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsLessThanFixture
    {
        [Fact]
        public void filter_returns_false_if_value_is_equal_to_specified_value()
        {
            Assert.False(It.IsLessThanFilter(5).Matches(5));
            Assert.False(It.IsLessThanFilter(5.152m).Matches(5.152m));
            Assert.False(It.IsLessThanFilter<string>(null).Matches(null));
        }

        [Fact]
        public void filter_returns_false_if_value_is_greater_than_specified_value()
        {
            Assert.False(It.IsLessThanFilter(5).Matches(6));
            Assert.False(It.IsLessThanFilter(5.152m).Matches(5.153m));
            Assert.False(It.IsLessThanFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void filter_returns_true_if_value_is_less_than_specified_value()
        {
            Assert.True(It.IsLessThanFilter(5).Matches(4));
            Assert.True(It.IsLessThanFilter(5.142m).Matches(5.141m));
            Assert.True(It.IsLessThanFilter("foo").Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is less than 10", It.IsLessThanFilter(10).ToString());
            Assert.Equal("Is less than 15.182M", It.IsLessThanFilter(15.182m).ToString());
            Assert.Equal("Is less than null", It.IsLessThanFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsLessThanFilter("foo").Equals(It.IsLessThanFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_specified_value()
        {
            Assert.False(It.IsLessThanFilter("foo").Equals(It.IsLessThanFilter("bar")));
            Assert.False(It.IsLessThanFilter("foo").Equals(It.IsLessThanFilter<string>(null)));
            Assert.False(It.IsLessThanFilter<string>(null).Equals(It.IsLessThanFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_specified_value()
        {
            Assert.True(It.IsLessThanFilter("foo").Equals(It.IsLessThanFilter("foo")));
            Assert.True(It.IsLessThanFilter(150).Equals(It.IsLessThanFilter(150)));
            Assert.True(It.IsLessThanFilter<string>(null).Equals(It.IsLessThanFilter<string>(null)));
        }
    }
}