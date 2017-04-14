namespace PCLMock.ArgumentFilters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    internal sealed class LogicalNotArgumentFilter : IArgumentFilter, IEquatable<LogicalNotArgumentFilter>
    {
        private readonly IArgumentFilter child;

        public LogicalNotArgumentFilter(IArgumentFilter child)
        {
            Debug.Assert(child != null);
            this.child = child;
        }

        public bool Matches(object argument)
        {
            return !this.child.Matches(argument);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "NOT({0})", this.child);
        }

        public bool Equals(LogicalNotArgumentFilter other)
        {
            if (other == null)
            {
                return false;
            }

            return this.child.Equals(other.child);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LogicalNotArgumentFilter);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}