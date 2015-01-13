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
            var arguments = ArgumentsVisitor.FindArgumentsWithin(GetExpression(() => Console.WriteLine()));
            Assert.NotNull(arguments);
            Assert.Equal(0, arguments.Length);
        }

        [Fact]
        public void can_find_arguments_within_method_call_taking_parameters()
        {
            var arguments = ArgumentsVisitor.FindArgumentsWithin(GetExpression(() => Console.WriteLine("something {0}, {1}, {2}", 13, DateTime.MinValue, Tuple.Create(1, "one"))));
            Assert.NotNull(arguments);
            Assert.Equal(4, arguments.Length);
            Assert.Equal("something {0}, {1}, {2}", arguments[0]);
            Assert.Equal(13, arguments[1]);
            Assert.Equal(DateTime.MinValue, arguments[2]);
            Assert.Equal(Tuple.Create(1, "one"), arguments[3]);
        }

        [Fact]
        public void cannot_find_arguments_in_non_method_call()
        {
            object[] arguments;
            Assert.False(ArgumentsVisitor.TryFindArgumentsWithin(GetExpression(() => 13), out arguments));
            Assert.False(ArgumentsVisitor.TryFindArgumentsWithin(GetExpression(() => "foo"), out arguments));
        }

        [Fact]
        public void cannot_find_arguments_if_there_is_a_method_call_but_it_is_not_at_root_level()
        {
            object[] arguments;
            Assert.False(ArgumentsVisitor.TryFindArgumentsWithin(GetExpression(() => new[] { Tuple.Create(1, 2) }), out arguments));
        }

        #region Supporting Members

        private static Expression GetExpression(Expression<Action> root)
        {
            return root.Body;
        }

        private static Expression GetExpression(Expression<Func<object>> root)
        {
            return root.Body;
        }

        #endregion
    }
}