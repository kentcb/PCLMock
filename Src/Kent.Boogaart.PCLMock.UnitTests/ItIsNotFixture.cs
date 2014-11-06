namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsNotFixture
    {
        [Fact]
        public void filter_works_with_null()
        {
            Assert.True(It.IsNotFilter<string>(null).Matches("foo"));
        }

        [Fact]
        public void filter_returns_true_if_values_do_not_match()
        {
            Assert.True(It.IsNotFilter("hello").Matches("world"));
            Assert.True(It.IsNotFilter("hello").Matches(null));
            Assert.True(It.IsNotFilter<string>(null).Matches("hello"));
            Assert.True(It.IsNotFilter(1).Matches(2));
        }

        [Fact]
        public void filter_returns_false_if_values_match()
        {
            Assert.False(It.IsNotFilter("hello").Matches("hello"));
            Assert.False(It.IsNotFilter(1).Matches(1));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is not \"hello\"", It.IsNotFilter("hello").ToString());
            Assert.Equal("Is not 10", It.IsNotFilter(10).ToString());
            Assert.Equal("Is not 15.182M", It.IsNotFilter(15.182m).ToString());
            Assert.Equal("Is not null", It.IsNotFilter<string>(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsNotFilter("foo").Equals(It.IsNotFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_expected_value()
        {
            Assert.False(It.IsNotFilter("foo").Equals(It.IsNotFilter("bar")));
            Assert.False(It.IsFilter("foo").Equals(It.IsFilter<string>(null)));
            Assert.False(It.IsFilter<string>(null).Equals(It.IsFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_expected_value()
        {
            Assert.True(It.IsNotFilter("foo").Equals(It.IsNotFilter("foo")));
            Assert.True(It.IsNotFilter(150).Equals(It.IsNotFilter(150)));
            Assert.True(It.IsNotFilter<string>(null).Equals(It.IsNotFilter<string>(null)));
        }
    }
}