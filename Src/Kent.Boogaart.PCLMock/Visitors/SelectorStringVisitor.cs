namespace Kent.Boogaart.PCLMock.Visitors
{
    using Kent.Boogaart.PCLMock.Utility;
    using System.Linq.Expressions;
    using System.Text;

    // converts the given expression into a string that can be used for debugging purposes
    internal sealed class SelectorStringVisitor : ExpressionVisitor
    {
        private readonly StringBuilder stringBuilder;

        public SelectorStringVisitor()
        {
            this.stringBuilder = new StringBuilder();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            this.stringBuilder.Append(node.Member.Name);
            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.stringBuilder.Append(node.Method.Name).Append("(");

            ArgumentFilterCollection argumentFilters;

            if (ArgumentFiltersVisitor.TryFindArgumentFiltersWithin(node, out argumentFilters))
            {
                for (var i = 0; i < argumentFilters.Count; ++i)
                {
                    if (i > 0)
                    {
                        this.stringBuilder.Append(", ");
                    }

                    this.stringBuilder.Append(argumentFilters[i].ToString());
                }
            }

            this.stringBuilder.Append(")");

            return node;
        }

        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }
    }
}