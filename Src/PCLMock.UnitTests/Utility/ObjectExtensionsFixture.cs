namespace PCLMock.UnitTests.Utility
{
    using System;
    using PCLMock.Utility;
    using Xunit;
    using Xunit.Extensions;

    public sealed class ToDebugStringExtensionsFixture
    {
        [Theory]
        [InlineData(null, "null")]
        [InlineData("foo", "\"foo\"")]
        [InlineData(true, "true")]
        [InlineData(false, "false")]
        [InlineData(35, "35")]
        [InlineData(-17, "-17")]
        [InlineData(35U, "35U")]
        [InlineData(17U, "17U")]
        [InlineData(35L, "35L")]
        [InlineData(-17L, "-17L")]
        [InlineData(35UL, "35UL")]
        [InlineData(17UL, "17UL")]
        [InlineData(3.4f, "3.4F")]
        [InlineData(-3.4f, "-3.4F")]
        [InlineData(3.4d, "3.4D")]
        [InlineData(-3.4d, "-3.4D")]
        [InlineData(typeof(bool), "bool")]
        [InlineData(typeof(byte), "byte")]
        [InlineData(typeof(sbyte), "sbyte")]
        [InlineData(typeof(char), "char")]
        [InlineData(typeof(decimal), "decimal")]
        [InlineData(typeof(short), "short")]
        [InlineData(typeof(ushort), "ushort")]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(uint), "uint")]
        [InlineData(typeof(long), "long")]
        [InlineData(typeof(ulong), "ulong")]
        [InlineData(typeof(float), "float")]
        [InlineData(typeof(double), "double")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(int?), "int?")]
        [InlineData(typeof(long?), "long?")]
        [InlineData(typeof(float?), "float?")]
        [InlineData(typeof(DayOfWeek?), "System.DayOfWeek?")]
        [InlineData(typeof(Guid?), "System.Guid?")]
        [InlineData(typeof(ToDebugStringExtensionsFixture), "PCLMock.UnitTests.Utility.ToDebugStringExtensionsFixture")]
        [InlineData(DayOfWeek.Monday, "DayOfWeek.Monday")]
        [InlineData(DayOfWeek.Friday, "DayOfWeek.Friday")]
        [InlineData((DayOfWeek)238, "DayOfWeek.238")]
        [InlineData(AttributeTargets.Assembly | AttributeTargets.Class, "AttributeTargets.Assembly | AttributeTargets.Class")]
        [InlineData(AttributeTargets.Assembly | (AttributeTargets)32768, "AttributeTargets.Assembly")]
        public void to_debug_string_returns_expected_result(object o, string expectedResult)
        {
            Assert.Equal(expectedResult, o.ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_decimals()
        {
            Assert.Equal("3.461M", 3.461M.ToDebugString());
            Assert.Equal("-3.461M", (-3.461M).ToDebugString());
        }

        [Fact]
        public void to_debug_string_handles_anything_else_in_a_generic_fashion()
        {
            Assert.Equal("00:01:00 [System.TimeSpan]", TimeSpan.FromMinutes(1).ToDebugString());
            Assert.Equal("d527c281-1a18-452d-81ea-e8c2f3a252df [System.Guid]", new Guid("D527C281-1A18-452D-81EA-E8C2F3A252DF").ToDebugString());
        }
    }
}