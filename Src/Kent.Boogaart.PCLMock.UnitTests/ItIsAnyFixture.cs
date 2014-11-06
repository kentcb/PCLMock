namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItIsAnyFixture
    {
        [Fact]
        public void filter_is_a_singleton()
        {
            Assert.Same(It.IsAnyFilter<string>(), It.IsAnyFilter<string>());
        }

        [Fact]
        public void filter_returns_true_for_null()
        {
            Assert.True(It.IsAnyFilter<string>().Matches(null));
        }

        [Fact]
        public void filter_returns_true_for_any_value()
        {
            Assert.True(It.IsAnyFilter<string>().Matches("hello"));
            Assert.True(It.IsAnyFilter<string>().Matches("world"));
            Assert.True(It.IsAnyFilter<string>().Matches(""));

            Assert.True(It.IsAnyFilter<int>().Matches(0));
            Assert.True(It.IsAnyFilter<int>().Matches(1));
            Assert.True(It.IsAnyFilter<int>().Matches(-1));
            Assert.True(It.IsAnyFilter<int>().Matches(int.MaxValue));
            Assert.True(It.IsAnyFilter<int>().Matches(int.MinValue));
            Assert.True(It.IsAnyFilter<int>().Matches(38));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is any System.String", It.IsAnyFilter<string>().ToString());
            Assert.Equal("Is any System.Int32", It.IsAnyFilter<int>().ToString());
            Assert.Equal("Is any System.Nullable`1[System.Int32]", It.IsAnyFilter<int?>().ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_null()
        {
            Assert.False(It.IsAnyFilter<string>().Equals(null));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsAnyFilter<string>().Equals(It.IsAnyFilter<int>()));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_of_the_same_type()
        {
            Assert.True(It.IsAnyFilter<string>().Equals(It.IsAnyFilter<string>()));
        }
    }
}
