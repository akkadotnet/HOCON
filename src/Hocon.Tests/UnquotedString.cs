// -----------------------------------------------------------------------
// <copyright file="UnquotedString.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using Xunit;

namespace Hocon.Tests
{
    public class UnquotedString
    {
        /*
         * FACT:
         * As long as simple values are separated only by non-newline whitespace, 
         * the whitespace between them is preserved and the values, along with the whitespace, 
         * are concatenated into a string.
         */
        [Fact]
        public void ShouldPreserveWhitespacesInTheMiddle()
        {
            var hocon = $"a = literal{Whitespace.Whitespaces}value";
            var config = HoconParser.Parse(hocon);

            Assert.Equal($"literal{Whitespace.Whitespaces}value", config.GetString("a"));
        }
        /*
         * From: String value concatenation.
         */

        /*
         * FACT:
         * Whitespace before the first and after the last simple value must be discarded
         */
        [Fact]
        public void ShouldRemoveAllLeadingWhitespace()
        {
            var hocon = $"a = {Whitespace.Whitespaces}literal value";
            var config = HoconParser.Parse(hocon);

            Assert.Equal("literal value", config.GetString("a"));
        }

        [Fact]
        public void ShouldRemoveAllTrailingWhitespace()
        {
            var hocon = $"a = literal value{Whitespace.Whitespaces}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal("literal value", config.GetString("a"));
        }
    }
}