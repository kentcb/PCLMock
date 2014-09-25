namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;

    // a base class that makes it easier to create mock objects
    public abstract class MockBase<TMock>
    {
        private readonly IDictionary<object, WhenContinuation> continuations;
        private readonly object continuationsSync;
        private readonly MockBehavior behavior;

        protected MockBase(MockBehavior behavior)
        {
            this.continuations = new Dictionary<object, WhenContinuation>();
            this.continuationsSync = new object();
            this.behavior = behavior;
        }

        public MockBehavior Behavior
        {
            get { return this.behavior; }
        }

        public abstract TMock MockedObject
        {
            get;
        }

        public WhenContinuation<TMock> When(Expression<Action<TMock>> selector)
        {
            var continuationKey = this.GetContinuationKey(selector);
            var continuation = new WhenContinuation<TMock>();

            lock (this.continuationsSync)
            {
                this.continuations[continuationKey] = continuation;
            }

            return continuation;
        }

        public WhenContinuation<TMock, TMember> When<TMember>(Expression<Func<TMock, TMember>> selector)
        {
            var continuationKey = this.GetContinuationKey(selector);
            var continuation = new WhenContinuation<TMock, TMember>();

            lock (this.continuationsSync)
            {
                this.continuations[continuationKey] = continuation;
            }

            return continuation;
        }

        protected void Apply(Expression<Action<TMock>> selector, params object[] args)
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return;
            }

            continuation.Apply(this.MockedObject, args);
        }

        protected TMember Apply<TMember>(Expression<Func<TMock, TMember>> selector, params object[] args)
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return default(TMember);
            }

            return (TMember)continuation.Apply(this.MockedObject, args);
        }

        protected T GetOutParameterValue<T>(Expression<Action<TMock>> selector, int parameterIndex)
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return default(T);
            }

            return continuation.GetOutParameterValue<T>(parameterIndex);
        }

        protected T GetRefParameterValue<T>(Expression<Action<TMock>> selector, int parameterIndex, T defaultValue = default(T))
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return default(T);
            }

            return continuation.GetRefParameterValue<T>(parameterIndex, defaultValue);
        }

        private object GetContinuationKey(LambdaExpression selector)
        {
            var methodCallExpression = selector.Body as MethodCallExpression;

            if (methodCallExpression != null)
            {
                return methodCallExpression.Method;
            }

            var memberExpression = selector.Body as MemberExpression;

            if (memberExpression != null)
            {
                return memberExpression.Member;
            }

            throw new InvalidOperationException("Unable to determine the details of the member being mocked.");
        }

        private WhenContinuation GetContinuation(LambdaExpression selector)
        {
            return this.GetContinuation(this.GetContinuationKey(selector));
        }

        private WhenContinuation GetContinuation(object continuationKey)
        {
            WhenContinuation continuation;

            lock (this.continuationsSync)
            {
                if (!this.continuations.TryGetValue(continuationKey, out continuation))
                {
                    if (this.behavior == MockBehavior.Loose)
                    {
                        return null;
                    }

                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not apply selector '{0}' because no continuation was found for it.", continuationKey));
                }
            }

            return continuation;
        }
    }
}