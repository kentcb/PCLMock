namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Kent.Boogaart.PCLMock.Utility;

    internal sealed class IsInArgumentFilter<T> : IArgumentFilter, IEquatable<IsInArgumentFilter<T>>
    {
        private readonly ISet<T> expected;

        public IsInArgumentFilter(params T[] expected)
        {
            this.expected = new HashSet<T>(expected ?? new T[0]);
        }

        public bool Matches(object argument)
        {
            if (argument != null && !(argument is T))
            {
                // shouldn't happen
                return false;
            }

            return this.expected.Contains((T)argument);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("It.IsIn(");
            var first = true;

            this.expected.Aggregate(
                sb,
                (running, next) =>
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append(next.ToDebugString());
                    return sb;
                });

            sb.Append(")");

            return sb.ToString();
        }

        public bool Equals(IsInArgumentFilter<T> other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.expected.Count != other.expected.Count)
            {
                return false;
            }

            return this.expected.IsSubsetOf(other.expected);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IsInArgumentFilter<T>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}