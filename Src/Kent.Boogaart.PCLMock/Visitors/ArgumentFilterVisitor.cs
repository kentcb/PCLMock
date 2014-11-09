namespace Kent.Boogaart.PCLMock.Visitors
{
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Kent.Boogaart.PCLMock.ArgumentFilters;

    // find a single argument filter at the root of the given expression
    internal sealed class ArgumentFilterVisitor : ExpressionVisitor
    {
        private readonly ConstantVisitor constantVisitor;
        private IArgumentFilter argumentFilter;

        private ArgumentFilterVisitor()
        {
            this.constantVisitor = new ConstantVisitor();
        }

        public static bool TryFindArgumentFilterWithin(Expression expression, out IArgumentFilter argumentFilter)
        {
            Debug.Assert(expression != null);

            var visitor = new ArgumentFilterVisitor();
            visitor.Visit(expression);
            argumentFilter = visitor.argumentFilter;
            return argumentFilter != null;
        }

        public static IArgumentFilter FindArgumentFilterWithin(Expression expression)
        {
            IArgumentFilter argumentFilter;

            if (!TryFindArgumentFilterWithin(expression, out argumentFilter))
            {
                return null;
            }

            return argumentFilter;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var filterMethod = node
                .Method
                .DeclaringType
                .GetTypeInfo()
                .GetDeclaredMethods(node.Method.Name + "Filter")
                .Where(x => x.GetParameters().Length == node.Arguments.Count)
                .FirstOrDefault();

            if (filterMethod != null && filterMethod.ReturnType == typeof(IArgumentFilter))
            {
                if (node.Method.IsGenericMethod)
                {
                    filterMethod = filterMethod.MakeGenericMethod(node.Method.GetGenericArguments());
                }

                var argumentsToFilterMethod = new object[node.Arguments.Count];

                for (var i = 0; i < argumentsToFilterMethod.Length; ++i)
                {
                    var constantExpression = this.constantVisitor.Visit(node.Arguments[i]) as ConstantExpression;
                    argumentsToFilterMethod[i] = constantExpression == null ? null : constantExpression.Value;
                }

                this.argumentFilter = filterMethod.Invoke(null, argumentsToFilterMethod) as IArgumentFilter;
            }

            // we don't visit any further than the top level call
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.argumentFilter = new IsArgumentFilter(node.Value);
            return node;
        }
    }
}