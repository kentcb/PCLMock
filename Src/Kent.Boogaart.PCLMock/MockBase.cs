namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Collections.ObjectModel;

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

        private readonly IDictionary<ContinuationKey, FilteredContinuationCollection> continuations;
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
            this.continuations = new Dictionary<ContinuationKey, FilteredContinuationCollection>();
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
            var continuation = new WhenContinuation<TMock>();
            this.RegisterContinuation(selector, continuation);
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
            var continuation = new WhenContinuation<TMock, TMember>();
            this.RegisterContinuation(selector, continuation);
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
            var continuation = this.GetContinuation(selector, args);

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
            var continuation = this.GetContinuation(selector, args);

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
            var continuation = this.GetContinuation(selector, null);

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
            var continuation = this.GetContinuation(selector, null);

            if (continuation == null)
            {
                return default(T);
            }

            return continuation.GetRefParameterValue<T>(parameterIndex, defaultValue);
        }

        // determines and resolves an argument filter within the specified expression
        // if an explicit filter argument is applied, this will be returned (e.g. <c>When(x => x.SomeMethod(It.IsAny<string>())</c>) would result in an appropriate argument filter)
        // if, instead, a constant is applied (e.g. <c>When(x => x.SomeMethod("hello")</c>), this will be wrapped in an appropriate argument filter
        private static IArgumentFilter FindArgumentFilterFor(Expression expression)
        {
            var argumentFilter = FindExplicitArgumentFilterFor(expression);

            if (argumentFilter != null)
            {
                return argumentFilter;
            }

            object constant;

            if (TryFindConstantWithin(expression, out constant))
            {
                return new It.IsArgumentFilter(constant);
            }

            // we never return null - instead, we return a filter that will always yield true
            return It.IsAnyArgumentFilter<object>.Instance;
        }

        // determines and resolves any explicit argument filter within the specified expression
        // for example, if the expression is It.Is("hello") then an appropriate argument filter will be returned
        private static IArgumentFilter FindExplicitArgumentFilterFor(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression == null)
            {
                return null;
            }

            var filterMethod = methodCallExpression
                .Method
                .DeclaringType
                .GetTypeInfo()
                .GetDeclaredMethods(methodCallExpression.Method.Name + "Filter")
                .Where(x => x.GetParameters().Length == methodCallExpression.Arguments.Count)
                .FirstOrDefault();

            if (filterMethod == null || filterMethod.ReturnType != typeof(IArgumentFilter))
            {
                return null;
            }

            if (methodCallExpression.Method.IsGenericMethod)
            {
                filterMethod = filterMethod.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments());
            }

            var argumentsToFilterMethod = methodCallExpression
                .Arguments
                .Select(FindConstantWithin)
                .ToArray();

            return filterMethod.Invoke(null, argumentsToFilterMethod) as IArgumentFilter;
        }

        private static object FindConstantWithin(Expression expression)
        {
            object result;

            if (!TryFindConstantWithin(expression, out result))
            {
                return null;
            }

            return result;
        }

        private static bool TryFindConstantWithin(Expression expression, out object constant)
        {
            var constantExpression = expression as ConstantExpression;

            if (constantExpression != null)
            {
                constant = constantExpression.Value;
                return true;
            }

            var memberExpression = expression as MemberExpression;

            if (memberExpression != null)
            {
                return TryFindConstantWithin(memberExpression.Expression, out constant);
            }

            constant = null;
            return false;
        }

        private static ContinuationKey GetContinuationKey(LambdaExpression selector)
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

        private static IEnumerable<IList<IArgumentFilter>> GetContinuationFilters(LambdaExpression selector)
        {
            var methodCallExpression = selector.Body as MethodCallExpression;

            if (methodCallExpression != null)
            {
                yield return methodCallExpression.Arguments
                    .Select(FindArgumentFilterFor)
                    .ToList();
                yield break;
            }

            var memberExpression = selector.Body as MemberExpression;

            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;

                if (propertyInfo != null)
                {
                    // for properties, we need to register argument filters for both the getter and setter
                    yield return new List<IArgumentFilter>();
                    yield return new List<IArgumentFilter>(new IArgumentFilter[] { It.IsAnyArgumentFilter<object>.Instance });
                }
            }

            yield return new List<IArgumentFilter>();
        }

        private void RegisterContinuation(LambdaExpression selector, WhenContinuation continuation)
        {
            var continuationKey = GetContinuationKey(selector);
            var continuationFiltersSet = GetContinuationFilters(selector);

            lock (this.continuationsSync)
            {
                foreach (var continuationFilter in continuationFiltersSet)
                {
                    var filteredContinuation = new FilteredContinuation(continuationFilter, continuation);

                    FilteredContinuationCollection filteredContinuationCollection;

                    if (!this.continuations.TryGetValue(continuationKey, out filteredContinuationCollection))
                    {
                        filteredContinuationCollection = new FilteredContinuationCollection();
                        this.continuations[continuationKey] = filteredContinuationCollection;
                    }

                    // first remove any existing filtered continuation that matches the one we're trying to add to avoid memory leaks
                    filteredContinuationCollection.Remove(filteredContinuation);
                    filteredContinuationCollection.Add(filteredContinuation);
                }
            }
        }

        private WhenContinuation GetContinuation(LambdaExpression selector, object[] args)
        {
            return this.GetContinuation(GetContinuationKey(selector), args);
        }

        private WhenContinuation GetContinuation(ContinuationKey continuationKey, object[] args)
        {
            FilteredContinuationCollection filteredContinuationCollection;
            WhenContinuation continuation = null;

            lock (this.continuationsSync)
            {
                if (this.continuations.TryGetValue(continuationKey, out filteredContinuationCollection))
                {
                    // we loop backwards so that more recently registered continuations take a higher precedence
                    for (var i = filteredContinuationCollection.Count - 1; i >= 0; --i)
                    {
                        var filteredContinuation = filteredContinuationCollection[i];

                        if (args == null || filteredContinuation.Filters == null || ArgumentsMatchFilters(args, filteredContinuation.Filters)){
                            continuation = filteredContinuation.Continuation;
                            break;
                        }
                    }
                }

                if (continuation == null)
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

        // determine whether the given arguments match corresponding filters
        private static bool ArgumentsMatchFilters(object[] args, IList<IArgumentFilter> filters)
        {
            Debug.Assert(args != null);
            Debug.Assert(filters != null);

            if (args.Length != filters.Count)
            {
                return false;
            }

            for (var i = 0; i < args.Length; ++i)
            {
                var filter = filters[i];

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

        private enum ContinuationKeyType
        {
            Method,
            Property
        }

        private struct ContinuationKey : IEquatable<ContinuationKey>
        {
            private readonly MemberInfo memberInfo;

            public ContinuationKey(MemberInfo memberInfo)
            {
                this.memberInfo = memberInfo;
            }

            public ContinuationKeyType Type
            {
                get { return this.memberInfo is MethodInfo ? ContinuationKeyType.Method : ContinuationKeyType.Property; }
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

        // a continuation associated with a set of filters to be applied to arguments in order to determine whether the continuation should be used
        private sealed class FilteredContinuation : IEquatable<FilteredContinuation>
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

        // a collection of FilteredContinuation objects
        private sealed class FilteredContinuationCollection : Collection<FilteredContinuation>
        {
            private readonly IList<FilteredContinuation> items;

            public FilteredContinuationCollection()
            {
                this.items = new List<FilteredContinuation>();
            }
        }
    }
}