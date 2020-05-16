namespace PCLMock
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;
    using PCLMock.Utility;
    using PCLMock.Visitors;

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

        private static readonly object[] emptyArgs = new object[0];
        private static readonly ArgumentFilterCollection emptyArgumentFilters = new ArgumentFilterCollection();

        private readonly IDictionary<ContinuationKey, WhenContinuationCollection> continuations;
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
            this.continuations = new Dictionary<ContinuationKey, WhenContinuationCollection>();
            this.continuationsSync = new object();
            this.behavior = behavior;
        }

        /// <summary>
        /// Gets the behavior of this mock.
        /// </summary>
        public MockBehavior Behavior => this.behavior;

        /// <summary>
        /// Gets the mocked object.
        /// </summary>
        /// <remarks>
        /// It is not typically necessary to override this property unless mocking a class.
        /// </remarks>
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
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            var continuation = new WhenContinuation<TMock>(selector, ArgumentFiltersVisitor.FindArgumentFiltersWithin(selector.Body) ?? emptyArgumentFilters);
            this.AddOrReplaceWhenContinuation(selector, continuation);
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
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            var continuation = new WhenContinuation<TMock, TMember>(selector, ArgumentFiltersVisitor.FindArgumentFiltersWithin(selector.Body) ?? emptyArgumentFilters);
            this.AddOrReplaceWhenContinuation(selector, continuation);
            return continuation;
        }

        /// <summary>
        /// Begins the specification of what the mock should do when a given property is set.
        /// </summary>
        /// <typeparam name="TMember">
        /// The type of the property.
        /// </typeparam>
        /// <param name="propertySelector">
        /// An expression that resolves the property.
        /// </param>
        /// <param name="valueFilterSelector">
        /// An optional expression that can provide filtering against the property value being set.
        /// </param>
        /// <returns>
        /// A continuation object with which the actions to be performed can be configured.
        /// </returns>
        public WhenContinuation<TMock, TMember> WhenPropertySet<TMember>(Expression<Func<TMock, TMember>> propertySelector, Expression<Func<TMember>> valueFilterSelector = null)
        {
            if (propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }

            if (valueFilterSelector == null)
            {
                valueFilterSelector = () => It.IsAny<TMember>();
            }

            var filters = new ArgumentFilterCollection();
            filters.Add(ArgumentFilterVisitor.FindArgumentFilterWithin(valueFilterSelector.Body));
            var continuation = new WhenContinuation<TMock, TMember>(propertySelector, filters);
            this.AddOrReplaceWhenContinuation(propertySelector, continuation);
            return continuation;
        }

        /// <summary>
        /// Begins a verification specification.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the member being verified.
        /// </param>
        /// <returns>
        /// A continuation object with which the verification can be specified.
        /// </returns>
        public VerifyContinuation Verify(Expression<Action<TMock>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            var whenContinuationCollection = this.EnsureWhenContinuationCollection(selector);
            return new VerifyContinuation(selector, whenContinuationCollection, ArgumentFiltersVisitor.FindArgumentFiltersWithin(selector.Body) ?? emptyArgumentFilters);
        }

        /// <summary>
        /// Begins a verification specification.
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="selector">
        /// An expression that resolves to the member being verified.
        /// </param>
        /// <returns>
        /// A continuation object with which the verification can be specified.
        /// </returns>
        public VerifyContinuation Verify<TMember>(Expression<Func<TMock, TMember>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            var whenContinuationCollection = this.EnsureWhenContinuationCollection(selector);
            return new VerifyContinuation(selector, whenContinuationCollection, ArgumentFiltersVisitor.FindArgumentFiltersWithin(selector.Body) ?? emptyArgumentFilters);
        }

        /// <summary>
        /// Begins a verification specification for a property set.
        /// </summary>
        /// <typeparam name="TMember">
        /// The type of the property.
        /// </typeparam>
        /// <param name="propertySelector">
        /// An expression that resolves to the property being verified.
        /// </param>
        /// <param name="valueFilterSelector">
        /// An optional expression that can provide filtering against the property value being set.
        /// </param>
        /// <returns>
        /// A continuation object with which the verification can be specified.
        /// </returns>
        public VerifyContinuation VerifyPropertySet<TMember>(Expression<Func<TMock, TMember>> propertySelector, Expression<Func<TMember>> valueFilterSelector = null)
        {
            if (propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }

            if (valueFilterSelector == null)
            {
                valueFilterSelector = () => It.IsAny<TMember>();
            }

            var whenContinuationCollection = this.EnsureWhenContinuationCollection(propertySelector);
            var filters = new ArgumentFilterCollection();
            filters.Add(ArgumentFilterVisitor.FindArgumentFilterWithin(valueFilterSelector.Body));
            return new VerifyContinuation(propertySelector, whenContinuationCollection, filters);
        }

        /// <summary>
        /// Applies any specifications configured via <see cref="When"/> for a given member.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the member.
        /// </param>
        protected void Apply(Expression<Action<TMock>> selector)
        {
            var args = ArgumentsVisitor.FindArgumentsWithin(selector.Body) ?? emptyArgs;
            WhenContinuationCollection whenContinuationCollection;
            var continuation = this.GetWhenContinuation(selector, args, out whenContinuationCollection);
            whenContinuationCollection.AddInvocation(new Invocation(args));

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
        protected TMember Apply<TMember>(Expression<Func<TMock, TMember>> selector)
        {
            var args = ArgumentsVisitor.FindArgumentsWithin(selector.Body) ?? emptyArgs;
            WhenContinuationCollection whenContinuationCollection;
            var continuation = this.GetWhenContinuation(selector, args, out whenContinuationCollection);
            whenContinuationCollection.AddInvocation(new Invocation(args));

            if (continuation == null)
            {
                return default(TMember);
            }

            return (TMember)continuation.Apply(this.MockedObject, args);
        }

        /// <summary>
        /// Applies any specifications configured via <see cref="WhenPropertySet{TMember}"/> for a given property setter.
        /// </summary>
        /// <param name="selector">
        /// An expression that resolves to the property being set.
        /// </param>
        /// <param name="value">
        /// The value being assigned to the property.
        /// </param>
        protected void ApplyPropertySet<TMember>(Expression<Func<TMock, TMember>> selector, object value)
        {
            // base arguments would be any indexers to the property
            var indexerArgs = ArgumentsVisitor.FindArgumentsWithin(selector.Body) ?? emptyArgs;
            var args = new object[indexerArgs.Length + 1];
            Array.Copy(indexerArgs, args, indexerArgs.Length);
            args[args.Length - 1] = value;

            WhenContinuationCollection whenContinuationCollection;
            var continuation = this.GetWhenContinuation(selector, args, out whenContinuationCollection);
            whenContinuationCollection.AddInvocation(new Invocation(args));

            if (continuation == null)
            {
                return;
            }

            continuation.Apply(this.MockedObject, args);
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
            var continuation = this.GetWhenContinuation(selector, null);

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
            var continuation = this.GetWhenContinuation(selector, null);

            if (continuation == null)
            {
                return default(T);
            }

            return continuation.GetRefParameterValue<T>(parameterIndex, defaultValue);
        }

        private static ContinuationKey GetContinuationKey(LambdaExpression selector)
        {
            var methodCallExpression = selector.Body as MethodCallExpression;

            if (methodCallExpression != null)
            {
                if (methodCallExpression.Object == null)
                {
                    throw new InvalidOperationException("Specifications against extension methods cannot be provided: " + methodCallExpression);
                }

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

        private WhenContinuationCollection EnsureWhenContinuationCollection(LambdaExpression memberSelector)
        {
            var continuationKey = GetContinuationKey(memberSelector);

            lock (this.continuationsSync)
            {
                WhenContinuationCollection result;

                if (!this.continuations.TryGetValue(continuationKey, out result))
                {
                    result = new WhenContinuationCollection();
                    this.continuations[continuationKey] = result;
                }

                return result;
            }
        }

        private void AddOrReplaceWhenContinuation(LambdaExpression memberSelector, WhenContinuation continuation)
        {
            lock (this.continuationsSync)
            {
                var whenContinuationCollection = this.EnsureWhenContinuationCollection(memberSelector);

                // first remove any existing filtered continuation that matches the one we're trying to add to avoid memory leaks
                whenContinuationCollection.Remove(continuation);
                whenContinuationCollection.Add(continuation);
            }
        }

        private WhenContinuation GetWhenContinuation(LambdaExpression memberSelector, object[] args)
        {
            WhenContinuationCollection whenContinuationCollection;
            return this.GetWhenContinuation(memberSelector, args, out whenContinuationCollection);
        }

        private WhenContinuation GetWhenContinuation(LambdaExpression memberSelector, object[] args, out WhenContinuationCollection whenContinuationCollection)
        {
            var continuationKey = GetContinuationKey(memberSelector);

            lock (this.continuationsSync)
            {
                whenContinuationCollection = this.EnsureWhenContinuationCollection(memberSelector);
                var whenContinuation = whenContinuationCollection.FindWhenContinuation(args);

                if (whenContinuation == null && this.behavior == MockBehavior.Strict)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, noContinuationErrorTemplate, continuationKey.Type, continuationKey.MemberInfo.Name));
                }

                return whenContinuation;
            }
        }
    }
}