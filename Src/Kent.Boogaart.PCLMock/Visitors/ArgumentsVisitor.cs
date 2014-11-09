namespace Kent.Boogaart.PCLMock.Visitors
{
    using System.Diagnostics;
    using System.Linq.Expressions;

    // finds the values of all arguments in a method invocation expression
    internal sealed class ArgumentsVisitor : ExpressionVisitor
    {
        private readonly ConstantVisitor constantVisitor;
        private object[] arguments;

        private ArgumentsVisitor()
        {
            this.constantVisitor = new ConstantVisitor();
        }

        public static bool TryFindArgumentsWithin(Expression expression, out object[] arguments)
        {
            Debug.Assert(expression != null);

            var visitor = new ArgumentsVisitor();
            visitor.Visit(expression);
            arguments = visitor.arguments;
            return arguments != null;
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

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // we only support finding arguments if it's a lambda with a method call at the top level
            return this.Visit(node.Body as MethodCallExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.arguments = new object[node.Arguments.Count];

            for (var i = 0; i < this.arguments.Length; ++i)
            {
                var constantExpression = this.constantVisitor.Visit(node.Arguments[i]) as ConstantExpression;
                this.arguments[i] = constantExpression == null ? null : constantExpression.Value;
            }

            return base.VisitMethodCall(node);
        }
    }
}