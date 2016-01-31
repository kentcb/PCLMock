namespace PCLMock.ArgumentFilters.UnitTests
{
    using PCLMock.ArgumentFilters;
    using Xunit;
    using Xunit.Extensions;

    public sealed class IsArgumentFilterFixture
    {
        [Fact]
        public void matches_works_with_null()
        {
            Assert.True(new IsArgumentFilter(null).Matches(null));
        }

        [Theory]
        [InlineData("hello", "world", false)]
        [InlineData("hello", null, false)]
        [InlineData(null, "hello", false)]
        [InlineData(1, 2, false)]
        [InlineData(1, 1f, false)]
        [InlineData("hello", "hello", true)]
        [InlineData(null, null, true)]
        [InlineData(13, 13, true)]
        [InlineData(56f, 56f, true)]
        public void matches_returns_correct_value(object firstValue, object secondValue, bool expectedResult)
        {
            Assert.Equal(expectedResult, new IsArgumentFilter(firstValue).Matches(secondValue));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.Is(\"hello\")", new IsArgumentFilter("hello").ToString());
            Assert.Equal("It.Is(10)", new IsArgumentFilter(10).ToString());
            Assert.Equal("It.Is(15.182M)", new IsArgumentFilter(15.182m).ToString());
            Assert.Equal("It.Is(null)", new IsArgumentFilter(null).ToString());
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