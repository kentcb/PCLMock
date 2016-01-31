namespace PCLMock.ArgumentFilters.UnitTests
{
    using PCLMock.ArgumentFilters;
    using Xunit;
    using Xunit.Extensions;

    public sealed class IsAnyArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_true_for_null()
        {
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("hello")]
        [InlineData("world")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(38)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void matches_returns_true_for_any_value(object value)
        {
            Assert.True(IsAnyArgumentFilter<string>.Instance.Matches(value));
            Assert.True(IsAnyArgumentFilter<int>.Instance.Matches(value));
            Assert.True(IsAnyArgumentFilter<int?>.Instance.Matches(value));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.IsAny<string>()", IsAnyArgumentFilter<string>.Instance.ToString());
            Assert.Equal("It.IsAny<int>()", IsAnyArgumentFilter<int>.Instance.ToString());
            Assert.Equal("It.IsAny<int?>()", IsAnyArgumentFilter<int?>.Instance.ToString());
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