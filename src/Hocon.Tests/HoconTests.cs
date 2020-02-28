// -----------------------------------------------------------------------
// <copyright file="HoconTests.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

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
        public HoconTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Theory]
        [InlineData(@"{
    include ""foo""
    a : include ""foo""
}")]
        [InlineData(@"
    include ""foo""
    a : include ""foo""
")]
        public void CanParseIncludeInRoot(string hocon)
        {
            var includeHocon = @"
x = 123
y = hello
";

            Task<string> IncludeCallback(HoconCallbackType t, string s)
            {
                return Task.FromResult(includeHocon);
            }

            var config = HoconParser.Parse(hocon, IncludeCallback);

            Assert.Equal(123, config.GetInt("x"));
            Assert.Equal("hello", config.GetString("y"));
            Assert.Equal(123, config.GetInt("a.x"));
            Assert.Equal("hello", config.GetString("a.y"));
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
            Assert.True(new[] {1, 2, 3}.SequenceEqual(HoconParser.Parse(hocon).GetIntList("a")));

            //hocon = @"a= [ 1, 2, 3 ]";
            //Assert.True(new[] { 1, 2, 3 }.SequenceEqual(Parser.Parse(hocon).GetIntList("a")));
        }

        [Fact]
        public void CanAssignBooleanToField()
        {
            var hocon = @"a=true";
            Assert.True(HoconParser.Parse(hocon).GetBoolean("a"));
            hocon = @"a=false";
            Assert.False(HoconParser.Parse(hocon).GetBoolean("a"));

            hocon = @"a=on";
            Assert.True(HoconParser.Parse(hocon).GetBoolean("a"));
            hocon = @"a=off";
            Assert.False(HoconParser.Parse(hocon).GetBoolean("a"));

            hocon = @"a=yes";
            Assert.True(HoconParser.Parse(hocon).GetBoolean("a"));
            hocon = @"a=no";
            Assert.False(HoconParser.Parse(hocon).GetBoolean("a"));
        }

        [Fact]
        public void CanAssignConcatenatedValueToField()
        {
            var hocon = @"a=1 2 3";
            Assert.Equal("1 2 3", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignDoubleToField()
        {
            var hocon = @"a=1.1";
            Assert.Equal(1.1, HoconParser.Parse(hocon).GetDouble("a"));
        }

        [Fact]
        public void CanAssignIpAddressToField()
        {
            var hocon = @"a=127.0.0.1";
            Assert.Equal("127.0.0.1", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignLongToField()
        {
            var hocon = @"a=1";
            Assert.Equal(1L, HoconParser.Parse(hocon).GetLong("a"));
        }

        [Fact]
        public void CanAssignNullStringToField()
        {
            var hocon = @"a=null";
            Assert.Null(HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignNullToField()
        {
            var hocon = @"a=null";
            Assert.Null(HoconParser.Parse(hocon).GetString("a"));
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
            var config = HoconParser.Parse(hocon);
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
            Assert.Throws<HoconValueException>(() => config.GetDecimal("b")).GetBaseException().Should()
                .BeOfType<HoconException>();
            Assert.Throws<HoconValueException>(() => config.GetDecimal("c")).GetBaseException().Should()
                .BeOfType<HoconException>();
            Assert.Throws<HoconValueException>(() => config.GetDecimal("d")).GetBaseException().Should()
                .BeOfType<HoconException>();

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
        public void CanAssignQuotedNullStringToField()
        {
            var hocon = @"a=""null""";
            Assert.Equal("null", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignQuotedStringToField()
        {
            var hocon = @"a=""hello""";
            Assert.Equal("hello", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignTripleQuotedStringToField()
        {
            var hocon = "a=\"\"\"hello\"\"\"";
            Assert.Equal("hello", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignUnQuotedStringToField()
        {
            var hocon = @"a=hello";
            Assert.Equal("hello", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanAssignValuesToPathExpressions()
        {
            var hocon = @"
a.b.c=1
a.b.d=2
a.b.e.f=3
";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(1L, config.GetLong("a.b.c"));
            Assert.Equal(2L, config.GetLong("a.b.d"));
            Assert.Equal(3L, config.GetLong("a.b.e.f"));
        }

        [Fact]
        public void CanAssignValueToPathExpression()
        {
            var hocon = @"a.b.c=1";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(1L, config.GetLong("a.b.c"));
        }

        [Fact]
        public void CanAssignValueToQuotedField()
        {
            var hocon = @"""a""=1";
            Assert.Equal(1L, HoconParser.Parse(hocon).GetLong("a"));
        }

        [Fact]
        public void CanConcatenateArray()
        {
            var hocon = @"a=[1,2] [3,4]";
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(HoconParser.Parse(hocon).GetIntList("a")));
        }

        [Fact]
        public void CanConsumeCommaAfterValue()
        {
            var hocon = "a=1,";
            Assert.Equal("1", HoconParser.Parse(hocon).GetString("a"));
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
            var config = HoconParser.Parse(hocon);
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
            var config = HoconParser.Parse(hocon);
            Assert.False(config.HasPath("a.b"));
            Assert.Equal("3", config.GetString("a.c"));
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
            var config = HoconParser.Parse(hocon);
            Assert.Equal(456, config.GetInt("test.value"));
        }

        [Fact]
        public void CanParseArrayInclude()
        {
            var hocon = @"a : include ""foo""";
            var includeHocon = @"[1, 2, 3]";

            Task<string> IncludeCallback(HoconCallbackType t, string s)
            {
                return Task.FromResult(includeHocon);
            }

            var config = HoconParser.Parse(hocon, IncludeCallback);
            Assert.True(new[] {1, 2, 3}.SequenceEqual(config.GetIntList("a")));
        }

        [Fact]
        public void CanParseArrayIncludeInsideArray()
        {
            var hocon = @"a : [ include ""foo"" ]";
            var includeHocon = @"[1, 2, 3]";

            Task<string> IncludeCallback(HoconCallbackType t, string s)
            {
                return Task.FromResult(includeHocon);
            }

            var config = HoconParser.Parse(hocon, IncludeCallback);
            // TODO: need to figure a better way to retrieve array inside array
            var array = config.GetValue("a").GetArray()[0].GetIntList();
            Assert.True(new[] {1, 2, 3}.SequenceEqual(array));
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
            {
                return Task.FromResult(includeHocon);
            }

            var config = HoconParser.Parse(hocon, IncludeCallback);

            Assert.Equal(123, config.GetInt("a.x"));
            Assert.Equal("hello", config.GetString("a.y"));
            Assert.Equal(123, config.GetInt("a.b.x"));
            Assert.Equal("hello", config.GetString("a.b.y"));
        }

        [Fact]
        public void CanParseObject()
        {
            var hocon = @"
a {
  b = 1
}
";
            Assert.Equal("1", HoconParser.Parse(hocon).GetString("a.b"));
        }

        [Fact]
        public void CanParseQuotedElements()
        {
            var hocon = @"
A.B = 1
A {
 ""X.Y"" = 1
}
";
            var ex = Record.Exception(() => HoconParser.Parse(hocon).GetObject("A"));
            Assert.Null(ex);
        }

        [Fact]
        public void CanSetDefaultValuesWhenGettingData()
        {
            var emptyConfig = HoconParser.Parse("{}");
            var missingKey = "a";

            emptyConfig.GetInt(missingKey, 0).Should().Be(0);
            emptyConfig.GetDouble(missingKey, 0).Should().Be(0);

            emptyConfig.GetBooleanList(missingKey, new List<bool>()).Should().Equal(new List<bool>());
            emptyConfig.Invoking(c => c.GetBooleanList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetByteList(missingKey, new List<byte>()).Should().Equal(new List<byte>());
            emptyConfig.Invoking(c => c.GetByteList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetDecimalList(missingKey, new List<decimal>()).Should().Equal(new List<decimal>());
            emptyConfig.Invoking(c => c.GetDecimalList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetDoubleList(missingKey, new List<double>()).Should().Equal(new List<double>());
            emptyConfig.Invoking(c => c.GetDoubleList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetFloatList(missingKey, new List<float>()).Should().Equal(new List<float>());
            emptyConfig.Invoking(c => c.GetFloatList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetIntList(missingKey, new List<int>()).Should().Equal(new List<int>());
            emptyConfig.Invoking(c => c.GetIntList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetLongList(missingKey, new List<long>()).Should().Equal(new List<long>());
            emptyConfig.Invoking(c => c.GetLongList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetObjectList(missingKey, new List<HoconObject>()).Should().Equal(new List<HoconObject>());
            emptyConfig.Invoking(c => c.GetObjectList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            emptyConfig.GetStringList(missingKey, new List<string>()).Should().Equal(new List<string>());
            emptyConfig.Invoking(c => c.GetStringList(missingKey)).Should().Throw<HoconValueException>().Which
                .InnerException.Should().BeOfType<KeyNotFoundException>();

            /*
            emptyConfig.Invoking(c => c.GetStringList(missingKey)).Should().NotThrow("String list is an exception of the rule")
                .And.Subject().Should().BeEquivalentTo(new List<string>());
            */
        }

        [Fact]
        public void ValueEqualityShouldWork()
        {
            var hoconStr =
                @"{
    a: string value
    b: [1, 2, 3]
    c: 12
    d: {
        x: string value
        y: [1, 2, 3]
        z: 12
    }
}";
            var hocon1 = HoconParser.Parse(hoconStr);
            var hocon2 = HoconParser.Parse(hoconStr);

            Assert.Equal(hocon1, hocon2);
            Assert.Equal(hocon1.Value, hocon2.Value);

            Assert.Equal(hocon1.GetValue("a"), hocon2.GetValue("a"));
            Assert.Equal(hocon1.GetValue("b"), hocon2.GetValue("b"));
            Assert.Equal(hocon1.GetValue("c"), hocon2.GetValue("c"));
            Assert.Equal(hocon1.GetValue("d"), hocon2.GetValue("d"));

            Assert.Equal(hocon1.GetValue("a"), hocon1.GetValue("d.x"));
            Assert.Equal(hocon1.GetValue("b"), hocon1.GetValue("d.y"));
            Assert.Equal(hocon1.GetValue("c"), hocon1.GetValue("d.z"));

            Assert.Equal(hocon1.GetValue("a"), hocon2.GetValue("d.x"));
            Assert.Equal(hocon1.GetValue("b"), hocon2.GetValue("d.y"));
            Assert.Equal(hocon1.GetValue("c"), hocon2.GetValue("d.z"));

            Assert.Equal(hocon1.GetString("a"), hocon2.GetString("a"));
            Assert.Equal(hocon1.GetString("a"), hocon1.GetString("d.x"));

            Assert.Equal(hocon1.GetValue("b").GetArray(), hocon2.GetValue("b").GetArray());
            Assert.Equal(hocon1.GetValue("b").GetArray(), hocon1.GetValue("d.y").GetArray());
        }

        [Fact]
        public void AtKey_Should_work()
        {
            var initial = HoconParser.Parse("a = 5");
            var config = initial.GetValue("a").AtKey("b");
            config.GetInt("b").Should().Be(5);
            config.HasPath("a").Should().BeFalse();
        }

        [Fact]
        public void CanTrimConcatenatedValue()
        {
            var hocon = "a= \t \t 1 2 3 \t \t,";
            Assert.Equal("1 2 3", HoconParser.Parse(hocon).GetString("a"));
        }

        [Fact]
        public void CanTrimValue()
        {
            var hocon = "a= \t \t 1 \t \t,";
            Assert.Equal("1", HoconParser.Parse(hocon).GetString("a"));
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
            var config = HoconParser.Parse(hocon).Value.GetObject().Unwrapped;

            var a = config["a"] as IDictionary<string, object>;
            Assert.NotNull(a);
            Assert.IsType<Dictionary<string, object>>(a);
            Assert.Contains("b", a.Keys);
            Assert.IsType<Dictionary<string, object>>(a["b"]);

            var b = a["b"] as IDictionary<string, object>;
            Assert.NotNull(b);
            Assert.Contains("c", b.Keys);
            Assert.Contains("d", b.Keys);

            Assert.NotNull(b["c"]);
            Assert.IsType<HoconField>(b["c"]);
            Assert.Equal(1, ((HoconField) b["c"]).Value.GetInt());

            Assert.NotNull(b["d"]);
            Assert.IsType<HoconField>(b["d"]);
            Assert.True(((HoconField) b["d"]).Value.GetBoolean());
        }

        [Fact]
        public void CanUsePathsAsKeys_3()
        {
            var hocon1 = @"3 : 42";
            var hocon2 = @"""3"" : 42";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("3"),
                HoconParser.Parse(hocon2).GetString("3"));
        }

        //Added tests to conform to the HOCON spec https://github.com/typesafehub/config/blob/master/HOCON.md
        [Fact]
        public void CanUsePathsAsKeys_3_14()
        {
            var hocon1 = @"3.14 : 42";
            var hocon2 = @"3 { 14 : 42}";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("3.14"),
                HoconParser.Parse(hocon2).GetString("3.14"));
        }

        [Fact]
        public void CanUsePathsAsKeys_A_B_C()
        {
            var hocon1 = @"a b c : 42";
            var hocon2 = @"""a b c"" : 42";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("a b c"),
                HoconParser.Parse(hocon2).GetString("a b c"));
        }

        [Fact]
        public void CanUsePathsAsKeys_AX_AY()
        {
            var hocon1 = @"a.x : 42, a.y : 43";
            var hocon2 = @"a { x : 42, y : 43 }";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("a.x"),
                HoconParser.Parse(hocon2).GetString("a.x"));
            Assert.Equal(HoconParser.Parse(hocon1).GetString("a.y"),
                HoconParser.Parse(hocon2).GetString("a.y"));
        }

        [Fact]
        public void CanMergeQuotedStrings()
        {
            var hocon = @"akka.actor.deployment { 
                            default { }
                        }
                        akka.actor.deployment { 
                            ""/weird/*"" {
                                router = round-robin-pool
                                nr-of-instances = 2
                              }
                        }";

            HoconParser.Parse(hocon).GetObject(@"akka.actor.deployment.""/weird/*""").Should().NotBeNull();
        }

        [Fact]
        public void CanUsePathsAsKeys_FooBar()
        {
            var hocon1 = @"foo.bar : 42";
            var hocon2 = @"foo { bar : 42 }";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("foo.bar"),
                HoconParser.Parse(hocon2).GetString("foo.bar"));
        }

        [Fact]
        public void CanUsePathsAsKeys_FooBarBaz()
        {
            var hocon1 = @"foo.bar.baz : 42";
            var hocon2 = @"foo { bar { baz : 42 } }";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("foo.bar.baz"),
                HoconParser.Parse(hocon2).GetString("foo.bar.baz"));
        }

        [Fact]
        public void CanUsePathsAsKeys_true()
        {
            var hocon1 = @"true : 42";
            var hocon2 = @"""true"" : 42";
            Assert.Equal(HoconParser.Parse(hocon1).GetString("true"),
                HoconParser.Parse(hocon2).GetString("true"));
        }

        /// <summary>
        ///     Related issue: https://github.com/akkadotnet/HOCON/issues/108
        /// </summary>
        [Fact]
        public void Config_ToString_Should_work_properly()
        {
            var hocon = @"
a {
  b = 1
  c : {
    d = 2
  }
}
";
            var config = HoconParser.Parse(hocon);
            Record.Exception(() => config.ToString()).Should().BeNull();
        }

        [Fact]
        public void FailedIncludeParsingShouldBeParsedAsLiteralInstead()
        {
            var hocon = @"{
  include = include required file(not valid)
  include file = not an include
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("include required file(not valid)", config.GetString("include"));
            Assert.Equal("not an include", config.GetString("include file"));
        }

        [Fact]
        public void Fix_cyclic_substitution_loop_error_Issue128()
        {
            var hocon = @"
c: {
    q: {
        a: [2, 5]
    }
}
c: {
    m: ${c.q} {p: 75}
    m.a: ${c.q.a} [6]
}
";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.Null(ex);
        }

        [Fact]
        public void Fix_substitutions_Issue123()
        {
            var hocon = @"
a: avalue

b {
  b1: ""0001-01-01Z""
  b2: 0
  b_alpha: ${a}/${c.c1}/${b.b3}/${b.b4}
  b_beta: ""[""${b.b_alpha}"",""${b.b1}"",""${b.b2}""]""
}

c {
  c1: c1value
}

b {
  b3: b4value
}

b {
  b4: b4value
}
";

            var config = HoconParser.Parse(hocon);
            config.GetString("b.b_alpha").Should().Be("avalue/c1value/b4value/b4value");
            config.GetString("b.b_beta").Should().Be("[avalue/c1value/b4value/b4value,0001-01-01Z,0]");
        }

        [Fact]
        public void Getter_failures_Should_include_bad_path()
        {
            var badConfig = HoconParser.Parse("{a.c: abc}");
            var badPath = "a.c";

            badConfig.Invoking(c => c.GetInt(badPath)).Should().Throw<HoconValueException>().Which.FailPath.Should()
                .Be(badPath);
            badConfig.Invoking(c => c.GetDouble(badPath)).Should().Throw<HoconValueException>().Which.FailPath.Should()
                .Be(badPath);
            badConfig.Invoking(c => c.GetBooleanList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetByteList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetDecimalList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetDoubleList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetFloatList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetIntList(badPath)).Should().Throw<HoconValueException>().Which.FailPath.Should()
                .Be(badPath);
            badConfig.Invoking(c => c.GetLongList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetObjectList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetStringList(badPath)).Should().Throw<HoconValueException>().Which.FailPath
                .Should().Be(badPath);
            badConfig.Invoking(c => c.GetInt(badPath)).Should().Throw<HoconValueException>().Which.FailPath.Should()
                .Be(badPath);
            badConfig.Invoking(c => c.GetInt(badPath)).Should().Throw<HoconValueException>().Which.FailPath.Should()
                .Be(badPath);
        }

        [Fact]
        public void GettingArrayFromLiteralsShouldThrow()
        {
            var hocon = " literal : a b c";
            HoconParser.Parse(hocon).Invoking(c => c.GetStringList("literal")).Should()
                .Throw<HoconException>("Anything converted to array should throw instead");
        }

        [Fact]
        public void GettingStringFromArrayShouldThrow()
        {
            var hocon = " array : [1,2,3]";
            HoconParser.Parse(hocon).Invoking(c => c.GetStringList("literal")).Should()
                .Throw<HoconException>("Anything converted to array should throw instead");
            //Assert.Null(HoconParser.Parse(hocon).GetString("array"));
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedArray() //undefined behavior in spec
        {
            var hocon = " array : [1,2,3";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedArrayInObject() //undefined behavior in spec
        {
            var hocon = " root { array : [1,2,3 }";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedString() //undefined behavior in spec
        {
            var hocon = " string : \"hello";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedStringInObject() //undefined behavior in spec
        {
            var hocon = " root { string : \"hello }";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }
    }
}