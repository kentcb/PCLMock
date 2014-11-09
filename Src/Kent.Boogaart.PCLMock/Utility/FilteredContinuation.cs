namespace Kent.Boogaart.PCLMock.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    // a continuation associated with a set of filters to be applied to arguments in order to determine whether the continuation should be used
    internal sealed class FilteredContinuation : IEquatable<FilteredContinuation>
    {
        private readonly IList<IArgumentFilter> filters;
        private readonly WhenContinuation continuation;

        public FilteredContinuation(IList<IArgumentFilter> filters, WhenContinuation continuation)
        {
            Debug.Assert(filters != null);
            Debug.Assert(continuation != null);

            this.filters = filters;
            this.continuation = continuation;
        }

        public IList<IArgumentFilter> Filters
        {
            get { return this.filters; }
        }

        public WhenContinuation Continuation
        {
            get { return this.continuation; }
        }

        public bool Matches(object[] args)
        {
            Debug.Assert(args != null);

            if (args.Length != this.filters.Count)
            {
                return false;
            }

            for (var i = 0; i < args.Length; ++i)
            {
                var filter = this.filters[i];

                if (filter == null)
                {
                    continue;
                }

                if (!filter.Matches(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // a FilteredContinuation is considered equal to another if the filters match exactly (the continuation itself doesn't matter)
        public bool Equals(FilteredContinuation other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.filters.Count != other.filters.Count)
            {
                return false;
            }

            for (var i = 0; i < this.filters.Count; ++i)
            {
                Debug.Assert(this.filters[i] != null);
                Debug.Assert(other.filters[i] != null);

                // filters implement equality semantics, so this is totally OK
                if (!this.filters[i].Equals(other.filters[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FilteredContinuation);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
