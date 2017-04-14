namespace PCLMock.Visitors
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using PCLMock.ArgumentFilters;
    using PCLMock.Utility;

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

            try
            {
                visitor.Visit(expression);
                argumentFilters = visitor.argumentFilters;
                return argumentFilters != null && argumentFilters.All(x => x != null);
            }
            catch (Exception)
            {
            }

            argumentFilters = null;
            return false;
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

            return Expression.Constant(this.argumentFilters);
        }
    }
}