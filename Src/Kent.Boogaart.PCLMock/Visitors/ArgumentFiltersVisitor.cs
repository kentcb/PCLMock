namespace Kent.Boogaart.PCLMock.Visitors
{
    using System.Diagnostics;
    using System.Linq.Expressions;

    // find a set of argument filters for a method call
    internal sealed class ArgumentFiltersVisitor : ExpressionVisitor
    {
        private IArgumentFilter[] argumentFilters;

        private ArgumentFiltersVisitor()
        {
        }

        public static bool TryFindArgumentFiltersWithin(Expression expression, out IArgumentFilter[] argumentFilters)
        {
            Debug.Assert(expression != null);

            var visitor = new ArgumentFiltersVisitor();
            visitor.Visit(expression);
            argumentFilters = visitor.argumentFilters;
            return argumentFilters != null;
        }

        public static IArgumentFilter[] FindArgumentFiltersWithin(Expression expression)
        {
            IArgumentFilter[] argumentFilters;

            if (!TryFindArgumentFiltersWithin(expression, out argumentFilters))
            {
                return null;
            }

            return argumentFilters;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Body.NodeType == ExpressionType.Call)
            {
                return this.Visit(node.Body);
            }

            return null;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.argumentFilters = new IArgumentFilter[node.Arguments.Count];
            var methodParameters = node.Method.GetParameters();

            for (var i = 0; i < argumentFilters.Length; ++i)
            {
                if (methodParameters[i].ParameterType.IsByRef || methodParameters[i].IsOut)
                {
                    this.argumentFilters[i] = It.IsAnyArgumentFilter<object>.Instance;
                }
                else
                {
                    this.argumentFilters[i] = ArgumentFilterVisitor.FindArgumentFilterWithin(node.Arguments[i]);
                }
            }

            // we don't recurse down beyond the top-level method call
            return node;
        }
    }
}