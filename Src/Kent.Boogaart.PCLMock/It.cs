namespace Kent.Boogaart.PCLMock
{
    /// <summary>
    /// Provides a simple means of obtaining a value of any type. Useful when configuring expectations against method calls where the methods take arguments.
    /// </summary>
    public static class It
    {
        /// <summary>
        /// Returns a default instance of type <c>T</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to return.
        /// </typeparam>
        /// <returns>
        /// A default instance of type <c>T</c>.
        /// </returns>
        public static T IsAny<T>()
        {
            return default(T);
        }
    }
}