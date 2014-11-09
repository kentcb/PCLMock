namespace Kent.Boogaart.PCLMock.UnitTests.Utility
{
    using System;
    using Kent.Boogaart.PCLMock.Utility;
    using Xunit;

    public sealed class ObjectExtensionsFixture
    {
        [Fact]
        public void to_debug_string_handles_null()
        {
            Assert.Equal("null", ((string)null).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_strings()
        {
            Assert.Equal("\"foo\"", "foo".ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_bools()
        {
            Assert.Equal("True", true.ToDebugString());
            Assert.Equal("False", false.ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_ints()
        {
            Assert.Equal("35", 35.ToDebugString());
            Assert.Equal("-17", (-17).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_uints()
        {
            Assert.Equal("35U", 35U.ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_longs()
        {
            Assert.Equal("35L", 35L.ToDebugString());
            Assert.Equal("-17L", (-17L).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_ulongs()
        {
            Assert.Equal("35UL", 35UL.ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_floats()
        {
            Assert.Equal("3.4F", 3.4F.ToDebugString());
            Assert.Equal("-3.4F", (-3.4F).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_doubles()
        {
            Assert.Equal("3.4D", 3.4D.ToDebugString());
            Assert.Equal("-3.4D", (-3.4D).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_decimals()
        {
            Assert.Equal("3.461M", 3.461M.ToDebugString());
            Assert.Equal("-3.461M", (-3.461M).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_anything_else()
        {
            Assert.Equal("00:01:00 (System.TimeSpan)", TimeSpan.FromMinutes(1).ToDebugString());
        }
    }
}