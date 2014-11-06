namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only <see langword="null"/> values are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNull<T>()
            where T : class
        {
            return default(T);
        }

        internal static IArgumentFilter IsNullFilter<T>()
            where T : class
        {
            return IsNullArgumentFilter<T>.Instance;
        }

        private sealed class IsNullArgumentFilter<T> : IArgumentFilter, IEquatable<IsNullArgumentFilter<T>>
        {
            public static readonly IsNullArgumentFilter<T> Instance = new IsNullArgumentFilter<T>();

            private IsNullArgumentFilter()
            {
            }

            public bool Matches(object argument)
            {
                return argument == null;
            }

            public override string ToString()
            {
                return "Is null " + typeof(T).FullName;
            }

            public bool Equals(IsNullArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsNullArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
