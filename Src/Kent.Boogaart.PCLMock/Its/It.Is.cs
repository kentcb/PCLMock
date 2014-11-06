namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values that equate to <paramref name="expected"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <param name="expected">
        /// The expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T Is<T>(T expected)
        {
            return default(T);
        }

        internal static IArgumentFilter IsFilter<T>(T expected)
        {
            return new IsArgumentFilter(expected);
        }

        internal sealed class IsArgumentFilter : IArgumentFilter, IEquatable<IsArgumentFilter>
        {
            private readonly object expected;

            public IsArgumentFilter(object expected)
            {
                this.expected = expected;
            }

            public bool Matches(object argument)
            {
                return object.Equals(argument, this.expected);
            }

            public override string ToString()
            {
                return "Is " + FormatValue(this.expected);
            }

            public bool Equals(IsArgumentFilter other)
            {
                if (other == null)
                {
                    return false;
                }

                return this.Matches(other.expected);
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsArgumentFilter);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
