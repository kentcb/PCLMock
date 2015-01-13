namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using System;
    using System.Text;
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Xunit;

    public sealed class IsOfTypeArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_false_if_value_is_not_of_specified_type()
        {
            Assert.False(IsOfTypeArgumentFilter<string>.Instance.Matches(1));
            Assert.False(IsOfTypeArgumentFilter<string>.Instance.Matches(new StringBuilder()));
        }

        [Fact]
        public void matches_returns_true_if_value_is_of_specified_type()
        {
            Assert.True(IsOfTypeArgumentFilter<string>.Instance.Matches("foo"));
            Assert.True(IsOfTypeArgumentFilter<IComparable>.Instance.Matches(1));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.IsOfType<string>()", IsOfTypeArgumentFilter<string>.Instance.ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(IsOfTypeArgumentFilter<string>.Instance.Equals(IsOfTypeArgumentFilter<int>.Instance));
            Assert.False(IsOfTypeArgumentFilter<IComparable<string>>.Instance.Equals(IsOfTypeArgumentFilter<IComparable<int>>.Instance));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_for_the_same_type()
        {
            Assert.True(IsOfTypeArgumentFilter<string>.Instance.Equals(IsOfTypeArgumentFilter<string>.Instance));
            Assert.True(IsOfTypeArgumentFilter<IComparable>.Instance.Equals(IsOfTypeArgumentFilter<IComparable>.Instance));
            Assert.True(IsOfTypeArgumentFilter<int>.Instance.Equals(IsOfTypeArgumentFilter<int>.Instance));
        }
    }
}