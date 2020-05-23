namespace PCLMock.Visitors
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    // extracts values from an expression
    //
    // examples:
    //      null                        extracts null
    //      "foo"                       extracts "foo"
    //      foo                         extracts the value of local variable foo
    //      new [] { 1, 2, 3 }          extracts int[] { 1, 2, 3 }
    //      int.MinValue                extracts minimum int value
    //      DateTime.MaxValue           extracts the maximum DateTime value
    //      new DateTime(...)           extracts the specified DateTime value
    //      DateTime.FromSeconds(1)     extracts the specified DateTime value
    //      i => i % 2 == 0             extracts the lambda i => i % 2 == 0
    internal static class ValueExtractor
    {
        public static bool TryFindValueWithin(Expression expression, out object value)
        {
            Debug.Assert(expression != null);

            if (expression is MethodCallExpression methodCallExpression && methodCallExpression.Method.ReturnType == typeof(void))
            {
                value = null;
                return false;
            }

            try
            {
                value = Expression
                    .Lambda(expression)
                    .Compile()
                    .DynamicInvoke();

                return true;
            }
            catch (Exception)
            {
            }

            value = null;
            return false;
        }

        public static object FindValueWithin(Expression expression)
        {
            object value;

            if (!TryFindValueWithin(expression, out value))
            {
                return null;
            }

            return value;
        }
    }
}