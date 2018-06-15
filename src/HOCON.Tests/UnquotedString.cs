using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class UnquotedString
    {
        [Fact]
        public void ShouldRemoveAllLeadingWhitespace()
        {
            var hocon = $"a = {Whitespace.Whitespaces}literal";
            var config = ConfigurationFactory.ParseString(hocon);

            Assert.Equal("literal", config.GetString("a"));
        }

        [Fact]
        public void ShouldRemoveAllTrailingWhitespace()
        {
            var hocon = $"a = literal{Whitespace.Whitespaces}";
            var config = ConfigurationFactory.ParseString(hocon);

            Assert.Equal("literal", config.GetString("a"));
        }
    }
}
