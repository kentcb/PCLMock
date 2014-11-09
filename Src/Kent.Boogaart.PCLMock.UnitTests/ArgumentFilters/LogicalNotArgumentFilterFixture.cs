namespace Kent.Boogaart.PCLMock.ArgumentFilters.UnitTests
{
    using System;
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Xunit;

    public sealed class LogicalNotArgumentFilterFixture
    {
        [Fact]
        public void matches_returns_opposite_of_what_child_returns()
        {
            var child = new IsArgumentFilter("foo");
            var argumentFilter = new LogicalNotArgumentFilter(child);

            Assert.False(argumentFilter.Matches("foo"));
            Assert.True(argumentFilter.Matches("bar"));
        }

        [Fact]
        public void has_a_nice_string_representation()
        {
            var child = new IsArgumentFilter("foo");
            var argumentFilter = new LogicalNotArgumentFilter(child);

            Assert.Equal("NOT(Is \"foo\")", argumentFilter.ToString());
        }

        [Fact]
        public void equals_returns_uses_childs_equality_to_determine_result()
        {
            var child1 = new IsArgumentFilter("foo");
            var child2 = new IsArgumentFilter("bar");
            var child3 = new IsArgumentFilter("foo");
            var argumentFilter1 = new LogicalNotArgumentFilter(child1);
            var argumentFilter2 = new LogicalNotArgumentFilter(child2);
            var argumentFilter3 = new LogicalNotArgumentFilter(child3);

            Assert.False(argumentFilter1.Equals(argumentFilter2));
            Assert.True(argumentFilter1.Equals(argumentFilter3));
        }
    }
}