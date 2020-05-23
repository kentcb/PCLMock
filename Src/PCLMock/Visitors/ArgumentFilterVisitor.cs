namespace PCLMock.Visitors
{
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using PCLMock.ArgumentFilters;

    // find a single argument filter at the root of the given expression
    internal sealed class ArgumentFilterVisitor : ExpressionVisitor
    {
        private IArgumentFilter argumentFilter;

        private ArgumentFilterVisitor()
        {
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
                .DeclaredMethods
                .Where(
                    declaredMethod =>
                        declaredMethod.IsStatic &&
                        declaredMethod.Name == node.Method.Name + "Filter" &&
                        declaredMethod.GetParameters().Length == node.Arguments.Count)
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
                    argumentsToFilterMethod[i] = ValueExtractor.FindValueWithin(node.Arguments[i]);
                }

                this.argumentFilter = filterMethod.Invoke(null, argumentsToFilterMethod) as IArgumentFilter;
            }
            else
            {
                object value;

                if (ValueExtractor.TryFindValueWithin(node, out value))
                {
                    this.argumentFilter = new IsArgumentFilter(value);
                }
            }

            return node;
        }

        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.Call)
            {
                return base.Visit(node);
            }

            object value;

            if (ValueExtractor.TryFindValueWithin(node, out value))
            {
                this.argumentFilter = new IsArgumentFilter(value);
                return node;
            }

            return node;
        }
    }
}