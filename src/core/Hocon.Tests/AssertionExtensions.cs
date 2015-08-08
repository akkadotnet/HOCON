using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
