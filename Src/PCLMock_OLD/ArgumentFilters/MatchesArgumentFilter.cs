namespace PCLMock.ArgumentFilters
{
    using System;
    using System.Diagnostics;
    using PCLMock.Utility;

    internal sealed class MatchesArgumentFilter<T> : IArgumentFilter, IEquatable<MatchesArgumentFilter<T>>
    {
        private readonly Func<T, bool> predicate;

        public MatchesArgumentFilter(Func<T, bool> predicate)
        {
            Debug.Assert(predicate != null);

            this.predicate = predicate;
        }

        public bool Matches(object argument)
        {
            if (argument != null && !(argument is T))
            {
                // shouldn't happen
                return false;
            }

            return this.predicate((T)argument);
        }

        public override string ToString()
        {
            return "It.Matches<" + typeof(T).ToDebugString() + ">(<predicate>)";
        }

        public bool Equals(MatchesArgumentFilter<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return this.predicate == other.predicate;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MatchesArgumentFilter<T>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}