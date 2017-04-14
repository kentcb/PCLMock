namespace PCLMock
{
    using System;
    using System.Text.RegularExpressions;
    using PCLMock.ArgumentFilters;

    /// <summary>
    /// Facilitates the filtering of arguments when interacting with mocks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a number of methods that can be used to filter arguments in calls to <see cref="MockBase{T}.When"/> or <see cref="MockBase{T}.Verify"/>.
    /// Consider the following example:
    /// </para>
    /// <para>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// someMock
    ///     .When(x => x.SomeMethod("abc", It.IsAny<int>(), It.IsNotNull()))
    ///     .Return(50);
    /// ]]>
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// In this case, the specification will only match if the first argument to <c>SomeMethod</c> is <c>"abc"</c> and the third argument is not <see langword="null"/>.
    /// The second argument can be anything.
    /// </para>
    /// <para>
    /// Note that the implementation of all <c>public</c> methods is simply to return a default instance of <c>T</c>. The actual implementation is resolved and applied
    /// at run-time.
    /// </para>
    /// </remarks>
    public static class It
    {
        /// <summary>
        /// Specifies that any object of type <typeparamref name="T"/> is valid.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsAny<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values that equate to <paramref name="expected"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <param name="expected">
        /// The expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T Is<T>(T expected)
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values that do not equate to <paramref name="notExpected"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the unexpected value.
        /// </typeparam>
        /// <param name="notExpected">
        /// The value that is not expected.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNot<T>(T notExpected)
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values between <paramref name="first"/> and <paramref name="second"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="first">
        /// An inclusive value representing one end of the range of values accepted.
        /// </param>
        /// <param name="second">
        /// An inclusive value representing the other end of the range of values accepted.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsBetween<T>(T first, T second)
            where T : IComparable<T>
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values not between <paramref name="first"/> and <paramref name="second"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="first">
        /// An inclusive value representing one end of the range of values not accepted.
        /// </param>
        /// <param name="second">
        /// An inclusive value representing the other end of the range of values not accepted.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNotBetween<T>(T first, T second)
            where T : IComparable<T>
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values greater than <paramref name="value"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="value">
        /// The exclusive minimum expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsGreaterThan<T>(T value)
            where T : IComparable<T>
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values less than <paramref name="value"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="value">
        /// The exclusive maximum expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsLessThan<T>(T value)
            where T : IComparable<T>
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values greater than or equal to <paramref name="value"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="value">
        /// The inclusive minimum expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsGreaterThanOrEqualTo<T>(T value)
            where T : IComparable<T>
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values less than or equal to <paramref name="value"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <param name="value">
        /// The inclusive maximum expected value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsLessThanOrEqualTo<T>(T value)
            where T : IComparable<T>
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values that are within <paramref name="expected"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <param name="expected">
        /// The set of expected values.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsIn<T>(params T[] expected)
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values that are like <paramref name="pattern"/> (a regular expression) are to be accepted.
        /// </summary>
        /// <param name="pattern">
        /// The regular expression pattern used to match values.
        /// </param>
        /// <returns>
        /// <see langword="null"/>.
        /// </returns>
        public static string IsLike(string pattern)
        {
            return null;
        }

        /// <summary>
        /// Specifies that only values that are like <paramref name="pattern"/> (a regular expression) are to be accepted.
        /// </summary>
        /// <param name="pattern">
        /// The regular expression pattern used to match values.
        /// </param>
        /// <param name="options">
        /// Optional options for the regular expression used when matching.
        /// </param>
        /// <returns>
        /// <see langword="null"/>.
        /// </returns>
        public static string IsLike(string pattern, RegexOptions options = RegexOptions.None)
        {
            return null;
        }

        /// <summary>
        /// Specifies that only values that are not like <paramref name="pattern"/> (a regular expression) are to be accepted.
        /// </summary>
        /// <param name="pattern">
        /// The regular expression pattern used to match values.
        /// </param>
        /// <returns>
        /// <see langword="null"/>.
        /// </returns>
        public static string IsNotLike(string pattern)
        {
            return null;
        }

        /// <summary>
        /// Specifies that only values that are not like <paramref name="pattern"/> (a regular expression) are to be accepted.
        /// </summary>
        /// <param name="pattern">
        /// The regular expression pattern used to match values.
        /// </param>
        /// <param name="options">
        /// Optional options for the regular expression used when matching.
        /// </param>
        /// <returns>
        /// <see langword="null"/>.
        /// </returns>
        public static string IsNotLike(string pattern, RegexOptions options = RegexOptions.None)
        {
            return null;
        }

        /// <summary>
        /// Specifies that only <see langword="null"/> values are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNull<T>()
            where T : class
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only non-<see langword="null"/> values are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNotNull<T>()
            where T : class
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values of type <typeparamref name="T"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The expected type.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsOfType<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values not of type <typeparamref name="T"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The unexpected type.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsNotOfType<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Specifies that only values for which the given predicate returns <see langword="true"/> are to be accepted.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected value.
        /// </typeparam>
        /// <param name="predicate">
        /// The predicate that will be invoked to check the value.
        /// </param>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T Matches<T>(Func<T, bool> predicate)
        {
            return default(T);
        }

        // the internal methods below must:
        //  1. have the same name as their public counterpart above, but with a "Filter" suffix
        //  2. return an implementation of IArgumentFilter that encapsulates the logic for the filter

        internal static IArgumentFilter IsAnyFilter<T>()
        {
            return IsAnyArgumentFilter<T>.Instance;
        }

        internal static IArgumentFilter IsFilter<T>(T expected)
        {
            return new IsArgumentFilter(expected);
        }

        internal static IArgumentFilter IsNotFilter<T>(T notExpected)
        {
            return new LogicalNotArgumentFilter(new IsArgumentFilter(notExpected));
        }

        internal static IArgumentFilter IsBetweenFilter<T>(T first, T second)
            where T : IComparable<T>
        {
            return new IsBetweenArgumentFilter<T>(first, second);
        }

        internal static IArgumentFilter IsNotBetweenFilter<T>(T first, T second)
            where T : IComparable<T>
        {
            return new LogicalNotArgumentFilter(new IsBetweenArgumentFilter<T>(first, second));
        }

        internal static IArgumentFilter IsGreaterThanFilter<T>(T value)
            where T : IComparable<T>
        {
            return new IsGreaterThanArgumentFilter<T>(value);
        }

        internal static IArgumentFilter IsLessThanFilter<T>(T value)
            where T : IComparable<T>
        {
            return new LogicalNotArgumentFilter(new IsGreaterThanOrEqualToArgumentFilter<T>(value));
        }

        internal static IArgumentFilter IsGreaterThanOrEqualToFilter<T>(T value)
            where T : IComparable<T>
        {
            return new IsGreaterThanOrEqualToArgumentFilter<T>(value);
        }

        internal static IArgumentFilter IsLessThanOrEqualToFilter<T>(T value)
            where T : IComparable<T>
        {
            return new LogicalNotArgumentFilter(new IsGreaterThanArgumentFilter<T>(value));
        }

        internal static IArgumentFilter IsInFilter<T>(params T[] expected)
        {
            return new IsInArgumentFilter<T>(expected);
        }

        internal static IArgumentFilter IsLikeFilter(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            return new IsLikeArgumentFilter(pattern, RegexOptions.None);
        }

        internal static IArgumentFilter IsLikeFilter(string pattern, RegexOptions options)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            return new IsLikeArgumentFilter(pattern, options);
        }

        internal static IArgumentFilter IsNotLikeFilter(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            return new LogicalNotArgumentFilter(new IsLikeArgumentFilter(pattern, RegexOptions.None));
        }

        internal static IArgumentFilter IsNotLikeFilter(string pattern, RegexOptions options)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            return new LogicalNotArgumentFilter(new IsLikeArgumentFilter(pattern, options));
        }

        internal static IArgumentFilter IsNullFilter<T>()
            where T : class
        {
            return IsNullArgumentFilter<T>.Instance;
        }

        internal static IArgumentFilter IsNotNullFilter<T>()
            where T : class
        {
            return new LogicalNotArgumentFilter(IsNullArgumentFilter<T>.Instance);
        }

        internal static IArgumentFilter IsOfTypeFilter<T>()
        {
            return IsOfTypeArgumentFilter<T>.Instance;
        }

        internal static IArgumentFilter IsNotOfTypeFilter<T>()
        {
            return new LogicalNotArgumentFilter(IsOfTypeArgumentFilter<T>.Instance);
        }

        internal static IArgumentFilter MatchesFilter<T>(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return new MatchesArgumentFilter<T>(predicate);
        }
    }
}