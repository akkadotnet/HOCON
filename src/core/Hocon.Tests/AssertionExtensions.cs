using NUnit.Framework;

namespace Hocon.Tests
{
    public static class AssertionExtensions
    {
        public static void ShouldBe<T>(this T self, T other)
        {
            Assert.AreEqual(other, self);
        }
    }
}
