namespace Kent.Boogaart.PCLMock
{
    public static class It
    {
        public static T IsAny<T>()
        {
            return default(T);
        }
    }
}