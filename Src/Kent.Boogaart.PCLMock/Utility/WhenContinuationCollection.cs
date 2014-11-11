namespace Kent.Boogaart.PCLMock.Utility
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    // a collection of WhenContinuation objects and a record of all invocations therein
    internal sealed class WhenContinuationCollection : Collection<WhenContinuation>
    {
        private readonly List<Invocation> invocations;
        private readonly object sync;

        public WhenContinuationCollection()
        {
            this.invocations = new List<Invocation>();
            this.sync = new object();
        }

        public IReadOnlyList<Invocation> Invocations
        {
            get { return this.invocations; }
        }

        public void AddInvocation(Invocation invocation)
        {
            lock (this.sync)
            {
                this.invocations.Add(invocation);
            }
        }

        public WhenContinuation FindWhenContinuation(object[] args)
        {
            // we loop backwards so that more recently registered continuations take a higher precedence
            for (var i = this.Count - 1; i >= 0; --i)
            {
                var continuation = this[i];

                if (args == null || continuation.Filters == null || continuation.Filters.Matches(args))
                {
                    return continuation;
                }
            }

            return null;
        }
    }
}