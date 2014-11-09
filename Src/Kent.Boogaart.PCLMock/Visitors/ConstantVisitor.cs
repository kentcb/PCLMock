namespace Kent.Boogaart.PCLMock.Visitors
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    // finds, and reduces to, the constant within an expression
    // Examples:
    //      () => "foo"                 reduces to "foo"
    //      () => foo                   reduces to the value of local variable foo
    //      () => new [] { 1, 2, 3 }    reduces to int[] { 1, 2, 3 }
    //      () => int.MinValue          reduces to minimum int value
    //      () => DateTime.MaxValue     reduces to the maximum DateTime value
    //      () => new DateTime(...)     reduces to the specified DateTime value
    internal sealed class ConstantVisitor : ExpressionVisitor
    {
        public static bool TryFindConstantWithin(Expression expression, out object constant)
        {
            Debug.Assert(expression != null);

            var visitor = new ConstantVisitor();
            var constantExpression = visitor.Visit(expression) as ConstantExpression;
            constant = constantExpression == null ? null : constantExpression.Value;
            return constantExpression != null;
        }

        public static object FindConstantWithin(Expression expression)
        {
            object constant;

            if (!TryFindConstantWithin(expression, out constant))
            {
                return null;
            }

            return constant;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // recurse down to simplify
            var constantExpression = this.Visit(node.Body) as ConstantExpression;
            return constantExpression;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // resurse down to simplify as much as possible
            var expression = this.Visit(node.Expression);
            var constantExpression = expression as ConstantExpression;
            var container = constantExpression == null ? null : constantExpression.Value;
            var member = node.Member;
            var fieldInfo = member as FieldInfo;

            if (fieldInfo != null)
            {
                return Expression.Constant(fieldInfo.GetValue(container));
            }

            var propertyInfo = member as PropertyInfo;

            if (propertyInfo != null)
            {
                return Expression.Constant(propertyInfo.GetValue(container, null));
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var args = new object[node.Arguments.Count];

            for (var i = 0; i < args.Length; ++i)
            {
                var constant = this.Visit(node.Arguments[i]) as ConstantExpression;
                args[i] = constant == null ? null : constant.Value;
            }

            return Expression.Constant(Activator.CreateInstance(node.Type, args));
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var array = (Array)Activator.CreateInstance(node.Type, node.Expressions.Count);

            for (var i = 0; i < node.Expressions.Count; ++i)
            {
                var constant = this.Visit(node.Expressions[i]) as ConstantExpression;
                array.SetValue(constant == null ? null : constant.Value, i);
            }

            return Expression.Constant(array);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                return this.Visit(node.Operand);
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // which argument would we examine for the constant?
            throw new InvalidOperationException("Finding constants within method invocations is ambiguous.");
        }
    }
}