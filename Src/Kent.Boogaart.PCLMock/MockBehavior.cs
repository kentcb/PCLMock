namespace Kent.Boogaart.PCLMock
{
    /// <summary>
    /// Defines possible mock behaviors.
    /// </summary>
    public enum MockBehavior
    {

        /// <summary>
        /// Any invocation against the mock must have a specification (configured via <see cref="MockBase{T}.When"/>). If a member of the mock
        /// is invoked and it does not have a specification, an exception is thrown.
        /// </summary>
        Strict,

        /// <summary>
        /// Invocations against the mock are not required to have specifications. If a member of the mock is invoked and it does not have a specification
        /// and that member needs to return a value, the default value for the member's type is returned.
        /// </summary>
        Loose
    }
}