namespace Kent.Boogaart.PCLMock
{
    public enum MockBehavior
    {
        // any invocation against the mock must have a continuation specified, or it will fail
        Strict,

        // an invocation against the mock need not have a continuation. If the invocation requires a return value and no continuation exists, the default value will be returned
        Loose
    }
}