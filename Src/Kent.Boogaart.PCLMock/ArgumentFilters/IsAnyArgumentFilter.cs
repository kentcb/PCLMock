namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using System;

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