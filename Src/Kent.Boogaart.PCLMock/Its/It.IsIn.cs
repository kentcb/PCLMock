namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values that are within <paramref name="expected"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <param name="expected">
        /// The set of expected values.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsIn<T>(params T[] expected)
        {
            return default(T);
        }

        internal static IArgumentFilter IsInFilter<T>(params T[] expected)
        {
            return new IsInArgumentFilter<T>(expected);
        }

        private sealed class IsInArgumentFilter<T> : IArgumentFilter, IEquatable<IsInArgumentFilter<T>>
        {
            private readonly ISet<T> expected;

            public IsInArgumentFilter(T[] expected)
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
                sb.Append("Is in [");
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
                        sb.Append(FormatValue(next));
                        return sb;
                    });

                sb.Append("]");

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
}