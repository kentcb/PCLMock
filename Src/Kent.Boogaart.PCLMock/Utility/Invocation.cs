namespace Kent.Boogaart.PCLMock.Utility
{
    // records details about an invocation
    internal struct Invocation
    {
        private readonly object[] arguments;

        public Invocation(object[] arguments)
        {
            this.arguments = arguments;
        }

        public object[] Arguments
        {
            get { return this.arguments; }
        }
    }
}