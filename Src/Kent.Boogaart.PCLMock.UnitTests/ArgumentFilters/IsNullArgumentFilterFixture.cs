namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using System.Text;
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Xunit;

    public sealed class IsNullArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_false_if_value_is_not_null()
        {
            Assert.False(IsNullArgumentFilter<string>.Instance.Matches("hello"));
            Assert.False(IsNullArgumentFilter<string>.Instance.Matches("world"));
        }

        [Fact]
        public void matches_returns_true_if_value_is_null()
        {
            Assert.True(IsNullArgumentFilter<string>.Instance.Matches(null));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("Is null System.String", IsNullArgumentFilter<string>.Instance.ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(IsNullArgumentFilter<string>.Instance.Equals(IsNullArgumentFilter<StringBuilder>.Instance));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_for_the_same_type()
        {
            Assert.True(IsNullArgumentFilter<string>.Instance.Equals(IsNullArgumentFilter<string>.Instance));
            Assert.True(IsNullArgumentFilter<StringBuilder>.Instance.Equals(IsNullArgumentFilter<StringBuilder>.Instance));
        }
    }
}