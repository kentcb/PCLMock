namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Provides a simple means of obtaining a value of any type. Useful when configuring expectations against method calls where the methods take arguments.
    /// </summary>
    public static partial class It
    {
        private static string FormatValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string)
            {
                return "\"" + value + "\"";
            }

            if (value is bool)
            {
                return value.ToString();
            }

            if (value is int)
            {
                return value.ToString();
            }

            if (value is uint)
            {
                return value.ToString() + "U";
            }

            if (value is long)
            {
                return value.ToString() + "L";
            }

            if (value is ulong)
            {
                return value.ToString() + "UL";
            }

            if (value is float)
            {
                return value.ToString() + "F";
            }

            if (value is double)
            {
                return value.ToString() + "D";
            }

            if (value is decimal)
            {
                return value.ToString() + "M";
            }

            return value.ToString() + " (" + value.GetType().Name + ")";
        }
    }
}