using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class Commas
    {
        /*
         * Values in arrays, and fields in objects, need not have a comma between them 
         * as long as they have at least one ASCII newline (\n, decimal value 10) between them.
         * 
         * The last element in an array or last field in an object may be followed by a single comma. This extra comma is ignored.
         */

        /*
         * FACT:
         * [1, 2, 3,] and [1, 2, 3] are the same array.
         */
        [Fact]
        public void ExtraCommaAtTheEndOfArraysIsIgnored()
        {
            var hocon = @"
array_1 : [1, 2, 3, ]
array_2 : [1, 2, 3]
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.True(
                config.GetIntList("array_1")
                .SequenceEqual(config.GetIntList("array_2")));
        }

        /*
         * FACT:
         * [1\n2\n3] and[1, 2, 3] are the same array.
         */
        [Fact]
        public void NewLineCanReplaceCommaInArrays()
        {
            var hocon = @"
array_1 : [
  1
  2
  3 ]
array_2 : [1, 2, 3]
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.True(
                config.GetIntList("array_1")
                    .SequenceEqual(config.GetIntList("array_2")));
        }

        /*
         * FACT:
         * [1, 2, 3,,] is invalid because it has two trailing commas.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ThrowsParserExceptionOnMultipleTrailingCommasInArray()
        {
            var hocon = @"array : [1, 2, 3,, ]";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        /*
         * FACT:
         * [, 1, 2, 3] is invalid because it has an initial comma.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ThrowsParserExceptionOnIllegalCommaInFrontOfArray()
        {
            var hocon = @"array : [, 1, 2, 3]";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        /*
         * FACT:
         * [1,, 2, 3] is invalid because it has two commas in a row.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ThrowsParserExceptionOnMultipleCommasInArray()
        {
            var hocon = @"array : [1,, 2, 3]";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        /*
         * these same comma rules apply to fields in objects.
         */

        /*
         * FACT:
         * {a:1, b:2, c:3,} and {a:1, b:2, c:3} are the same object.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ExtraCommaAtTheEndIgnored()
        {
            var hocon_1 = @"a:1, b:2, c:3,";
            var hocon_2 = @"a:1, b:2, c:3";

            var config_1 = ConfigurationFactory.ParseString(hocon_1);
            var config_2 = ConfigurationFactory.ParseString(hocon_2);
            Assert.True(config_1.AsEnumerable().SequenceEqual(config_2.AsEnumerable()));
        }

        /*
         * FACT:
         * {a:1\nb:2\nc:3} and {a:1, b:2, c:3} are the same object.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void NewLineCanReplaceComma()
        {
            var hocon_1 = @"
a:1
b:2
c:3";
            var hocon_2 = @"a:1, b:2, c:3";

            var config_1 = ConfigurationFactory.ParseString(hocon_1);
            var config_2 = ConfigurationFactory.ParseString(hocon_2);
            Assert.True(config_1.AsEnumerable().SequenceEqual(config_2.AsEnumerable()));
        }

        /*
         * FACT:
         * {a:1, b:2, c:3,,} is invalid because it has two trailing commas.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ThrowsParserExceptionOnMultipleTrailingCommas()
        {
            var hocon = @"{a:1, b:2, c:3,,}";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        /*
         * FACT:
         * {, a:1, b:2, c:3} is invalid because it has an initial comma.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ThrowsParserExceptionOnIllegalCommaInFront()
        {
            var hocon = @"{, a:1, b:2, c:3}";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        /*
         * FACT:
         * {a:1,, b:2, c:3} is invalid because it has two commas in a row.
         */
        [Fact(Skip = "Failed, not in spec")]
        public void ThrowsParserExceptionOnMultipleCommas()
        {
            var hocon = @"{a:1,, b:2, c:3}";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }
    }
}
