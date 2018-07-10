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
            var config = HoconParser.Parse(hocon);
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
            var config = HoconParser.Parse(hocon);
            Assert.True(
                config.GetIntList("array_1")
                    .SequenceEqual(config.GetIntList("array_2")));
        }

        /*
         * FACT:
         * [1, 2, 3,,] is invalid because it has two trailing commas.
         */
        [Fact]
        public void ThrowsParserExceptionOnMultipleTrailingCommasInArray()
        {
            var hocon = @"array : [1, 2, 3,, ]";
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(hocon));
        }

        /*
         * FACT:
         * [, 1, 2, 3] is invalid because it has an initial comma.
         */
        [Fact]
        public void ThrowsParserExceptionOnIllegalCommaInFrontOfArray()
        {
            var hocon = @"array : [, 1, 2, 3]";
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(hocon));
        }

        /*
         * FACT:
         * [1,, 2, 3] is invalid because it has two commas in a row.
         */
        [Fact]
        public void ThrowsParserExceptionOnMultipleCommasInArray()
        {
            var hocon = @"array : [1,, 2, 3]";
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(hocon));
        }

        /*
         * these same comma rules apply to fields in objects.
         */

        /*
         * FACT:
         * {a:1, b:2, c:3,} and {a:1, b:2, c:3} are the same object.
         */
        [Fact]
        public void ExtraCommaAtTheEndIgnored()
        {
            var hocon_1 = @"a:1, b:2, c:3,";
            var hocon_2 = @"a:1, b:2, c:3";

            using (var config_1 = HoconParser.Parse(hocon_1).AsEnumerable().GetEnumerator())
            {
                using (var config_2 = HoconParser.Parse(hocon_2).AsEnumerable().GetEnumerator())
                {
                    while (config_1.MoveNext())
                    {
                        config_2.MoveNext();
                        Assert.Equal(config_1.Current.Key, config_2.Current.Key);
                        Assert.Equal(config_1.Current.Value.GetInt(), config_2.Current.Value.GetInt());
                    }
                }
            }
        }

        /*
         * FACT:
         * {a:1\nb:2\nc:3} and {a:1, b:2, c:3} are the same object.
         */
        [Fact]
        public void NewLineCanReplaceComma()
        {
            var hocon_1 = @"
a:1
b:2
c:3";
            var hocon_2 = @"a:1, b:2, c:3";

            using (var config_1 = HoconParser.Parse(hocon_1).AsEnumerable().GetEnumerator())
            {
                using (var config_2 = HoconParser.Parse(hocon_2).AsEnumerable().GetEnumerator())
                {
                    while (config_1.MoveNext())
                    {
                        config_2.MoveNext();
                        Assert.Equal(config_1.Current.Key, config_2.Current.Key);
                        Assert.Equal(config_1.Current.Value.GetInt(), config_2.Current.Value.GetInt());
                    }
                }
            }
        }

        /*
         * FACT:
         * {a:1, b:2, c:3,,} is invalid because it has two trailing commas.
         */
        [Fact]
        public void ThrowsParserExceptionOnMultipleTrailingCommas()
        {
            var hocon = @"{a:1, b:2, c:3,,}";
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(hocon));
        }

        /*
         * FACT:
         * {, a:1, b:2, c:3} is invalid because it has an initial comma.
         */
        [Fact]
        public void ThrowsParserExceptionOnIllegalCommaInFront()
        {
            var hocon = @"{, a:1, b:2, c:3}";
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(hocon));
        }

        /*
         * FACT:
         * {a:1,, b:2, c:3} is invalid because it has two commas in a row.
         */
        [Fact]
        public void ThrowsParserExceptionOnMultipleCommas()
        {
            var hocon = @"{a:1,, b:2, c:3}";
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(hocon));
        }
    }
}
