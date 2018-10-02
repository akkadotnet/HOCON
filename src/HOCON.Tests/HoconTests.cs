//-----------------------------------------------------------------------
// <copyright file="HoconTests.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;



namespace Hocon.Tests
{
    public class HoconTests
    {
        private readonly ITestOutputHelper _output;

        public HoconTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanUnwrapSubConfig() //undefined behavior in spec, this does not behave the same as JVM hocon.
        {
            var hocon = @"
a {
   b {
     c = 1
     d = true
   }
}";
            var config = Parser.Parse(hocon).Value.GetObject().Unwrapped;
            var a = config["a"] as IDictionary<string, object>;
            var b = a["b"] as IDictionary<string, object>;
            (b["c"] as HoconField).Value.GetInt().Should().Be(1);
            (b["d"] as HoconField).Value.GetBoolean().Should().Be(true);
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedString() //undefined behavior in spec
        {
            var hocon = " string : \"hello";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedStringInObject() //undefined behavior in spec
        {
            var hocon = " root { string : \"hello }";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedArray() //undefined behavior in spec
        {
            var hocon = " array : [1,2,3";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedArrayInObject() //undefined behavior in spec
        {
            var hocon = " root { array : [1,2,3 }";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void GettingStringFromArrayReturnsNull() //undefined behavior in spec
        {
            var hocon = " array : [1,2,3]";
            Parser.Parse(hocon).GetString("array").Should().Be(null);
        }

        //TODO: not sure if this is the expected behavior but it is what we have established in Akka.NET
        [Fact]
        public void GettingArrayFromLiteralsReturnsNull() //undefined behavior in spec
        {
            var hocon = " literal : a b c";
            var res = Parser.Parse(hocon).GetStringList("literal");

            res.Should().BeEmpty();
        }

        //Added tests to conform to the HOCON spec https://github.com/typesafehub/config/blob/master/HOCON.md
        [Fact]
        public void CanUsePathsAsKeys_3_14()
        {
            var hocon1 = @"3.14 : 42";
            var hocon2 = @"3 { 14 : 42}";
            Assert.Equal(Parser.Parse(hocon1).GetString("3.14"),
                Parser.Parse(hocon2).GetString("3.14"));
        }

        [Fact]
        public void CanUsePathsAsKeys_3()
        {
            var hocon1 = @"3 : 42";
            var hocon2 = @"""3"" : 42";
            Assert.Equal(Parser.Parse(hocon1).GetString("3"),
                Parser.Parse(hocon2).GetString("3"));
        }

        [Fact]
        public void CanUsePathsAsKeys_true()
        {
            var hocon1 = @"true : 42";
            var hocon2 = @"""true"" : 42";
            Assert.Equal(Parser.Parse(hocon1).GetString("true"),
                Parser.Parse(hocon2).GetString("true"));
        }

        [Fact]
        public void CanUsePathsAsKeys_FooBar()
        {
            var hocon1 = @"foo.bar : 42";
            var hocon2 = @"foo { bar : 42 }";
            Assert.Equal(Parser.Parse(hocon1).GetString("foo.bar"),
                Parser.Parse(hocon2).GetString("foo.bar"));
        }

        [Fact]
        public void CanUsePathsAsKeys_FooBarBaz()
        {
            var hocon1 = @"foo.bar.baz : 42";
            var hocon2 = @"foo { bar { baz : 42 } }";
            Assert.Equal(Parser.Parse(hocon1).GetString("foo.bar.baz"),
                Parser.Parse(hocon2).GetString("foo.bar.baz"));
        }

        [Fact]
        public void CanUsePathsAsKeys_AX_AY()
        {
            var hocon1 = @"a.x : 42, a.y : 43";
            var hocon2 = @"a { x : 42, y : 43 }";
            Assert.Equal(Parser.Parse(hocon1).GetString("a.x"),
                Parser.Parse(hocon2).GetString("a.x"));
            Assert.Equal(Parser.Parse(hocon1).GetString("a.y"),
                Parser.Parse(hocon2).GetString("a.y"));
        }

        [Fact]
        public void CanUsePathsAsKeys_A_B_C()
        {
            var hocon1 = @"a b c : 42";
            var hocon2 = @"""a b c"" : 42";
            Assert.Equal(Parser.Parse(hocon1).GetString("a b c"),
                Parser.Parse(hocon2).GetString("a b c"));
        }


        [Fact]
        public void CanMergeObject()
        {
            var hocon = @"
a.b.c = {
        x = 1
        y = 2
    }
a.b.c = {
        z = 3
    }
";
            var config = Parser.Parse(hocon);
            Assert.Equal("1", config.GetString("a.b.c.x"));
            Assert.Equal("2", config.GetString("a.b.c.y"));
            Assert.Equal("3", config.GetString("a.b.c.z"));
        }

        [Fact]
        public void CanOverrideObject()
        {
            var hocon = @"
a.b = 1
a = null
a.c = 3
";
            var config = Parser.Parse(hocon);
            Assert.False(config.HasPath("a.b"));
            Assert.Equal("3", config.GetString("a.c"));
        }

        [Fact]
        public void CanParseObject()
        {
            var hocon = @"
a {
  b = 1
}
";
            Assert.Equal("1", Parser.Parse(hocon).GetString("a.b"));
        }

        [Fact]
        public void CanTrimValue()
        {
            var hocon = "a= \t \t 1 \t \t,";
            Assert.Equal("1", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanTrimConcatenatedValue()
        {
            var hocon = "a= \t \t 1 2 3 \t \t,";
            Assert.Equal("1 2 3", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanConsumeCommaAfterValue()
        {
            var hocon = "a=1,";
            Assert.Equal("1", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignIpAddressToField()
        {
            var hocon = @"a=127.0.0.1";
            Assert.Equal("127.0.0.1", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignConcatenatedValueToField()
        {
            var hocon = @"a=1 2 3";
            Assert.Equal("1 2 3", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignValueToQuotedField()
        {
            var hocon = @"""a""=1";
            Assert.Equal(1L, Parser.Parse(hocon).GetLong("a"));
        }

        [Fact]
        public void CanAssignValueToPathExpression()
        {
            var hocon = @"a.b.c=1";
            var config = Parser.Parse(hocon);
            Assert.Equal(1L, config.GetLong("a.b.c"));
        }

        [Fact]
        public void CanAssignValuesToPathExpressions()
        {
            var hocon = @"
a.b.c=1
a.b.d=2
a.b.e.f=3
";
            var config = Parser.Parse(hocon);
            Assert.Equal(1L, config.GetLong("a.b.c"));
            Assert.Equal(2L, config.GetLong("a.b.d"));
            Assert.Equal(3L, config.GetLong("a.b.e.f"));
        }

        [Fact]
        public void CanAssignLongToField()
        {
            var hocon = @"a=1";
            Assert.Equal(1L, Parser.Parse(hocon).GetLong("a"));
        }

        [Fact]
        public void CanAssignArrayToField()
        {
            var hocon = @"a=
[
    1
    2
    3
]";
            Assert.True(new[] {1, 2, 3}.SequenceEqual(Parser.Parse(hocon).GetIntList("a")));

            //hocon = @"a= [ 1, 2, 3 ]";
            //Assert.True(new[] { 1, 2, 3 }.SequenceEqual(Parser.Parse(hocon).GetIntList("a")));
        }

        [Fact]
        public void CanConcatenateArray()
        {
            var hocon = @"a=[1,2] [3,4]";
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(Parser.Parse(hocon).GetIntList("a")));
        }

        [Fact]
        public void CanAssignDoubleToField()
        {
            var hocon = @"a=1.1";
            Assert.Equal(1.1, Parser.Parse(hocon).GetDouble("a"));
        }

        [Fact]
        public void CanAssignNumbersToField()
        {
            var hocon = @"
a = 1000.05
b = Infinity
c = -Infinity
d = +Infinity
e = NaN
f = 255
g = 0xff
h = 0377
";
            var config = Parser.Parse(hocon);
            Assert.Equal(1000.05, config.GetDouble("a"));
            Assert.Equal(double.PositiveInfinity, config.GetDouble("b"));
            Assert.Equal(double.NegativeInfinity, config.GetDouble("c"));
            Assert.Equal(double.PositiveInfinity, config.GetDouble("d"));
            Assert.Equal(double.NaN, config.GetDouble("e"));

            Assert.Equal(1000.05f, config.GetFloat("a"));
            Assert.Equal(float.PositiveInfinity, config.GetFloat("b"));
            Assert.Equal(float.NegativeInfinity, config.GetFloat("c"));
            Assert.Equal(float.PositiveInfinity, config.GetFloat("d"));
            Assert.Equal(float.NaN, config.GetFloat("e"));

            Assert.Equal(1000.05m, config.GetDecimal("a"));
            Assert.Throws<HoconException>(() => config.GetDecimal("b"));
            Assert.Throws<HoconException>(() => config.GetDecimal("c"));
            Assert.Throws<HoconException>(() => config.GetDecimal("d"));

            Assert.Equal(255, config.GetLong("f"));
            Assert.Equal(255, config.GetLong("g"));
            Assert.Equal(255, config.GetLong("h"));

            Assert.Equal(255, config.GetInt("f"));
            Assert.Equal(255, config.GetInt("g"));
            Assert.Equal(255, config.GetInt("h"));

            Assert.Equal(255, config.GetByte("f"));
            Assert.Equal(255, config.GetByte("g"));
            Assert.Equal(255, config.GetByte("h"));
        }

        [Fact]
        public void CanAssignNullToField()
        {
            var hocon = @"a=null";
            Assert.Null(Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignBooleanToField()
        {
            var hocon = @"a=true";
            Assert.True(Parser.Parse(hocon).GetBoolean("a"));
            hocon = @"a=false";
            Assert.False(Parser.Parse(hocon).GetBoolean("a"));

            hocon = @"a=on";
            Assert.True(Parser.Parse(hocon).GetBoolean("a"));
            hocon = @"a=off";
            Assert.False(Parser.Parse(hocon).GetBoolean("a"));

            hocon = @"a=yes";
            Assert.True(Parser.Parse(hocon).GetBoolean("a"));
            hocon = @"a=no";
            Assert.False(Parser.Parse(hocon).GetBoolean("a"));
        }

        [Fact]
        public void FailedIncludeParsingShouldBeParsedAsLiteralInstead()
        {
            var hocon = @"{
  include = include required file(not valid)
  include file = not an include
}";
            var config = Parser.Parse(hocon);
            Assert.Equal("include required file(not valid)", config.GetString("include"));
            Assert.Equal("not an include", config.GetString("include file"));
        }

        [Fact]
        public void CanAssignQuotedStringToField()
        {
            var hocon = @"a=""hello""";
            Assert.Equal("hello", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignUnQuotedStringToField()
        {
            var hocon = @"a=hello";
            Assert.Equal("hello", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignTripleQuotedStringToField()
        {
            var hocon = "a=\"\"\"hello\"\"\"";
            Assert.Equal("hello", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanOverwriteValue()
        {
            var hocon = @"
test {
  value  = 123
}
test.value = 456
";
            var config = Parser.Parse(hocon);
            config.GetInt("test.value").Should().Be(456);
        }

        [Fact]
        public void CanAssignNullStringToField()
        {
            var hocon = @"a=null";
            Assert.Null(Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignQuotedNullStringToField()
        {
            var hocon = @"a=""null""";
            Assert.Equal("null", Parser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanParseInclude()
        {
            var hocon = @"a {
    include ""foo""
    b : include ""foo""
}";
            var includeHocon = @"
x = 123
y = hello
";
            Task<string> IncludeCallback(HoconCallbackType t, string s) 
                => Task.FromResult(includeHocon);

            var config = Parser.Parse(hocon, IncludeCallback);

            Assert.Equal(123, config.GetInt("a.x"));
            Assert.Equal("hello", config.GetString("a.y"));
            Assert.Equal(123, config.GetInt("a.b.x"));
            Assert.Equal("hello", config.GetString("a.b.y"));
        }

        [Fact]
        public void CanParseArrayInclude()
        {
            var hocon = @"a : include ""foo""";
            var includeHocon = @"[1, 2, 3]";

            Task<string> IncludeCallback(HoconCallbackType t, string s)
                => Task.FromResult(includeHocon);

            var config = Parser.Parse(hocon, IncludeCallback);
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(config.GetIntList("a")));
        }

        [Fact]
        public void CanParseArrayIncludeInsideArray()
        {
            var hocon = @"a : [ include ""foo"" ]";
            var includeHocon = @"[1, 2, 3]";

            Task<string> IncludeCallback(HoconCallbackType t, string s)
                => Task.FromResult(includeHocon);

            var config = Parser.Parse(hocon, IncludeCallback);
            // TODO: need to figure a better way to retrieve array inside array
            var array = config.GetValue("a").GetArray()[0].GetArray().Select(v => v.GetInt());
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(array));
        }
    }
}

