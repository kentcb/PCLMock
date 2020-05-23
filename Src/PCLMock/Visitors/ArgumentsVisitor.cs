namespace PCLMock.Visitors
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    // finds the values of all arguments in a method invocation expression
    internal sealed class ArgumentsVisitor : ExpressionVisitor
    {
        private object[] arguments;

        private ArgumentsVisitor()
        {
        }

        public static bool TryFindArgumentsWithin(Expression expression, out object[] arguments)
        {
            Debug.Assert(expression != null);

            var visitor = new ArgumentsVisitor();

            try
            {
                visitor.Visit(expression);
                arguments = visitor.arguments;

                return arguments != null;
            }
            catch (Exception)
            {
            }

            arguments = null;
            return false;
        }

        public static object[] FindArgumentsWithin(Expression expression)
        {
            object[] arguments;

            if (!TryFindArgumentsWithin(expression, out arguments))
            {
                return null;
            }

            return arguments;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.arguments = node
                .Arguments
                .Select(x => ValueExtractor.FindValueWithin(x))
                .ToArray();

            return Expression.Constant(this.arguments);
        }
    }
}