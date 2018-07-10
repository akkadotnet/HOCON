using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class OmitRootBraces
    {
        /*
         * FACT:
         * Empty files are invalid documents.
         */
        [Fact]
        public void EmptyFilesShouldThrows()
        {
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(""));
            Assert.Throws<HoconParserException>(() => HoconParser.Parse(null));
        }

        /* 
         * FACT:
         * Files containing only a non-array non-object value such as a string are invalid.
         */
        [Fact]
        public void FileWithLiteralOnlyShouldThrows()
        {
            Assert.Throws<HoconParserException>(() => HoconParser.Parse("literal"));
            Assert.Throws<HoconParserException>(() => HoconParser.Parse("${?path}"));
        }

        /*
         * FACT:
         * If the file does not begin with a square bracket or curly brace, 
         * it is parsed as if it were enclosed with {} curly braces.
         */
        [Fact]
        public void CanParseJson()
        {
            var hocon = @"{
  ""root"" : {
    ""int"" : 1,
    ""string"" : ""foo"",
    ""object"" : {
      ""hasContent"" : true
    },
    ""array"" : [1,2,3],
    ""null"" : null,
    ""double"" : 1.23,
    ""bool"" : true
  },
  ""root_2"" : 1234
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("1", config.GetString("root.int"));
            Assert.Equal("1.23", config.GetString("root.double"));
            Assert.True(config.GetBoolean("root.bool"));
            Assert.True(config.GetBoolean("root.object.hasContent"));
            Assert.Null(config.GetString("root.null"));
            Assert.Equal("foo", config.GetString("root.string"));
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(HoconParser.Parse(hocon).GetIntList("root.array")));
            Assert.Equal("1234", config.GetString("root_2"));
        }

        [Fact]
        public void CanParseJsonWithNoRootBraces()
        {
            var hocon = @"
""root"" : {
  ""int"" : 1,
  ""string"" : ""foo"",
  ""object"" : {
    ""hasContent"" : true
  },
  ""array"" : [1,2,3],
  ""null"" : null,
  ""double"" : 1.23,
  ""bool"" : true
},
""root_2"" : 1234";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("1", config.GetString("root.int"));
            Assert.Equal("1.23", config.GetString("root.double"));
            Assert.True(config.GetBoolean("root.bool"));
            Assert.True(config.GetBoolean("root.object.hasContent"));
            Assert.Null(config.GetString("root.null"));
            Assert.Equal("foo", config.GetString("root.string"));
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(HoconParser.Parse(hocon).GetIntList("root.array")));
            Assert.Equal("1234", config.GetString("root_2"));
        }

        [Fact]
        public void CanParseHocon()
        {
            var hocon = @"
root {
  int = 1
  quoted-string = ""foo""
  unquoted-string = bar
  concat-string = foo bar
  object {
    hasContent = true
  }
  array = [1,2,3,4]
  array-concat = [[1,2] [3,4]]
  array-single-element = [1 2 3 4]
  array-newline-element = [
    1
    2
    3
    4
  ]
  null = null
  double = 1.23
  bool = true
}
root_2 = 1234
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
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(HoconParser.Parse(hocon).GetIntList("root.array")));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(
                    HoconParser.Parse(hocon).GetIntList("root.array-newline-element")));
            Assert.True(
                new[] { "1 2 3 4" }.SequenceEqual(
                    HoconParser.Parse(hocon).GetStringList("root.array-single-element")));
            Assert.Equal("1234", config.GetString("root_2"));
        }

        [Fact]
        public void CanParseHoconWithRootBraces()
        {
            var hocon = @"
{
  root {
    int = 1
    quoted-string = ""foo""
    unquoted-string = bar
    concat-string = foo bar
    object {
      hasContent = true
    }
    array = [1,2,3,4]
    array-concat = [[1,2] [3,4]]
    array-single-element = [1 2 3 4]
    array-newline-element = [
      1
      2
      3
      4
    ]
    null = null
    double = 1.23
    bool = true
  }
  root_2 : 1234
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("1", config.GetString("root.int"));
            Assert.Equal("1.23", config.GetString("root.double"));
            Assert.True(config.GetBoolean("root.bool"));
            Assert.True(config.GetBoolean("root.object.hasContent"));
            Assert.Null(config.GetString("root.null"));
            Assert.Equal("foo", config.GetString("root.quoted-string"));
            Assert.Equal("bar", config.GetString("root.unquoted-string"));
            Assert.Equal("foo bar", config.GetString("root.concat-string"));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(HoconParser.Parse(hocon).GetIntList("root.array")));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(
                    HoconParser.Parse(hocon).GetIntList("root.array-newline-element")));
            Assert.True(
                new[] { "1 2 3 4" }.SequenceEqual(
                    HoconParser.Parse(hocon).GetStringList("root.array-single-element")));
            Assert.Equal("1234", config.GetString("root_2"));
        }

        /*
         * FACT:
         * A HOCON file is invalid if it omits the opening { but still has a closing }
         * the curly braces must be balanced.
         */

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedFile()
        {
            var hocon = "{ root { string : \"hello\" }";
            Assert.Throws<HoconParserException>(() =>
                HoconParser.Parse(hocon));
        }

        [Fact]
        public void ThrowsParserExceptionOnInvalidTerminatedFile()
        {
            var hocon = "root { string : \"hello\" }}";
            Assert.Throws<HoconParserException>(() =>
                HoconParser.Parse(hocon));
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedObject()
        {
            var hocon = " root { string : \"hello\" ";
            Assert.Throws<HoconParserException>(() =>
                HoconParser.Parse(hocon));
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedNestedObject()
        {
            var hocon = " root { bar { string : \"hello\" } ";
            Assert.Throws<HoconParserException>(() =>
                HoconParser.Parse(hocon));
        }

    }
}
