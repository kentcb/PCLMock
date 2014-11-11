namespace Kent.Boogaart.PCLMock.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    // a collection of IArgumentFilter objects
    internal sealed class ArgumentFilterCollection : Collection<IArgumentFilter>, IEquatable<ArgumentFilterCollection>
    {
        public ArgumentFilterCollection()
        {
        }

        public ArgumentFilterCollection(IEnumerable<IArgumentFilter> filters)
            : base(filters.ToList())
        {
        }

        public bool Matches(object[] args)
        {
            Debug.Assert(args != null);

            if (args.Length != this.Count)
            {
                return false;
            }

            for (var i = 0; i < args.Length; ++i)
            {
                var filter = this[i];

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

        public bool Equals(ArgumentFilterCollection other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            for (var i = 0; i < this.Count; ++i)
            {
                Debug.Assert(this[i] != null);
                Debug.Assert(other[i] != null);

                // filters implement equality semantics, so this is totally OK
                if (!this[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ArgumentFilterCollection);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}