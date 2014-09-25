namespace Kent.Boogaart.PCLMock.UnitTests
{
    using Xunit;
    using Kent.Boogaart.PCLMock;

    public sealed class ItFixture
    {
        [Fact]
        public void it_returns_default_value_for_given_type()
        {
            Assert.Equal(0, It.IsAny<int>());
            Assert.Null(It.IsAny<string>());
        }

        [Fact]
        public void it_returns_object_of_type_specified()
        {
            string s = It.IsAny<string>();
            int i = It.IsAny<int>();

            // avoid compiler warnings
            Assert.Null(s);
            Assert.Equal(0, i);
        }
    }
}