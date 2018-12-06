using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Tests
{
    public class PathTests
    {
        private readonly ITestOutputHelper _output;

        public PathTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(new string[] { "foo", "bar" }, "foo.bar")]
        [InlineData(new string[] { "foo", "bar.baz" }, "foo.\"bar.baz\"")]
        [InlineData(new string[] { "shoot", "a \"laser\" beam" }, "shoot.\"a \\\"laser\\\" beam\"")]
        [InlineData(new string[] { "foo", "bar\nbaz" }, "foo.\"bar\\nbaz\"")]
        [InlineData(new string[] { "foo", "bar baz", " wis " }, "foo.bar baz. wis ")]
        [InlineData(new string[] { "foo", "bar\tbaz"}, "foo.bar\tbaz")]
        [InlineData(new string[] { "foo", "bar\r\nbaz", "x\r" }, "foo.\"bar\r\\nbaz\".x\r")]
        [InlineData(new string[] { "foo", "" }, "foo.\"\"")]
        [InlineData(new string[] { "foo", "\\" }, "foo.\"\\\\\"")]
        [InlineData(new string[] { "$\"{}[]:=,#`^?!@*&\\" }, "\"" + "$\\\"{}[]:=,#`^?!@*&\\\\" + "\"")]
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

            _output.WriteLine(string.Format("Path [{0}] serialized from: {1}", path, string.Join(", ", pathKeys)));
        }

        [Fact]
        public void PathToStringQuoteKeysContainingDot()
        {
            var path1 = new HoconPath(new string[] {
                "i am",
                "kong.fu",
                "panda"
            });

            var path2 = new HoconPath(new string[]
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
    }
}
