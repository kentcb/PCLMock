namespace PCLMock.ArgumentFilters
{
    using System;
    using PCLMock.Utility;

    internal sealed class IsOfTypeArgumentFilter<T> : IArgumentFilter, IEquatable<IsOfTypeArgumentFilter<T>>
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
            return "It.IsOfType<" + typeof(T).ToDebugString() + ">()";
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