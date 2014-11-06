namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values of type <typeparamref name="T"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The expected type.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsOfType<T>()
        {
            return default(T);
        }

        internal static IArgumentFilter IsOfTypeFilter<T>()
        {
            return IsOfTypeArgumentFilter<T>.Instance;
        }

        private sealed class IsOfTypeArgumentFilter<T> : IArgumentFilter, IEquatable<IsOfTypeArgumentFilter<T>>
        {
            public static readonly IsOfTypeArgumentFilter<T> Instance = new IsOfTypeArgumentFilter<T>();

            private IsOfTypeArgumentFilter()
            {
            }

            public bool Matches(object argument)
            {
                return argument is T;
            }

            public override string ToString()
            {
                return "Is of type " + typeof(T).FullName;
            }

            public bool Equals(IsOfTypeArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsOfTypeArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
