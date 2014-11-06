namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Kent.Boogaart.PCLMock;
    using System;
    using System.Text.RegularExpressions;
    using Xunit;

    public sealed class ItIsLikeFixture
    {
        [Fact]
        public void it_is_like_filter_throws_if_pattern_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => It.IsLikeFilter(null, RegexOptions.None));
        }

        [Fact]
        public void filter_returns_false_if_values_do_not_match()
        {
            Assert.False(It.IsLikeFilter("[hH]ello", RegexOptions.None).Matches("world"));
            Assert.False(It.IsLikeFilter("hello", RegexOptions.None).Matches(null));
        }

        [Fact]
        public void filter_returns_true_if_values_match()
        {
            Assert.True(It.IsLikeFilter("[hH]ello", RegexOptions.None).Matches("Hello"));
            Assert.True(It.IsLikeFilter("[hH]ello", RegexOptions.None).Matches("hello"));
            Assert.True(It.IsLikeFilter("hello.", RegexOptions.IgnoreCase).Matches("HELLO!"));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is like \"[hH]ello\"", It.IsLikeFilter("[hH]ello", RegexOptions.None).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_pattern()
        {
            Assert.False(It.IsLikeFilter("foo", RegexOptions.None).Equals(It.IsLikeFilter("bar", RegexOptions.None)));
            Assert.False(It.IsLikeFilter("foo", RegexOptions.None).Equals(It.IsLikeFilter("Foo", RegexOptions.None)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_different_options()
        {
            Assert.False(It.IsLikeFilter("foo", RegexOptions.None).Equals(It.IsLikeFilter("foo", RegexOptions.Multiline)));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_pattern_and_options()
        {
            Assert.True(It.IsLikeFilter("foo", RegexOptions.None).Equals(It.IsLikeFilter("foo", RegexOptions.None)));
            Assert.True(It.IsLikeFilter("foo", RegexOptions.Multiline | RegexOptions.Compiled).Equals(It.IsLikeFilter("foo", RegexOptions.Multiline | RegexOptions.Compiled)));
        }
    }
}