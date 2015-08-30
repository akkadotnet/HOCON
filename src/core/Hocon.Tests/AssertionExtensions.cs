using Xunit;

namespace Hocon.Tests
{
    public static class AssertionExtensions
    {
        public static void ShouldBe<T>(this T self, T other)
        {
            Assert.Equal(other, self);
        }
    }
}
