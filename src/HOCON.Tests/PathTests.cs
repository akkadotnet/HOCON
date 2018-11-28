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

        [Fact]
        public void PathToStringAddQuotesIfRequired()
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

            // path1 => "i am"."kong.fu".panda 
            // path2 => "i am".kong.fu.panda
            Assert.NotEqual(path1.Value, path2.Value);
            Assert.Equal("\"i am\".\"kong.fu\".panda", path1.Value);
        }

        [Fact]
        public void PathToStringEscapeQuotesInKey()
        {
            var path = new HoconPath(new string[]
            {
                "please",
                "es\"cape",
                "me"
            });

            // please."es\"cape".me
            Assert.Equal("please.\"es\\\"cape\".me", path.Value);
        }
    }
}
