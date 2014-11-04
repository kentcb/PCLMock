namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// A base class from which mock objects must be derived.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mock objects should derive from this base class. Typically, you will want to mock an interface. Please see the online documentation for more information.
    /// </para>
    /// </remarks>
    /// <typeparam name="TMock">
    /// The type being mocked.
    /// </typeparam>
    public abstract class MockBase<TMock>
    {
        private const string mockedObjectErrorTemplate = @"The default implementation of MockBase<{0}> is unable to automatically determine an instance of type {0} to be used as the mocked object. You should override MockedObject in {1} and return the mocked object.
Full mock type name: {2}
Full mocked object type name: {3}";

        private const string noContinuationErrorTemplate = @"{0} '{1}', for which no specifications have been configured, was invoked on a strict mock. You must either configure specifications via calls to When on the mock, or use a loose mock by passing in MockBehavior.Loose to the mock's constructor.";

        private readonly IDictionary<ContinuationKey, WhenContinuation> continuations;
        private readonly object continuationsSync;
        private readonly MockBehavior behavior;

        /// <summary>
        /// Initializes a new instance of the MockBase class.
        /// </summary>
        /// <param name="behavior">
        /// The behavior to be used by this mock.
        /// </param>
        protected MockBase(MockBehavior behavior)
        {
            this.continuations = new Dictionary<ContinuationKey, WhenContinuation>();
            this.continuationsSync = new object();
            this.behavior = behavior;
        }

        /// <summary>
        /// Gets the behavior of this mock.
        /// </summary>
        public MockBehavior Behavior
        {
            get { return this.behavior; }
        }

        /// <summary>
        /// Gets the mocked object.
        /// </summary>
        public virtual TMock MockedObject
        {
            get
            {
                object toCast = this;

                if (!(toCast is TMock))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            mockedObjectErrorTemplate,
                            typeof(TMock).Name,
                            this.GetType().Name,
                            this.GetType().FullName,
                            typeof(TMock).FullName));
                }

                return (TMock)toCast;
            }
        }

        /// <summary>
        /// Begins the specification of what the mock should do when a given member is accessed.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the member.
        /// </param>
        /// <returns>
        /// A continuation object with which the actions to be performed can be configured.
        /// </returns>
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
        /// <summary>
        /// Begins the specification of what the mock should do when a given member is accessed.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the member.
        /// </param>
        /// <returns>
        /// A continuation object with which the actions to be performed can be configured.
        /// </returns>

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

        /// <summary>
        /// Applies any specifications configured via <see cref="When"/> for a given member.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the member.
        /// </param>
        /// <param name="args">
        /// Any arguments passed in when the member was invoked.
        /// </param>
        protected void Apply(Expression<Action<TMock>> selector, params object[] args)
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return;
            }

            continuation.Apply(this.MockedObject, args);
        }

        /// <summary>
        /// Applies any specifications configured via <see cref="When"/> for a given member.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the member.
        /// </param>
        /// <param name="args">
        /// Any arguments passed in when the member was invoked.
        /// </param>
        protected TMember Apply<TMember>(Expression<Func<TMock, TMember>> selector, params object[] args)
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return default(TMember);
            }

            return (TMember)continuation.Apply(this.MockedObject, args);
        }

        /// <summary>
        /// Gets the value for an <c>out</c> parameter for a given method at a given parameter index.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the <c>out</c> parameter.
        /// </typeparam>
        /// <param name="selector">
        /// An expression that resolves to the method.
        /// </param>
        /// <param name="parameterIndex">
        /// The zero-based index of the parameter.
        /// </param>
        /// <returns>
        /// The value assigned to that <c>out</c> parameter.
        /// </returns>
        protected T GetOutParameterValue<T>(Expression<Action<TMock>> selector, int parameterIndex)
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return default(T);
            }

            return continuation.GetOutParameterValue<T>(parameterIndex);
        }
        /// <summary>
        /// Gets the value for a <c>ref</c> parameter for a given method at a given parameter index.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the <c>ref</c> parameter.
        /// </typeparam>
        /// <param name="selector">
        /// An expression that resolves to the method.
        /// </param>
        /// <param name="parameterIndex">
        /// The zero-based index of the parameter.
        /// </param>
        /// <param name="defaultValue">
        /// An optional default value for the <c>ref</c> parameter, which defaults to the default value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// The value assigned to that <c>ref</c> parameter.
        /// </returns>

        protected T GetRefParameterValue<T>(Expression<Action<TMock>> selector, int parameterIndex, T defaultValue = default(T))
        {
            var continuation = this.GetContinuation(selector);

            if (continuation == null)
            {
                return default(T);
            }

            return continuation.GetRefParameterValue<T>(parameterIndex, defaultValue);
        }

        private ContinuationKey GetContinuationKey(LambdaExpression selector)
        {
            var methodCallExpression = selector.Body as MethodCallExpression;

            if (methodCallExpression != null)
            {
                if (methodCallExpression.Object.NodeType != ExpressionType.Parameter)
                {
                    throw new InvalidOperationException("Specifications against methods cannot be chained: " + methodCallExpression);
                }

                return new ContinuationKey(methodCallExpression.Method);
            }
            
            var memberExpression = selector.Body as MemberExpression;

            if (memberExpression != null)
            {
                if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
                {
                    throw new InvalidOperationException("Specifications against properties cannot be chained: " + memberExpression);
                }

                return new ContinuationKey(memberExpression.Member);
            }

            throw new InvalidOperationException("Unable to determine the details of the member being mocked.");
        }

        private WhenContinuation GetContinuation(LambdaExpression selector)
        {
            return this.GetContinuation(this.GetContinuationKey(selector));
        }

        private WhenContinuation GetContinuation(ContinuationKey continuationKey)
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

                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, noContinuationErrorTemplate, continuationKey.Type, continuationKey.MemberInfo.Name));
                }
            }

            return continuation;
        }

        private enum ContinuationKeyType
        {
            Method,
            Property
        }

        private struct ContinuationKey : IEquatable<ContinuationKey>
        {
            private readonly ContinuationKeyType type;
            private readonly MemberInfo memberInfo;

            public ContinuationKey(MemberInfo memberInfo)
            {
                this.type = memberInfo is MethodInfo ? ContinuationKeyType.Method : ContinuationKeyType.Property;
                this.memberInfo = memberInfo;
            }

            public ContinuationKeyType Type
            {
                get { return this.type; }
            }

            public MemberInfo MemberInfo
            {
                get { return this.memberInfo; }
            }

            public bool Equals(ContinuationKey other)
            {
                Debug.Assert(this.memberInfo != null);
                Debug.Assert(other.memberInfo != null);

                return other.memberInfo.Equals(this.memberInfo);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ContinuationKey))
                {
                    return false;
                }

                return this.Equals((ContinuationKey)obj);
            }

            public override int GetHashCode()
            {
                Debug.Assert(this.memberInfo != null);
                return this.memberInfo.GetHashCode();
            }
        }
    }
}