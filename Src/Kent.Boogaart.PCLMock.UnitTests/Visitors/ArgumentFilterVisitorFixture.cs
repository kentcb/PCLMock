using Kent.Boogaart.PCLMock.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kent.Boogaart.PCLMock.UnitTests.Visitors
{
    public sealed class ArgumentFilterVisitorFixture
    {
        [Fact]
        public void can_find_argument_filter_in_method_call_against_it_class()
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin((Expression<Action>)(() => It.IsAny<string>()));
            Assert.NotNull(argumentFilter);
            Assert.True(argumentFilter.Matches("foo"));
            Assert.True(argumentFilter.Matches("bar"));
            Assert.True(argumentFilter.Matches(null));

            argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin((Expression<Action>)(() => It.Is("foo")));
            Assert.NotNull(argumentFilter);
            Assert.True(argumentFilter.Matches("foo"));
            Assert.False(argumentFilter.Matches("bar"));

            argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin((Expression<Action>)(() => It.IsIn(1, 2, 3)));
            Assert.NotNull(argumentFilter);
            Assert.True(argumentFilter.Matches(1));
            Assert.True(argumentFilter.Matches(3));
            Assert.False(argumentFilter.Matches(4));
        }

        [Fact]
        public void a_constant_at_the_top_level_is_interpreted_as_it_is()
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin((Expression<Func<int>>)(() => 35));
            Assert.NotNull(argumentFilter);
            Assert.IsType<It.IsArgumentFilter>(argumentFilter);
            Assert.True(argumentFilter.Matches(35));
            Assert.False(argumentFilter.Matches(34));

            argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin((Expression<Func<object>>)(() => 35));
            Assert.NotNull(argumentFilter);
            Assert.IsType<It.IsArgumentFilter>(argumentFilter);
            Assert.True(argumentFilter.Matches(35));
            Assert.False(argumentFilter.Matches(34));
        }

        [Fact]
        public void cannot_find_argument_filter_in_random_method_call()
        {
            var argumentFilter = ArgumentFilterVisitor.FindArgumentFilterWithin((Expression<Action>)(() => Console.WriteLine()));
            Assert.Null(argumentFilter);
        }
    }
}
