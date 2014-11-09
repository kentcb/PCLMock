namespace Kent.Boogaart.PCLMock.Utility
{
    internal static class ObjectExtensions
    {
        public static string ToDebugString(this object @this)
        {
            if (@this == null)
            {
                return "null";
            }

            if (@this is string)
            {
                return "\"" + @this + "\"";
            }

            if (@this is bool)
            {
                return @this.ToString();
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
                return @this.ToString() + "F";
            }

            if (@this is double)
            {
                return @this.ToString() + "D";
            }

            if (@this is decimal)
            {
                return @this.ToString() + "M";
            }

            return @this.ToString() + " (" + @this.GetType().FullName + ")";
        }
    }
}