namespace PCLMock.ArgumentFilters
{
    using System;
    using System.Collections.Generic;
    using PCLMock.Utility;

    internal sealed class IsBetweenArgumentFilter<T> : IArgumentFilter, IEquatable<IsBetweenArgumentFilter<T>>
        where T : IComparable<T>
    {
        private readonly T minimum;
        private readonly T maximum;

        public IsBetweenArgumentFilter(T minimum, T maximum)
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
                Comparer<T>.Default.Compare((T)argument, this.minimum) >= 0 &&
                Comparer<T>.Default.Compare((T)argument, this.maximum) <= 0;
        }

        public override string ToString()
        {
            return "It.IsBetween(" + this.minimum.ToDebugString() + ", " + this.maximum.ToDebugString() + ")";
        }

        public bool Equals(IsBetweenArgumentFilter<T> other)
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
            return this.Equals(obj as IsBetweenArgumentFilter<T>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}