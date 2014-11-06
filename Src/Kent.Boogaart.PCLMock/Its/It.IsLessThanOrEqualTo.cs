namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values less than or equal to <paramref name="value"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="value">
        /// The inclusive maximum expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsLessThanOrEqualTo<T>(T value)
            where T : IComparable<T>
        {
            return default(T);
        }

        internal static IArgumentFilter IsLessThanOrEqualToFilter<T>(T value)
            where T : IComparable<T>
        {
            return new IsLessThanOrEqualToArgumentFilter<T>(value);
        }

        internal sealed class IsLessThanOrEqualToArgumentFilter<T> : IArgumentFilter, IEquatable<IsLessThanOrEqualToArgumentFilter<T>>
            where T : IComparable<T>
        {
            private readonly T value;

            public IsLessThanOrEqualToArgumentFilter(T value)
            {
                this.value = value;
            }

            public bool Matches(object argument)
            {
                if (argument != null && !(argument is T))
                {
                    // shouldn't happen
                    return false;
                }

                return Comparer<T>.Default.Compare((T)argument, this.value) <= 0;
            }

            public override string ToString()
            {
                return "Is less than or equal to " + FormatValue(this.value);
            }

            public bool Equals(IsLessThanOrEqualToArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return Comparer<T>.Default.Compare(this.value, other.value) == 0;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsLessThanOrEqualToArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}