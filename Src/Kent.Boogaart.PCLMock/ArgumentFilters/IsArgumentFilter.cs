namespace Kent.Boogaart.PCLMock.ArgumentFilters
{
    using System;
    using Kent.Boogaart.PCLMock.Utility;

    internal sealed class IsArgumentFilter : IArgumentFilter, IEquatable<IsArgumentFilter>
    {
        private readonly object expected;

        public IsArgumentFilter(object expected)
        {
            this.expected = expected;
        }

        public bool Matches(object argument)
        {
            return object.Equals(argument, this.expected);
        }

        public override string ToString()
        {
            return "Is " + this.expected.ToDebugString();
        }

        public bool Equals(IsArgumentFilter other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Matches(other.expected);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IsArgumentFilter);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}