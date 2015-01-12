namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using System;
    using System.Text.RegularExpressions;
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Xunit;

    public sealed class IsLikeArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_false_if_values_do_not_match()
        {
            Assert.False(new IsLikeArgumentFilter("[hH]ello", RegexOptions.None).Matches("world"));
            Assert.False(new IsLikeArgumentFilter("hello", RegexOptions.None).Matches(null));
        }

        [Fact]
        public void matches_returns_true_if_values_match()
        {
            Assert.True(new IsLikeArgumentFilter("[hH]ello", RegexOptions.None).Matches("Hello"));
            Assert.True(new IsLikeArgumentFilter("[hH]ello", RegexOptions.None).Matches("hello"));
            Assert.True(new IsLikeArgumentFilter("hello.", RegexOptions.IgnoreCase).Matches("HELLO!"));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.IsLike(\"[hH]ello\")", new IsLikeArgumentFilter("[hH]ello", RegexOptions.None).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_pattern()
        {
            Assert.False(new IsLikeArgumentFilter("foo", RegexOptions.None).Equals(new IsLikeArgumentFilter("bar", RegexOptions.None)));
            Assert.False(new IsLikeArgumentFilter("foo", RegexOptions.None).Equals(new IsLikeArgumentFilter("Foo", RegexOptions.None)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_different_options()
        {
            Assert.False(new IsLikeArgumentFilter("foo", RegexOptions.None).Equals(new IsLikeArgumentFilter("foo", RegexOptions.Multiline)));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_pattern_and_options()
        {
            Assert.True(new IsLikeArgumentFilter("foo", RegexOptions.None).Equals(new IsLikeArgumentFilter("foo", RegexOptions.None)));
            Assert.True(new IsLikeArgumentFilter("foo", RegexOptions.Multiline | RegexOptions.Compiled).Equals(new IsLikeArgumentFilter("foo", RegexOptions.Multiline | RegexOptions.Compiled)));
        }
    }
}