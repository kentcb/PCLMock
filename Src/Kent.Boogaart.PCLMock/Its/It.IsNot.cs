namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values that do not equate to <paramref name="notExpected"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the unexpected value.
        /// </typeparam>
        /// <param name="notExpected">
        /// The value that is not expected.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNot<T>(T notExpected)
        {
            return default(T);
        }

        internal static IArgumentFilter IsNotFilter<T>(T notExpected)
        {
            return new IsNotArgumentFilter(notExpected);
        }

        private sealed class IsNotArgumentFilter : IArgumentFilter, IEquatable<IsNotArgumentFilter>
        {
            private readonly object notExpected;

            public IsNotArgumentFilter(object notExpected)
            {
                this.notExpected = notExpected;
            }

            public bool Matches(object argument)
            {
                return !object.Equals(argument, this.notExpected);
            }

            public override string ToString()
            {
                return "Is not " + FormatValue(this.notExpected);
            }

            public bool Equals(IsNotArgumentFilter other)
            {
                if (other == null)
                {
                    return false;
                }

                return object.Equals(this.notExpected, other.notExpected);
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsNotArgumentFilter);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
