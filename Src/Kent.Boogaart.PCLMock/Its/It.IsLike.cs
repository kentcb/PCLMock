namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    public static partial class It
    {
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

        internal static IArgumentFilter IsLikeFilter(string pattern)
        {
            // I'd like to use TheHelperTrinity here, but not gonna take a dependency for one argument check
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            return new IsLikeArgumentFilter(pattern, RegexOptions.None);
        }

        internal static IArgumentFilter IsLikeFilter(string pattern, RegexOptions options)
        {
            // I'd like to use TheHelperTrinity here, but not gonna take a dependency for one argument check
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            return new IsLikeArgumentFilter(pattern, options);
        }

        private sealed class IsLikeArgumentFilter : IArgumentFilter, IEquatable<IsLikeArgumentFilter>
        {
            private readonly Regex expression;

            public IsLikeArgumentFilter(string pattern, RegexOptions options)
            {
                Debug.Assert(pattern != null);
                this.expression = new Regex(pattern, options);
            }

            public bool Matches(object argument)
            {
                if (argument == null || !(argument is string))
                {
                    return false;
                }

                return this.expression.IsMatch((string)argument);
            }

            public override string ToString()
            {
                return "Is like " + FormatValue(this.expression.ToString());
            }

            public bool Equals(IsLikeArgumentFilter other)
            {
                if (other == null)
                {
                    return false;
                }

                return
                    this.expression.Options == other.expression.Options &&
                    string.Equals(this.expression.ToString(), other.expression.ToString(), StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as IsLikeArgumentFilter);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}