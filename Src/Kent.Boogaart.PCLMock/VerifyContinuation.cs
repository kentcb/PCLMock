namespace Kent.Boogaart.PCLMock
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Kent.Boogaart.PCLMock.Utility;

    /// <summary>
    /// Facilitates the expression of verifications against a member in a <see cref="MockBase{T}"/>.
    /// </summary>
    public sealed class VerifyContinuation
    {
        private readonly WhenContinuationCollection whenContinuationCollection;
        private readonly ArgumentFilterCollection filters;

        internal VerifyContinuation(WhenContinuationCollection whenContinuationCollection, ArgumentFilterCollection filters)
        {
            Debug.Assert(whenContinuationCollection != null);
            Debug.Assert(filters != null);

            this.whenContinuationCollection = whenContinuationCollection;
            this.filters = filters;
        }

        /// <summary>
        /// Verifies that the member was not called.
        /// </summary>
        public void WasNotCalled()
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count > 0)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        /// <summary>
        /// Verifies that the member was called exactly one time.
        /// </summary>
        public void WasCalledExactlyOnce()
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count != 1)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        /// <summary>
        /// Verifies that the member was called one or more times.
        /// </summary>
        public void WasCalledAtLeastOnce()
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count == 0)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        /// <summary>
        /// Verifies that the member was either not called, or only called once.
        /// </summary>
        public void WasCalledAtMostOnce()
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count > 1)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        /// <summary>
        /// Verifies that the member was called exactly <paramref name="times"/> time.
        /// </summary>
        /// <param name="times">
        /// The number of times the member must have been called.
        /// </param>
        public void WasCalledExactly(int times)
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count != times)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        /// <summary>
        /// Verifies that the member was called <paramref name="times"/> or more times.
        /// </summary>
        /// <param name="times">
        /// The minimum number of times the member must have been called.
        /// </param>
        public void WasCalledAtLeast(int times)
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count < times)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        /// <summary>
        /// Verifies that the member called <paramref name="times"/> or fewer times.
        /// </summary>
        /// <param name="times">
        /// The maximum number of times the member must have been called.
        /// </param>
        public void WasCalledAtMost(int times)
        {
            var invocations = this.GetMatchingInvocations();

            if (invocations.Count > times)
            {
                ThrowVerificationException(invocations.Count);
            }
        }

        private static void ThrowVerificationException(int invocationCount)
        {
            if (invocationCount == 0)
            {
                throw new VerificationException("Member was not called, so verification has failed.");
            }
            else
            {
                throw new VerificationException(string.Format(CultureInfo.InvariantCulture, "Member was called {0} time{1}, so verification has failed.", invocationCount, invocationCount == 1 ? string.Empty : "s"));
            }
        }

        private IList<Invocation> GetMatchingInvocations()
        {
            return this
                .whenContinuationCollection.Invocations
                .Where(x => this.filters.Matches(x.Arguments))
                .ToList();
        }
    }
}