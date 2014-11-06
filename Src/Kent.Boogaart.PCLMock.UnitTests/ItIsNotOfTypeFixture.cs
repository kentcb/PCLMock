namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Kent.Boogaart.PCLMock;
    using System;
    using System.Text;
    using Xunit;

    public sealed class ItIsNotOfTypeFixture
    {
        [Fact]
        public void filter_returns_true_if_value_is_not_of_specified_type()
        {
            Assert.True(It.IsNotOfTypeFilter<string>().Matches(1));
            Assert.True(It.IsNotOfTypeFilter<string>().Matches(new StringBuilder()));
        }

        [Fact]
        public void filter_returns_false_if_value_is_of_specified_type()
        {
            Assert.False(It.IsNotOfTypeFilter<string>().Matches("foo"));
            Assert.False(It.IsNotOfTypeFilter<IComparable>().Matches(1));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is not of type System.String", It.IsNotOfTypeFilter<string>().ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsNotOfTypeFilter<string>().Equals(It.IsNotOfTypeFilter<int>()));
            Assert.False(It.IsNotOfTypeFilter<IComparable<string>>().Equals(It.IsNotOfTypeFilter<IComparable<int>>()));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_for_the_same_type()
        {
            Assert.True(It.IsNotOfTypeFilter<string>().Equals(It.IsNotOfTypeFilter<string>()));
            Assert.True(It.IsNotOfTypeFilter<IComparable>().Equals(It.IsNotOfTypeFilter<IComparable>()));
            Assert.True(It.IsNotOfTypeFilter<int>().Equals(It.IsNotOfTypeFilter<int>()));
        }
    }
}