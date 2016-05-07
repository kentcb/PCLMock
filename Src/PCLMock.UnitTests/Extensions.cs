namespace PCLMock.UnitTests
{
    public static class Extensions
    {
        public static string NormalizeLineEndings(this string @this) =>
            @this
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "\r\n");
    }
}