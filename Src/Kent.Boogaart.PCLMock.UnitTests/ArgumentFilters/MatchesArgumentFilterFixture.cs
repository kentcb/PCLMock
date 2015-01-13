namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using System;
    using Xunit;
    using Xunit.Extensions;

    public sealed class MatchesArgumentFilterFixture
    {
        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(4, true)]
        public void matches_returns_correct_value(object value, bool expectedResult)
        {
            Assert.Equal(expectedResult, new MatchesArgumentFilter<int>(x => x % 2 == 0).Matches(value));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            Assert.Equal("It.Matches<string>(<predicate>)", new MatchesArgumentFilter<string>(_ => true).ToString());
            Assert.Equal("It.Matches<int>(<predicate>)", new MatchesArgumentFilter<int>(_ => true).ToString());
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_for_a_different_type()
        {
            Assert.False(new MatchesArgumentFilter<string>(_ => true).Equals(new MatchesArgumentFilter<int>(_ => true)));
        }

        [Fact]
        public void equals_returns_false_if_given_a_filter_with_a_different_predicate()
        {
            Assert.False(new MatchesArgumentFilter<string>(_ => true).Equals(new MatchesArgumentFilter<string>(_ => false)));
            Assert.False(new MatchesArgumentFilter<string>(_ => false).Equals(new MatchesArgumentFilter<string>(_ => true)));
            Assert.False(new MatchesArgumentFilter<int>(_ => true).Equals(new MatchesArgumentFilter<int>(_ => false)));
            Assert.False(new MatchesArgumentFilter<int>(_ => false).Equals(new MatchesArgumentFilter<int>(_ => true)));
        }

        [Fact]
        public void equals_returns_true_if_given_a_filter_with_the_same_predicate()
        {
            Func<string, bool> stringTruePredicate = _ => true;
            Func<string, bool> stringFalsePredicate = _ => false;
            Func<int, bool> intTruePredicate = _ => true;
            Func<int, bool> intFalsePredicate = _ => false;

            Assert.True(new MatchesArgumentFilter<string>(stringTruePredicate).Equals(new MatchesArgumentFilter<string>(stringTruePredicate)));
            Assert.True(new MatchesArgumentFilter<string>(stringFalsePredicate).Equals(new MatchesArgumentFilter<string>(stringFalsePredicate)));
            Assert.True(new MatchesArgumentFilter<int>(intTruePredicate).Equals(new MatchesArgumentFilter<int>(intTruePredicate)));
            Assert.True(new MatchesArgumentFilter<int>(intFalsePredicate).Equals(new MatchesArgumentFilter<int>(intFalsePredicate)));
        }
    }
}