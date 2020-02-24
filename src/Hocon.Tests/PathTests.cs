// -----------------------------------------------------------------------
// <copyright file="PathTests.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Tests
{
    public class PathTests
    {
        public PathTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Theory]
        [InlineData(new[] {"foo", "bar"}, "foo.bar")]
        [InlineData(new[] {"foo", "bar.baz"}, "foo.\"bar.baz\"")]
        [InlineData(new[] {"shoot", "a \"laser\" beam"}, "shoot.\"a \\\"laser\\\" beam\"")]
        [InlineData(new[] {"foo", "bar\nbaz"}, "foo.\"bar\\nbaz\"")]
        [InlineData(new[] {"foo", "bar baz", " wis "}, "foo.bar baz. wis ")]
        [InlineData(new[] {"foo", "bar\tbaz"}, "foo.bar\tbaz")]
        [InlineData(new[] {"foo", "bar\r\nbaz", "x\r"}, "foo.\"bar\r\\nbaz\".x\r")]
        [InlineData(new[] {"foo", ""}, "foo.\"\"")]
        [InlineData(new[] {"foo", "\\"}, "foo.\"\\\\\"")]
        [InlineData(new[] {"$\"{}[]:=,#`^?!@*&\\"}, "\"" + "$\\\"{}[]:=,#`^?!@*&\\\\" + "\"")]
        public void CanParseAndSerialize(string[] pathKeys, string path)
        {
            HoconPath hoconPath = new HoconPath(pathKeys);
            HoconPath hoconPath2 = HoconPath.Parse(path);

            Assert.Equal(path, hoconPath.Value);
            Assert.Equal(path, hoconPath2.Value);

            Assert.Equal(hoconPath, hoconPath2);

            Assert.Equal(pathKeys.Length, hoconPath.Count);
            Assert.Equal(pathKeys.Length, hoconPath2.Count);

            for (int i = 0; i < pathKeys.Length; i++)
            {
                Assert.Equal(pathKeys[i], hoconPath[i]);
                Assert.Equal(pathKeys[i], hoconPath2[i]);
            }

            _output.WriteLine($"Path [{path}] serialized from: {string.Join(", ", pathKeys)}");
        }

        // Test that #79 invalid token exception does not happen.
        [Fact]
        public void DotAndCommaInQuotedKeyShouldBePreserved_79()
        {
            const string hocon = @"
akka {
  persistence {
    journal {
      plugin = ""akka.persistence.journal.sql - server""
      sql-server {
        class = ""Akka.Persistence.SqlServer.Journal.BatchingSqlServerJournal, Akka.Persistence.SqlServer""
        schema-name = dbo
        table-name = EventJournal
        auto-initialize = off
        event-adapters
        {
          json-adapter = ""Demo.EventAdapter, Demo""
        }
        event-adapter-bindings
        {
          ""Demo.IMyEvent, MyDemoAssembly"" = json-adapter #this line makes a invalid token exception
        }
      }
    }
  }
}";
            var config = HoconParser.Parse(hocon);
            var path = HoconPath.Parse(
                "akka.persistence.journal.sql-server.event-adapter-bindings.\"Demo.IMyEvent, MyDemoAssembly\"");
            Assert.Equal("Demo.IMyEvent, MyDemoAssembly", path.Last());
            Assert.Equal("json-adapter", config.GetString(path));
        }

        [Fact]
        public void PathToStringQuoteKeysContainingDot()
        {
            var path1 = new HoconPath(new[]
            {
                "i am",
                "kong.fu",
                "panda"
            });

            var path2 = new HoconPath(new[]
            {
                "i am",
                "kong",
                "fu",
                "panda"
            });

            // path1 => i am."kong.fu".panda 
            // path2 => i am.kong.fu.panda
            Assert.NotEqual(path1.Value, path2.Value);
            Assert.Equal("i am.\"kong.fu\".panda", path1.Value);
        }

        [Fact]
        public void QuotedKeyShouldHandleInvalidCharacters()
        {
            var hoconString = @"this.""should[]"".work = true";
            var config = HoconParser.Parse(hoconString);
            Assert.True(config.GetBoolean("this.\"should[]\".work"));
        }
    }
}