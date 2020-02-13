// -----------------------------------------------------------------------
// <copyright file="KeyValueSeparator.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Xunit;

namespace Hocon.Tests
{
    public class KeyValueSeparator
    {
        /*
         * FACT:
         * The = character can be used anywhere JSON allows :, i.e. to separate keys from values.
         */
        [Fact]
        public void CanParseHoconWithEqualsOrColonSeparator()
        {
            var hocon = @"
root = {
  int = 1
  quoted-string = ""foo""
  unquoted-string = bar
  concat-string = foo bar
  object = {
    hasContent = true
  }
  array = [1,2,3,4]
  array-concat = [[1,2] [3,4]]
  array-single-element : [1 2 3 4]
  array-newline-element : [
    1
    2
    3
    4
  ]
  null : null
  double : 1.23
  bool : true
}
root_2 : 1234
";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("1", config.GetString("root.int"));
            Assert.Equal("1.23", config.GetString("root.double"));
            Assert.True(config.GetBoolean("root.bool"));
            Assert.True(config.GetBoolean("root.object.hasContent"));
            Assert.Null(config.GetString("root.null"));
            Assert.Equal("foo", config.GetString("root.quoted-string"));
            Assert.Equal("bar", config.GetString("root.unquoted-string"));
            Assert.Equal("foo bar", config.GetString("root.concat-string"));
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(config.GetIntList("root.array")));
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(config.GetIntList("root.array-newline-element")));
            Assert.True(new[] {"1 2 3 4"}.SequenceEqual(config.GetStringList("root.array-single-element")));
            Assert.Equal("1234", config.GetString("root_2"));
        }

        /*
         * FACT:
         * If a key is followed by {, the : or = may be omitted. So "foo" {} means "foo" : {}
         */
        [Fact]
        public void CanParseHoconWithSeparatorForObjectFieldAssignment()
        {
            var hocon = @"
root {
  int = 1
}
root_2 {
  unquoted-string = bar
}
";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("1", config.GetString("root.int"));
            Assert.Equal("bar", config.GetString("root_2.unquoted-string"));
        }
    }
}