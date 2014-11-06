namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Kent.Boogaart.PCLMock;
    using System.Text;
    using Xunit;

    public sealed class ItIsNullFixture
    {
        [Fact]
        public void filter_returns_false_if_value_is_not_null()
        {
            Assert.False(It.IsNullFilter<string>().Matches("hello"));
            Assert.False(It.IsNullFilter<string>().Matches("world"));
        }

        [Fact]
        public void filter_returns_true_if_value_is_null()
        {
            Assert.True(It.IsNullFilter<string>().Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is null System.String", It.IsNullFilter<string>().ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsNullFilter<string>().Equals(It.IsNullFilter<StringBuilder>()));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_for_the_same_type()
        {
            Assert.True(It.IsNullFilter<string>().Equals(It.IsNullFilter<string>()));
            Assert.True(It.IsNullFilter<StringBuilder>().Equals(It.IsNullFilter<StringBuilder>()));
        }
    }
}