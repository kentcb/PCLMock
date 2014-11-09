namespace Kent.Boogaart.PCLMock.UnitTests.Visitors
{
    using Kent.Boogaart.PCLMock.Visitors;
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public sealed class ArgumentsVisitorFixture
    {
        [Fact]
        public void can_find_arguments_in_parameterless_method_call()
        {
            var arguments = ArgumentsVisitor.FindArgumentsWithin((Expression<Action>)(() => Console.WriteLine()));
            Assert.NotNull(arguments);
            Assert.Equal(0, arguments.Length);
        }

        [Fact]
        public void can_find_arguments_within_method_call_taking_parameters()
        {
            var arguments = ArgumentsVisitor.FindArgumentsWithin((Expression<Action>)(() => Console.WriteLine("something {0}, {1}, {2}", 13, "foo", DateTime.MinValue)));
            Assert.NotNull(arguments);
            Assert.Equal(4, arguments.Length);
            Assert.Equal("something {0}, {1}, {2}", arguments[0]);
            Assert.Equal(13, arguments[1]);
            Assert.Equal("foo", arguments[2]);
            Assert.Equal(DateTime.MinValue, arguments[3]);
        }

        [Fact]
        public void cannot_find_arguments_in_non_method_call()
        {
            object[] arguments;
            Assert.False(ArgumentsVisitor.TryFindArgumentsWithin((Expression<Func<int>>)(() => 13), out arguments));
            Assert.False(ArgumentsVisitor.TryFindArgumentsWithin((Expression<Func<object>>)(() => "foo"), out arguments));
        }
    }
}
