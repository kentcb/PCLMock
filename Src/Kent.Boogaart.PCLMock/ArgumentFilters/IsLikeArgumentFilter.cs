namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Kent.Boogaart.PCLMock.Utility;

    internal sealed class IsLikeArgumentFilter : IArgumentFilter, IEquatable<IsLikeArgumentFilter>
    {
        private readonly Regex expression;

        public IsLikeArgumentFilter(string pattern, RegexOptions options)
        {
            Debug.Assert(pattern != null);
            this.expression = new Regex(pattern, options);
        }

        public bool Matches(object argument)
        {
            if (argument == null || !(argument is string))
            {
                return false;
            }

            return this.expression.IsMatch((string)argument);
        }

        public override string ToString()
        {
            return "Is like " + this.expression.ToString().ToDebugString();
        }

        public bool Equals(IsLikeArgumentFilter other)
        {
            if (other == null)
            {
                return false;
            }

            return
                this.expression.Options == other.expression.Options &&
                string.Equals(this.expression.ToString(), other.expression.ToString(), StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IsLikeArgumentFilter);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

}
