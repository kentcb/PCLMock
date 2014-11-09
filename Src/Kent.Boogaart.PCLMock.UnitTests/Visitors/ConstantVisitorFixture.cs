namespace Kent.Boogaart.PCLMock.UnitTests.Visitors
{
    using Kent.Boogaart.PCLMock.Visitors;
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public sealed class ConstantVisitorFixture
    {
        [Fact]
        public void can_find_constant_in_an_expression_that_simply_returns_a_constant()
        {
            Assert.Equal(5, ConstantVisitor.FindConstantWithin((Expression<Func<int>>)(() => 5)));
            Assert.Equal("foo", ConstantVisitor.FindConstantWithin((Expression<Func<string>>)(() => "foo")));
            Assert.Equal(12.7156m, ConstantVisitor.FindConstantWithin((Expression<Func<decimal>>)(() => 12.7156m)));
        }

        [Fact]
        public void can_find_constant_in_an_expression_that_returns_a_dereferenced_constant()
        {
            Assert.Equal(int.MaxValue, ConstantVisitor.FindConstantWithin((Expression<Func<int>>)(() => int.MaxValue)));
            Assert.Equal(DateTime.MinValue, ConstantVisitor.FindConstantWithin((Expression<Func<DateTime>>)(() => DateTime.MinValue)));
            Assert.Equal(decimal.MinusOne, ConstantVisitor.FindConstantWithin((Expression<Func<decimal>>)(() => decimal.MinusOne)));
        }

        [Fact]
        public void can_find_constant_in_an_expression_that_returns_a_local_value()
        {
            var local = "foo";
            Assert.Equal("foo", ConstantVisitor.FindConstantWithin((Expression<Func<string>>)(() => local)));
        }

        [Fact]
        public void can_find_constant_in_an_expression_that_creates_an_array()
        {
            Assert.Equal(new[] { 1, 2, 3 }, ConstantVisitor.FindConstantWithin((Expression<Func<int[]>>)(() => new [] { 1, 2, 3 })));
            Assert.Equal(new[] { DateTime.MinValue, DateTime.MaxValue }, ConstantVisitor.FindConstantWithin((Expression<Func<DateTime[]>>)(() => new [] { DateTime.MinValue, DateTime.MaxValue })));
        }

        [Fact]
        public void can_find_constant_in_an_expression_that_news_up_an_object()
        {
            Assert.Equal(new DateTime(1979, 10, 26), ConstantVisitor.FindConstantWithin((Expression<Func<DateTime>>)(() => new DateTime(1979, 10, 26))));
            Assert.Equal(Tuple.Create("one", 1), ConstantVisitor.FindConstantWithin((Expression<Func<Tuple<string, int>>>)(() => new Tuple<string, int>("one", 1))));
        }

        [Fact]
        public void can_find_constant_in_an_expression_that_requires_conversion()
        {
            Assert.Equal(5, ConstantVisitor.FindConstantWithin((Expression<Func<object>>)(() => 5)));
        }

        [Fact]
        public void cannot_find_constant_in_a_method_invocation()
        {
            // technically, we could find the constant if the method has a single parameter, but I think it best to avoid confusion by simply disallowing method invocations at all
            var ex = Assert.Throws<InvalidOperationException>(() => ConstantVisitor.FindConstantWithin((Expression<Action>)(() => Console.WriteLine("foo"))));
            Assert.Equal("Finding constants within method invocations is ambiguous.", ex.Message);
        }
    }
}