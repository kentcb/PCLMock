namespace Kent.Boogaart.PCLMock
{
    using System;

    public static partial class It
    {
        /// <summary>
        /// Specifies that any object of type <typeparamref name="T"/> is valid.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsAny<T>()
        {
            return default(T);
        }

        internal static IArgumentFilter IsAnyFilter<T>()
        {
            return IsAnyArgumentFilter<T>.Instance;
        }

        internal sealed class IsAnyArgumentFilter<T> : IArgumentFilter, IEquatable<IsAnyArgumentFilter<T>>
        {
            public static readonly IsAnyArgumentFilter<T> Instance = new IsAnyArgumentFilter<T>();

            private IsAnyArgumentFilter()
            {
            }

            public bool Matches(object argument)
            {
                return true;
            }

            public override string ToString()
            {
                return "Is any " + typeof(T).ToString();
            }

            public bool Equals(IsAnyArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsAnyArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}
