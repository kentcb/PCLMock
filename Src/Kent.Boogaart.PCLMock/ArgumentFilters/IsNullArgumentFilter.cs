namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using Kent.Boogaart.PCLMock.Utility;
    using System;

    internal sealed class IsNullArgumentFilter<T> : IArgumentFilter, IEquatable<IsNullArgumentFilter<T>>
    {
        public static readonly IsNullArgumentFilter<T> Instance = new IsNullArgumentFilter<T>();

        private IsNullArgumentFilter()
        {
        }

        public bool Matches(object argument)
        {
            return argument == null;
        }

        public override string ToString()
        {
            return "It.IsNull<" + typeof(T).ToDebugString() + ">()";
        }

        public bool Equals(IsNullArgumentFilter<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IsNullArgumentFilter<T>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}