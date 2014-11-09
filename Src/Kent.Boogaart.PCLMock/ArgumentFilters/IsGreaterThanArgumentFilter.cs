namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using System;
    using System.Collections.Generic;
    using Kent.Boogaart.PCLMock.Utility;

    internal sealed class IsGreaterThanArgumentFilter<T> : IArgumentFilter, IEquatable<IsGreaterThanArgumentFilter<T>>
        where T : IComparable<T>
    {
        private readonly T value;

        public IsGreaterThanArgumentFilter(T value)
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

            return Comparer<T>.Default.Compare((T)argument, this.value) > 0;
        }

        public override string ToString()
        {
            return "Is greater than " + this.value.ToDebugString();
        }

        public bool Equals(IsGreaterThanArgumentFilter<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return Comparer<T>.Default.Compare(this.value, other.value) == 0;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IsGreaterThanArgumentFilter<T>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}