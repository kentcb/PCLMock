namespace PCLMock.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    internal static class ToDebugStringExtensions
    {
        private static readonly IDictionary<Type, string> typeToNameMappings = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(object), "object" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" }
        };

        public static string ToDebugString(this Type @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("@this");
            }

            string result;

            if (typeToNameMappings.TryGetValue(@this, out result))
            {
                return result;
            }

            var underlyingNullableType = Nullable.GetUnderlyingType(@this);

            if (underlyingNullableType != null)
            {
                return underlyingNullableType.ToDebugString() + "?";
            }

            return @this.FullName;
        }

        public static string ToDebugString(this object @this)
        {
            if (@this == null)
            {
                return "null";
            }

            if (@this is Type)
            {
                return ((Type)@this).ToDebugString();
            }

            if (@this is string)
            {
                return "\"" + @this + "\"";
            }

            if (@this is bool)
            {
                return @this.ToString().ToLowerInvariant();
            }

            if (@this is int)
            {
                return @this.ToString();
            }

            if (@this is uint)
            {
                return @this.ToString() + "U";
            }

            if (@this is long)
            {
                return @this.ToString() + "L";
            }

            if (@this is ulong)
            {
                return @this.ToString() + "UL";
            }

            if (@this is float)
            {
                return ((float)@this).ToString(CultureInfo.InvariantCulture) + "F";
            }

            if (@this is double)
            {
                return ((double)@this).ToString(CultureInfo.InvariantCulture) + "D";
            }

            if (@this is decimal)
            {
                return ((decimal)@this).ToString(CultureInfo.InvariantCulture) + "M";
            }

            if (@this is Enum)
            {
                var @enum = (Enum)@this;
                var isFlags = @this.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
                var result = new StringBuilder();

                if (isFlags)
                {
                    foreach (Enum value in Enum.GetValues(@this.GetType()))
                    {
                        if (@enum.HasFlag(value))
                        {
                            if (result.Length != 0)
                            {
                                result.Append(" | ");
                            }

                            result.Append(@this.GetType().Name).Append(".").Append(value);
                        }
                    }

                    return result.ToString();
                }

                return @this.GetType().Name + "." + @this;
            }

            return @this.ToString() + " [" + @this.GetType().ToDebugString() + "]";
        }
    }
}