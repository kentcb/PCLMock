namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using System;
    using System.Collections.Generic;
    using Kent.Boogaart.PCLMock.Utility;

    internal sealed class IsGreaterThanOrEqualToArgumentFilter<T> : IArgumentFilter, IEquatable<IsGreaterThanOrEqualToArgumentFilter<T>>
        where T : IComparable<T>
    {
        private readonly T value;

        public IsGreaterThanOrEqualToArgumentFilter(T value)
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

            return Comparer<T>.Default.Compare((T)argument, this.value) >= 0;
        }

        public override string ToString()
        {
            return "Is greater than or equal to " + this.value.ToDebugString();
        }

        public bool Equals(IsGreaterThanOrEqualToArgumentFilter<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return Comparer<T>.Default.Compare(this.value, other.value) == 0;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IsGreaterThanOrEqualToArgumentFilter<T>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}