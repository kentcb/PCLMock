namespace Kent.Boogaart.PCLMock
{
    using System;

    /// <summary>
    /// The exception type used to indicate a verification failure.
    /// </summary>
    public sealed class VerificationException : Exception
    {
        /// <inheritdoc/>
        public VerificationException()
        {
        }

        /// <inheritdoc/>
        public VerificationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public VerificationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}