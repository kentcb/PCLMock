namespace Kent.Boogaart.PCLMock.Visitors
{
    using System.Diagnostics;
    using System.Linq.Expressions;
    using Kent.Boogaart.PCLMock.ArgumentFilters;
    using Kent.Boogaart.PCLMock.Utility;

    // find a set of argument filters for a method call
    internal sealed class ArgumentFiltersVisitor : ExpressionVisitor
    {
        private ArgumentFilterCollection argumentFilters;

        private ArgumentFiltersVisitor()
        {
        }

        public static bool TryFindArgumentFiltersWithin(Expression expression, out ArgumentFilterCollection argumentFilters)
        {
            Debug.Assert(expression != null);

            var visitor = new ArgumentFiltersVisitor();
            visitor.Visit(expression);
            argumentFilters = visitor.argumentFilters;
            return argumentFilters != null;
        }

        public static ArgumentFilterCollection FindArgumentFiltersWithin(Expression expression)
        {
            ArgumentFilterCollection argumentFilters;

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
            this.argumentFilters = new ArgumentFilterCollection();
            var methodParameters = node.Method.GetParameters();

            for (var i = 0; i < methodParameters.Length; ++i)
            {
                if (methodParameters[i].ParameterType.IsByRef || methodParameters[i].IsOut)
                {
                    this.argumentFilters.Add(IsAnyArgumentFilter<object>.Instance);
                }
                else
                {
                    this.argumentFilters.Add(ArgumentFilterVisitor.FindArgumentFilterWithin(node.Arguments[i]));
                }
            }

            // we don't recurse down beyond the top-level method call
            return node;
        }
    }
}