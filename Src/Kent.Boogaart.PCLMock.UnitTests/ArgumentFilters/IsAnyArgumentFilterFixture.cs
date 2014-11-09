namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;
    using Kent.Boogaart.PCLMock.ArgumentFilters;

    public sealed class IsAnyArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_true_for_null()
        {
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(null));
        }

        [Fact]
        public void matches_returns_true_for_any_value()
        {
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches("hello"));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches("world"));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(""));

            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(0));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(1));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(-1));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(int.MaxValue));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(int.MinValue));
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(38));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("Is any System.String", IsAnyArgumentFilter<string>.Instance.ToString());
            Assert.Equal("Is any System.Int32", IsAnyArgumentFilter<int>.Instance.ToString());
            Assert.Equal("Is any System.Nullable`1[System.Int32]", IsAnyArgumentFilter<int?>.Instance.ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_null()
        {
            Assert.False(IsAnyArgumentFilter<string>.Instance.Equals(null));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(IsAnyArgumentFilter<string>.Instance.Equals(IsAnyArgumentFilter<int>.Instance));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_of_the_same_type()
        {
            Assert.True(IsAnyArgumentFilter<string>.Instance.Equals(IsAnyArgumentFilter<string>.Instance));
        }
    }
}