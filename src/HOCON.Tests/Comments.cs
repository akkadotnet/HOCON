using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class Comments
    {
        /*
         * Anything between // or # and the next newline is considered a comment and ignored, 
         * unless the // or # is inside a quoted string.
         */

        [Fact]
        public void CommentsShouldBeIgnored()
        {
            var hocon = @"a = 1
// This should be ignored
b = 2 // This should be ignored
# This should be ignored
c = 3 # This should be ignored";
            Assert.Equal("2", Parser.Parse(hocon).GetString("b"));
            Assert.Equal("3", Parser.Parse(hocon).GetString("c"));
        }

        [Fact]
        public void DoubleSlashOrPoundsInQuotedTextAreNotIgnored()
        {
            var hocon = @"a = 1
b = ""2 // This should not be ignored"" 
c = ""3 # This should not be ignored"" 
";
            Assert.Equal("2 // This should not be ignored", Parser.Parse(hocon).GetString("b"));
            Assert.Equal("3 # This should not be ignored", Parser.Parse(hocon).GetString("c"));
        }
    }
}
