namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Kent.Boogaart.PCLMock;
    using System.Text;
    using Xunit;

    public sealed class ItIsNotNullFixture
    {
        [Fact]
        public void filter_returns_true_if_value_is_not_null()
        {
            Assert.True(It.IsNotNullFilter<string>().Matches("hello"));
            Assert.True(It.IsNotNullFilter<string>().Matches("world"));
        }

        [Fact]
        public void filter_returns_false_if_value_is_null()
        {
            Assert.False(It.IsNotNullFilter<string>().Matches(null));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is not null System.String", It.IsNotNullFilter<string>().ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsNotNullFilter<string>().Equals(It.IsNotNullFilter<StringBuilder>()));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_for_the_same_type()
        {
            Assert.True(It.IsNotNullFilter<string>().Equals(It.IsNotNullFilter<string>()));
            Assert.True(It.IsNotNullFilter<StringBuilder>().Equals(It.IsNotNullFilter<StringBuilder>()));
        }
    }
}