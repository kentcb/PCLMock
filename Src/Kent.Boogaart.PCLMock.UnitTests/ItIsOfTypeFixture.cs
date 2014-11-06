namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Kent.Boogaart.PCLMock;
    using System;
    using System.Text;
    using Xunit;

    public sealed class ItIsOfTypeFixture
    {
        [Fact]
        public void filter_returns_false_if_value_is_not_of_specified_type()
        {
            Assert.False(It.IsOfTypeFilter<string>().Matches(1));
            Assert.False(It.IsOfTypeFilter<string>().Matches(new StringBuilder()));
        }

        [Fact]
        public void filter_returns_true_if_value_is_of_specified_type()
        {
            Assert.True(It.IsOfTypeFilter<string>().Matches("foo"));
            Assert.True(It.IsOfTypeFilter<IComparable>().Matches(1));
        }

        [Fact]
        public void filter_has_a_nice_string_representation()
        {
            Assert.Equal("Is of type System.String", It.IsOfTypeFilter<string>().ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(It.IsOfTypeFilter<string>().Equals(It.IsOfTypeFilter<int>()));
            Assert.False(It.IsOfTypeFilter<IComparable<string>>().Equals(It.IsOfTypeFilter<IComparable<int>>()));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_for_the_same_type()
        {
            Assert.True(It.IsOfTypeFilter<string>().Equals(It.IsOfTypeFilter<string>()));
            Assert.True(It.IsOfTypeFilter<IComparable>().Equals(It.IsOfTypeFilter<IComparable>()));
            Assert.True(It.IsOfTypeFilter<int>().Equals(It.IsOfTypeFilter<int>()));
        }
    }
}