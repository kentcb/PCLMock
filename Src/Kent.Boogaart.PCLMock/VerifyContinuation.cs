namespace Kent.Boogaart.PCLMock
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Kent.Boogaart.PCLMock.Utility;
    using System.Linq.Expressions;
    using Kent.Boogaart.PCLMock.Visitors;

    /// <summary>
    /// Facilitates the expression of verifications against a member in a <see cref="MockBase{T}"/>.
    /// </summary>
    public sealed class VerifyContinuation
    {
        private readonly Expression selector;
        private readonly WhenContinuationCollection whenContinuationCollection;
        private readonly ArgumentFilterCollection filters;

        internal VerifyContinuation(Expression selector, WhenContinuationCollection whenContinuationCollection, ArgumentFilterCollection filters)
        {
            Debug.Assert(selector != null);
            Debug.Assert(whenContinuationCollection != null);
            Debug.Assert(filters != null);

            this.selector = selector;
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
                ThrowVerificationException(
                    "Verification that {0} was not called failed because it was called {1} time{2}.",
                    this.GetSelectorString(),
                    invocations.Count,
                    invocations.Count == 1 ? string.Empty : "s");
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
                ThrowVerificationException(
                    "Verification that {0} was called exactly once failed because it was called {1} time{2}.",
                    this.GetSelectorString(),
                    invocations.Count,
                    invocations.Count == 1 ? string.Empty : "s");
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
                ThrowVerificationException(
                    "Verification that {0} was called at least once failed because it was called 0 times.",
                    this.GetSelectorString());
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
                ThrowVerificationException(
                    "Verification that {0} was called at most once failed because it was called {1} times.",
                    this.GetSelectorString(),
                    invocations.Count);
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
                ThrowVerificationException(
                    "Verification that {0} was called exactly {1} time{2} failed because it was called {3} time{4}.",
                    this.GetSelectorString(),
                    times,
                    times == 1 ? string.Empty : "s",
                    invocations.Count,
                    invocations.Count == 1 ? string.Empty : "s");
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
                ThrowVerificationException(
                    "Verification that {0} was called at least {1} time{2} failed because it was called {3} time{4}.",
                    this.GetSelectorString(),
                    times,
                    times == 1 ? string.Empty : "s",
                    invocations.Count,
                    invocations.Count == 1 ? string.Empty : "s");
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
                ThrowVerificationException(
                    "Verification that {0} was called at most {1} time{2} failed because it was called {3} time{4}.",
                    this.GetSelectorString(),
                    times,
                    times == 1 ? string.Empty : "s",
                    invocations.Count,
                    invocations.Count == 1 ? string.Empty : "s");
            }
        }

        private string GetSelectorString()
        {
            var visitor = new SelectorStringVisitor();
            visitor.Visit(this.selector);
            return visitor.ToString();
        }

        private static void ThrowVerificationException(string format, params object[] args)
        {
            throw new VerificationException(string.Format(CultureInfo.InvariantCulture, format, args));
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