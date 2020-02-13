// -----------------------------------------------------------------------
// <copyright file="Comments.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

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
            Assert.Equal("2", HoconParser.Parse(hocon).GetString("b"));
            Assert.Equal("3", HoconParser.Parse(hocon).GetString("c"));
        }

        [Fact]
        public void DoubleSlashOrPoundsInQuotedTextAreNotIgnored()
        {
            var hocon = @"a = 1
b = ""2 // This should not be ignored"" 
c = ""3 # This should not be ignored"" 
";
            Assert.Equal("2 // This should not be ignored", HoconParser.Parse(hocon).GetString("b"));
            Assert.Equal("3 # This should not be ignored", HoconParser.Parse(hocon).GetString("c"));
        }
    }
}