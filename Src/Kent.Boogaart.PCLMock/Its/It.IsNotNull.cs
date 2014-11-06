namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only non-<see langword="null"/> values are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNotNull<T>()
            where T : class
        {
            return default(T);
        }

        internal static IArgumentFilter IsNotNullFilter<T>()
            where T : class
        {
            return IsNotNullArgumentFilter<T>.Instance;
        }

        private sealed class IsNotNullArgumentFilter<T> : IArgumentFilter, IEquatable<IsNotNullArgumentFilter<T>>
        {
            public static readonly IsNotNullArgumentFilter<T> Instance = new IsNotNullArgumentFilter<T>();

            private IsNotNullArgumentFilter()
            {
            }

            public bool Matches(object argument)
            {
                return argument != null;
            }

            public override string ToString()
            {
                return "Is not null " + typeof(T).FullName;
            }

            public bool Equals(IsNotNullArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsNotNullArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
