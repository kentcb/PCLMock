namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;
    using Kent.Boogaart.PCLMock.ArgumentFilters;

    public sealed class IsArgumentFilterFixture
    {
        [Fact]
        public void matches_works_with_null()
        {
            Assert.True(new IsArgumentFilter(null).Matches(null));
        }

        [Fact]
        public void matches_returns_false_if_values_do_not_match()
        {
            Assert.False(new IsArgumentFilter("hello").Matches("world"));
            Assert.False(new IsArgumentFilter("hello").Matches(null));
            Assert.False(new IsArgumentFilter(null).Matches("hello"));
            Assert.False(new IsArgumentFilter(1).Matches(2));
        }

        [Fact]
        public void matches_returns_true_if_values_match()
        {
            Assert.True(new IsArgumentFilter("hello").Matches("hello"));
            Assert.True(new IsArgumentFilter(1).Matches(1));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("Is \"hello\"", new IsArgumentFilter("hello").ToString());
            Assert.Equal("Is 10", new IsArgumentFilter(10).ToString());
            Assert.Equal("Is 15.182M", new IsArgumentFilter(15.182m).ToString());
            Assert.Equal("Is null", new IsArgumentFilter(null).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(new IsArgumentFilter("foo").Equals(new IsArgumentFilter(10)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_expected_value()
        {
            Assert.False(new IsArgumentFilter("foo").Equals(new IsArgumentFilter("bar")));
            Assert.False(new IsArgumentFilter("foo").Equals(new IsArgumentFilter(null)));
            Assert.False(new IsArgumentFilter(null).Equals(new IsArgumentFilter("foo")));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_expected_value()
        {
            Assert.True(new IsArgumentFilter("foo").Equals(new IsArgumentFilter("foo")));
            Assert.True(new IsArgumentFilter(150).Equals(new IsArgumentFilter(150)));
            Assert.True(new IsArgumentFilter(null).Equals(new IsArgumentFilter(null)));
        }
    }
}