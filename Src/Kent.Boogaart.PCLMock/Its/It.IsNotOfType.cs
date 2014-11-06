namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values not of type <typeparamref name="T"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The unexpected type.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNotOfType<T>()
        {
            return default(T);
        }

        internal static IArgumentFilter IsNotOfTypeFilter<T>()
        {
            return IsNotOfTypeArgumentFilter<T>.Instance;
        }

        private sealed class IsNotOfTypeArgumentFilter<T> : IArgumentFilter, IEquatable<IsNotOfTypeArgumentFilter<T>>
        {
            public static readonly IsNotOfTypeArgumentFilter<T> Instance = new IsNotOfTypeArgumentFilter<T>();

            private IsNotOfTypeArgumentFilter()
            {
            }

            public bool Matches(object argument)
            {
                return !(argument is T);
            }

            public override string ToString()
            {
                return "Is not of type " + typeof(T).FullName;
            }

            public bool Equals(IsNotOfTypeArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsNotOfTypeArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
