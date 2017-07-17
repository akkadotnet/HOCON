//-----------------------------------------------------------------------
// <copyright file="HoconTests.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using NUnit.Framework;
using Hocon.Tests;
using System.Collections.Generic;

namespace Akka.Tests.Configuration
{
    [TestFixture]
    public class HoconTests
    {
        [TestCase]
        public void CanUnwrapSubConfig() //undefined behavior in spec, this does not behave the same as JVM hocon.
        {
            var hocon = @"
a {
   b {
     c = 1
     d = true
   }
}";
            var config = ConfigurationFactory.ParseString(hocon).Root.GetObject().Unwrapped;
            var a = config["a"] as IDictionary<string, object>;
            var b = a["b"] as IDictionary<string, object>;
            (b["c"] as HoconValue).GetInt().ShouldBe(1);
            (b["d"] as HoconValue).GetBoolean().ShouldBe(true);
        }

        [TestCase]
        public void ThrowsParserExceptionOnUnterminatedObject() //undefined behavior in spec
        {
            var hocon = " root { string : \"hello\" ";
            Assert.Throws<HoconParserException>(() => 
                ConfigurationFactory.ParseString(hocon));
        }

        [TestCase]
        public void ThrowsParserExceptionOnUnterminatedNestedObject() //undefined behavior in spec
        {
            var hocon = " root { bar { string : \"hello\" } ";
            Assert.Throws<HoconParserException>(() =>
                ConfigurationFactory.ParseString(hocon));
        }

        [TestCase]
        public void ThrowsParserExceptionOnUnterminatedString() //undefined behavior in spec
        {
            var hocon = " string : \"hello";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        [TestCase]
        public void ThrowsParserExceptionOnUnterminatedStringInObject() //undefined behavior in spec
        {
            var hocon = " root { string : \"hello }";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        [TestCase]
        public void ThrowsParserExceptionOnUnterminatedArray() //undefined behavior in spec
        {
            var hocon = " array : [1,2,3";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        [TestCase]
        public void ThrowsParserExceptionOnUnterminatedArrayInObject() //undefined behavior in spec
        {
            var hocon = " root { array : [1,2,3 }";
            Assert.Throws<HoconParserException>(() => ConfigurationFactory.ParseString(hocon));
        }

        [TestCase]
        public void GettingStringFromArrayReturnsNull() //undefined behavior in spec
        {
            var hocon = " array : [1,2,3]";
            ConfigurationFactory.ParseString(hocon).GetString("array").ShouldBe(null);
        }


        //TODO: not sure if this is the expected behavior but it is what we have established in Akka.NET
        [TestCase]
        public void GettingArrayFromLiteralsReturnsNull() //undefined behavior in spec
        {
            var hocon = " literal : a b c";
            var res = ConfigurationFactory.ParseString(hocon).GetStringList("literal");

            Assert.That(res, Is.Empty);
        }

        //Added tests to conform to the HOCON spec https://github.com/typesafehub/config/blob/master/HOCON.md
        [TestCase]
        public void CanUsePathsAsKeys_3_14()
        {
            var hocon1 = @"3.14 : 42";
            var hocon2 = @"3 { 14 : 42}";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("3.14"),
                ConfigurationFactory.ParseString(hocon2).GetString("3.14"));
        }

        [TestCase]
        public void CanUsePathsAsKeys_3()
        {
            var hocon1 = @"3 : 42";
            var hocon2 = @"""3"" : 42";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("3"),
                ConfigurationFactory.ParseString(hocon2).GetString("3"));
        }

        [TestCase]
        public void CanUsePathsAsKeys_true()
        {
            var hocon1 = @"true : 42";
            var hocon2 = @"""true"" : 42";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("true"),
                ConfigurationFactory.ParseString(hocon2).GetString("true"));
        }

        [TestCase]
        public void CanUsePathsAsKeys_FooBar()
        {
            var hocon1 = @"foo.bar : 42";
            var hocon2 = @"foo { bar : 42 }";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("foo.bar"),
                ConfigurationFactory.ParseString(hocon2).GetString("foo.bar"));
        }

        [TestCase]
        public void CanUsePathsAsKeys_FooBarBaz()
        {
            var hocon1 = @"foo.bar.baz : 42";
            var hocon2 = @"foo { bar { baz : 42 } }";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("foo.bar.baz"),
                ConfigurationFactory.ParseString(hocon2).GetString("foo.bar.baz"));
        }

        [TestCase]
        public void CanUsePathsAsKeys_AX_AY()
        {
            var hocon1 = @"a.x : 42, a.y : 43";
            var hocon2 = @"a { x : 42, y : 43 }";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("a.x"),
                ConfigurationFactory.ParseString(hocon2).GetString("a.x"));
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("a.y"),
                ConfigurationFactory.ParseString(hocon2).GetString("a.y"));
        }

        [TestCase]
        public void CanUsePathsAsKeys_A_B_C()
        {
            var hocon1 = @"a b c : 42";
            var hocon2 = @"""a b c"" : 42";
            Assert.AreEqual(ConfigurationFactory.ParseString(hocon1).GetString("a b c"),
                ConfigurationFactory.ParseString(hocon2).GetString("a b c"));
        }


        [TestCase]
        public void CanConcatenateSubstitutedUnquotedString()
        {
            var hocon = @"a {
  name = Roger
  c = Hello my name is ${a.name}
}";
            Assert.AreEqual("Hello my name is Roger", ConfigurationFactory.ParseString(hocon).GetString("a.c"));
        }

        [TestCase]
        public void CanConcatenateSubstitutedArray()
        {
            var hocon = @"a {
  b = [1,2,3]
  c = ${a.b} [4,5,6]
}";
            Assert.True(new[] {1, 2, 3, 4, 5, 6}.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("a.c")));
        }

        [TestCase]
        public void CanParseSubConfig()
        {
            var hocon = @"
a {
   b {
     c = 1
     d = true
   }
}";
            var config = ConfigurationFactory.ParseString(hocon);
            var subConfig = config.GetConfig("a");
            Assert.AreEqual(1, subConfig.GetInt("b.c"));
            Assert.AreEqual(true, subConfig.GetBoolean("b.d"));
        }


        [TestCase]
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
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.AreEqual("1", config.GetString("root.int"));
            Assert.AreEqual("1.23", config.GetString("root.double"));
            Assert.AreEqual(true, config.GetBoolean("root.bool"));
            Assert.AreEqual(true, config.GetBoolean("root.object.hasContent"));
            Assert.AreEqual(null, config.GetString("root.null"));
            Assert.AreEqual("foo", config.GetString("root.quoted-string"));
            Assert.AreEqual("bar", config.GetString("root.unquoted-string"));
            Assert.AreEqual("foo bar", config.GetString("root.concat-string"));
            Assert.True(
                new[] {1, 2, 3, 4}.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("root.array")));
            Assert.True(
                new[] {1, 2, 3, 4}.SequenceEqual(
                    ConfigurationFactory.ParseString(hocon).GetIntList("root.array-newline-element")));
            Assert.True(
                new[] {"1 2 3 4"}.SequenceEqual(
                    ConfigurationFactory.ParseString(hocon).GetStringList("root.array-single-element")));
        }

        [TestCase]
        public void CanParseJson()
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
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.AreEqual("1", config.GetString("root.int"));
            Assert.AreEqual("1.23", config.GetString("root.double"));
            Assert.AreEqual(true, config.GetBoolean("root.bool"));
            Assert.AreEqual(true, config.GetBoolean("root.object.hasContent"));
            Assert.AreEqual(null, config.GetString("root.null"));
            Assert.AreEqual("foo", config.GetString("root.string"));
            Assert.True(new[] {1, 2, 3}.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("root.array")));
        }

        [TestCase]
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
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.AreEqual("1", config.GetString("a.b.c.x"));
            Assert.AreEqual("2", config.GetString("a.b.c.y"));
            Assert.AreEqual("3", config.GetString("a.b.c.z"));
        }

        [TestCase]
        public void CanOverrideObject()
        {
            var hocon = @"
a.b = 1
a = null
a.c = 3
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.AreEqual(null, config.GetString("a.b"));
            Assert.AreEqual("3", config.GetString("a.c"));
        }

        [TestCase]
        public void CanParseObject()
        {
            var hocon = @"
a {
  b = 1
}
";
            Assert.AreEqual("1", ConfigurationFactory.ParseString(hocon).GetString("a.b"));
        }

        [TestCase]
        public void CanTrimValue()
        {
            var hocon = "a= \t \t 1 \t \t,";
            Assert.AreEqual("1", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanTrimConcatenatedValue()
        {
            var hocon = "a= \t \t 1 2 3 \t \t,";
            Assert.AreEqual("1 2 3", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanConsumeCommaAfterValue()
        {
            var hocon = "a=1,";
            Assert.AreEqual("1", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignIpAddressToField()
        {
            var hocon = @"a=127.0.0.1";
            Assert.AreEqual("127.0.0.1", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignConcatenatedValueToField()
        {
            var hocon = @"a=1 2 3";
            Assert.AreEqual("1 2 3", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignValueToQuotedField()
        {
            var hocon = @"""a""=1";
            Assert.AreEqual(1L, ConfigurationFactory.ParseString(hocon).GetLong("a"));
        }

        [TestCase]
        public void CanAssignValueToPathExpression()
        {
            var hocon = @"a.b.c=1";
            Assert.AreEqual(1L, ConfigurationFactory.ParseString(hocon).GetLong("a.b.c"));
        }

        [TestCase]
        public void CanAssignValuesToPathExpressions()
        {
            var hocon = @"
a.b.c=1
a.b.d=2
a.b.e.f=3
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.AreEqual(1L, config.GetLong("a.b.c"));
            Assert.AreEqual(2L, config.GetLong("a.b.d"));
            Assert.AreEqual(3L, config.GetLong("a.b.e.f"));
        }

        [TestCase]
        public void CanAssignLongToField()
        {
            var hocon = @"a=1";
            Assert.AreEqual(1L, ConfigurationFactory.ParseString(hocon).GetLong("a"));
        }

        [TestCase]
        public void CanAssignArrayToField()
        {
            var hocon = @"a=
[
    1
    2
    3
]";
            Assert.True(new[] {1, 2, 3}.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("a")));

            //hocon = @"a= [ 1, 2, 3 ]";
            //Assert.True(new[] { 1, 2, 3 }.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("a")));
        }

        [TestCase]
        public void CanConcatenateArray()
        {
            var hocon = @"a=[1,2] [3,4]";
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("a")));
        }

        [TestCase]
        public void CanAssignSubstitutionToField()
        {
            var hocon = @"a{
    b = 1
    c = ${a.b}
    d = ${a.c}23
}";
            Assert.AreEqual(1, ConfigurationFactory.ParseString(hocon).GetInt("a.c"));
            Assert.AreEqual(123, ConfigurationFactory.ParseString(hocon).GetInt("a.d"));
        }

        [TestCase]
        public void CanAssignDoubleToField()
        {
            var hocon = @"a=1.1";
            Assert.AreEqual(1.1, ConfigurationFactory.ParseString(hocon).GetDouble("a"));
        }

        [TestCase]
        public void CanAssignNullToField()
        {
            var hocon = @"a=null";
            Assert.Null(ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignBooleanToField()
        {
            var hocon = @"a=true";
            Assert.AreEqual(true, ConfigurationFactory.ParseString(hocon).GetBoolean("a"));
            hocon = @"a=false";
            Assert.AreEqual(false, ConfigurationFactory.ParseString(hocon).GetBoolean("a"));

            hocon = @"a=on";
            Assert.AreEqual(true, ConfigurationFactory.ParseString(hocon).GetBoolean("a"));
            hocon = @"a=off";
            Assert.AreEqual(false, ConfigurationFactory.ParseString(hocon).GetBoolean("a"));
        }

        [TestCase]
        public void CanAssignQuotedStringToField()
        {
            var hocon = @"a=""hello""";
            Assert.AreEqual("hello", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignTrippleQuotedStringToField()
        {
            var hocon = @"a=""""""hello""""""";
            Assert.AreEqual("hello", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignUnQuotedStringToField()
        {
            var hocon = @"a=hello";
            Assert.AreEqual("hello", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanAssignTripleQuotedStringToField()
        {
            var hocon = @"a=""""""hello""""""";
            Assert.AreEqual("hello", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanUseFallback()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);

            var config = config1.WithFallback(config2);

            Assert.AreEqual(123, config.GetInt("foo.bar.a"));
            Assert.AreEqual(2, config.GetInt("foo.bar.b"));
            Assert.AreEqual(3, config.GetInt("foo.bar.c"));
        }

        [TestCase]
        public void CanUseFallbackInSubConfig()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);

            var config = config1.WithFallback(config2).GetConfig("foo.bar");

            Assert.AreEqual(123, config.GetInt("a"));
            Assert.AreEqual(2, config.GetInt("b"));
            Assert.AreEqual(3, config.GetInt("c"));
        }

        [TestCase]
        public void CanUseMultiLevelFallback()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";
            var hocon3 = @"
foo {
   bar {
      a=99
      zork=555
   }
}";
            var hocon4 = @"
foo {
   bar {
      borkbork=-1
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);
            var config3 = ConfigurationFactory.ParseString(hocon3);
            var config4 = ConfigurationFactory.ParseString(hocon4);

            var config = config1.WithFallback(config2.WithFallback(config3.WithFallback(config4)));

            config.GetInt("foo.bar.a").ShouldBe(123);
            config.GetInt("foo.bar.b").ShouldBe(2);
            config.GetInt("foo.bar.c").ShouldBe(3);
            config.GetInt("foo.bar.zork").ShouldBe(555);
            config.GetInt("foo.bar.borkbork").ShouldBe(-1);
        }

        [TestCase]
        public void CanUseFluentMultiLevelFallback()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";
            var hocon3 = @"
foo {
   bar {
      a=99
      zork=555
   }
}";
            var hocon4 = @"
foo {
   bar {
      borkbork=-1
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);
            var config3 = ConfigurationFactory.ParseString(hocon3);
            var config4 = ConfigurationFactory.ParseString(hocon4);

            var config = config1.WithFallback(config2).WithFallback(config3).WithFallback(config4);

            config.GetInt("foo.bar.a").ShouldBe(123);
            config.GetInt("foo.bar.b").ShouldBe(2);
            config.GetInt("foo.bar.c").ShouldBe(3);
            config.GetInt("foo.bar.zork").ShouldBe(555);
            config.GetInt("foo.bar.borkbork").ShouldBe(-1);
        }

        [TestCase]
        public void CanParseQuotedKeys()
        {
            var hocon = @"
a {
   ""some quoted, key"": 123
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            config.GetInt("a.some quoted, key").ShouldBe(123);
        }

        [TestCase]
        public void StringElementIsNotArray()
        {
            var hocon = @"
{
   ""bla"": ""bla""
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            config.AsEnumerable().First().Value.IsArray().ShouldBe(false);
        }

        [TestCase]
        public void ArrayElementIsArray()
        {
            var hocon = @"
{
   ""bla"": [""bla""]
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            config.AsEnumerable().First().Value.IsArray().ShouldBe(true);
        }

        [TestCase]
        public void CanEnumerateQuotedKeys()
        {
            var hocon = @"
a {
   ""some quoted, key"": 123
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            var config2 = config.GetConfig("a");
            var enumerable = config2.AsEnumerable();

            enumerable.Select(kvp => kvp.Key).First().ShouldBe("some quoted, key");
        }

        [TestCase]
        public void CanParseSerializersAndBindings()
        {
            var hocon = @"
akka.actor {
    serializers {
      akka-containers = ""Akka.Remote.Serialization.MessageContainerSerializer, Akka.Remote""
      proto = ""Akka.Remote.Serialization.ProtobufSerializer, Akka.Remote""
      daemon-create = ""Akka.Remote.Serialization.DaemonMsgCreateSerializer, Akka.Remote""
    }

    serialization-bindings {
      # Since com.google.protobuf.Message does not extend Serializable but
      # GeneratedMessage does, need to use the more specific one here in order
      # to avoid ambiguity
      ""Akka.Actor.ActorSelectionMessage"" = akka-containers
      ""Akka.Remote.DaemonMsgCreate, Akka.Remote"" = daemon-create
    }

}";

            var config = ConfigurationFactory.ParseString(hocon);

            var serializersConfig = config.GetConfig("akka.actor.serializers").AsEnumerable().ToList();
            var serializerBindingConfig = config.GetConfig("akka.actor.serialization-bindings").AsEnumerable().ToList();

            serializersConfig.Select(kvp => kvp.Value)
                .First()
                .GetString()
                .ShouldBe("Akka.Remote.Serialization.MessageContainerSerializer, Akka.Remote");
            serializerBindingConfig.Select(kvp => kvp.Key).Last().ShouldBe("Akka.Remote.DaemonMsgCreate, Akka.Remote");
        }

        [TestCase]
        public void CanOverwriteValue()
        {
            var hocon = @"
test {
  value  = 123
}
test.value = 456
";
            var config = ConfigurationFactory.ParseString(hocon);
            config.GetInt("test.value").ShouldBe(456);
        }

        [TestCase]
        public void CanCSubstituteObject()
        {
            var hocon = @"a {
  b {
      foo = hello
      bar = 123
  }
  c {
     d = xyz
     e = ${a.b}
  }  
}";
            var ace = ConfigurationFactory.ParseString(hocon).GetConfig("a.c.e");
            Assert.AreEqual("hello", ace.GetString("foo"));
            Assert.AreEqual(123, ace.GetInt("bar"));
        }

        [TestCase]
        public void CanAssignNullStringToField()
        {
            var hocon = @"a=null";
            Assert.AreEqual(null, ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
		[Ignore("we currently do not make any destinction between quoted and unquoted strings once parsed")]
        public void CanAssignQuotedNullStringToField()
        {
            var hocon = @"a=""null""";
            Assert.AreEqual("null", ConfigurationFactory.ParseString(hocon).GetString("a"));
        }

        [TestCase]
        public void CanParseInclude()
        {
            var hocon = @"a {
  b { 
       include ""foo""
  }";
            var includeHocon = @"
x = 123
y = hello
";
            Func<string, HoconRoot> include = s => Parser.Parse(includeHocon, null);
            var config = ConfigurationFactory.ParseString(hocon,include);

            Assert.AreEqual(123,config.GetInt("a.b.x"));
            Assert.AreEqual("hello", config.GetString("a.b.y"));
        }

        [TestCase]
        public void CanResolveSubstitutesInInclude()
        {
            var hocon = @"a {
  b { 
       include ""foo""
  }";
            var includeHocon = @"
x = 123
y = ${x}
";
            Func<string, HoconRoot> include = s => Parser.Parse(includeHocon, null);
            var config = ConfigurationFactory.ParseString(hocon, include);

            Assert.AreEqual(123, config.GetInt("a.b.x"));
            Assert.AreEqual(123, config.GetInt("a.b.y"));
        }

        [TestCase]
        public void CanResolveSubstitutesInNestedIncludes()
        {
            var hocon = @"a.b.c {
  d { 
       include ""foo""
  }";
            var includeHocon = @"
f = 123
e {
      include ""foo""
}
";

            var includeHocon2 = @"
x = 123
y = ${x}
";

            Func<string, HoconRoot> include2 = s => Parser.Parse(includeHocon2, null);
            Func<string, HoconRoot> include = s => Parser.Parse(includeHocon, include2);
            var config = ConfigurationFactory.ParseString(hocon, include);

            Assert.AreEqual(123, config.GetInt("a.b.c.d.e.x"));
            Assert.AreEqual(123, config.GetInt("a.b.c.d.e.y"));
        }
    }
}

