namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;

    public static partial class It
    {
        /// <summary>
        /// Specifies that only values not between <paramref name="first"/> and <paramref name="second"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="first">
        /// An inclusive value representing one end of the range of values not accepted.
        /// </param>
        /// <param name="second">
        /// An inclusive value representing the other end of the range of values not accepted.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNotBetween<T>(T first, T second)
            where T : IComparable<T>
        {
            return default(T);
        }

        internal static IArgumentFilter IsNotBetweenFilter<T>(T first, T second)
            where T : IComparable<T>
        {
            return new IsNotBetweenArgumentFilter<T>(first, second);
        }

        internal sealed class IsNotBetweenArgumentFilter<T> : IArgumentFilter, IEquatable<IsNotBetweenArgumentFilter<T>>
            where T : IComparable<T>
        {
            private readonly T minimum;
            private readonly T maximum;

            public IsNotBetweenArgumentFilter(T minimum, T maximum)
            {
                this.minimum = minimum;
                this.maximum = maximum;

                // to make our job easier below, we ensure that we can rely on knowing the lower end of the range versus the upper
                if (Comparer<T>.Default.Compare(this.minimum, this.maximum) > 0)
                {
                    this.minimum = maximum;
                    this.maximum = minimum;
                }
            }

            public bool Matches(object argument)
            {
                if (argument != null && !(argument is T))
                {
                    // shouldn't happen
                    return false;
                }

                return
                    Comparer<T>.Default.Compare((T)argument, this.minimum) < 0 ||
                    Comparer<T>.Default.Compare((T)argument, this.maximum) > 0;
            }

            public override string ToString()
            {
                return "Is not between " + FormatValue(this.minimum) + " and " + FormatValue(this.maximum);
            }

            public bool Equals(IsNotBetweenArgumentFilter<T> other)
            {
                if (other == null)
                {
                    return false;
                }

                return
                    Comparer<T>.Default.Compare(this.minimum, other.minimum) == 0 &&
                    Comparer<T>.Default.Compare(this.maximum, other.maximum) == 0;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsNotBetweenArgumentFilter<T>);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}