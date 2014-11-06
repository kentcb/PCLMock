namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsFixture
    {
        [Fact]
        public void filter_works_with_null()
        {
            Assert.True(It.IsFilter<string>(null).Matches(null));
        }

        [Fact]
        public void filter_returns_false_if_values_do_not_match()
        {
            Assert.False(It.IsFilter("hello").Matches("world"));
            Assert.False(It.IsFilter("hello").Matches(null));
            Assert.False(It.IsFilter<string>(null).Matches("hello"));
            Assert.False(It.IsFilter(1).Matches(2));
        }

        [Fact]
        public void filter_returns_true_if_values_match()
        {
            Assert.True(It.IsFilter("hello").Matches("hello"));
            Assert.True(It.IsFilter(1).Matches(1));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is \"hello\"", It.IsFilter("hello").ToString());
            Assert.Equal("Is 10", It.IsFilter(10).ToString());
            Assert.Equal("Is 15.182M", It.IsFilter(15.182m).ToString());
            Assert.Equal("Is null", It.IsFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsFilter("foo").Equals(It.IsFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_expected_value()
        {
            Assert.False(It.IsFilter("foo").Equals(It.IsFilter("bar")));
            Assert.False(It.IsFilter("foo").Equals(It.IsFilter<string>(null)));
            Assert.False(It.IsFilter<string>(null).Equals(It.IsFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_expected_value()
        {
            Assert.True(It.IsFilter("foo").Equals(It.IsFilter("foo")));
            Assert.True(It.IsFilter(150).Equals(It.IsFilter(150)));
            Assert.True(It.IsFilter<string>(null).Equals(It.IsFilter<string>(null)));
        }
    }
}