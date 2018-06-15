using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class Substitution
    {
        [Fact]
        public void CanConcatenateUnquotedString()
        {
            var hocon = @"a {
  name = Roger
  c = Hello my name is ${a.name}
}";
            Assert.Equal("Hello my name is Roger", ConfigurationFactory.ParseString(hocon).GetString("a.c"));
        }

        [Fact]
        public void CanConcatenateQuotedString()
        {
            var hocon = @"a {
  name = Roger
  c = ""Hello my name is ""${a.name}
}";
            Assert.Equal("Hello my name is Roger", ConfigurationFactory.ParseString(hocon).GetString("a.c"));
        }

        [Fact]
        public void CanConcatenateArray()
        {
            var hocon = @"a {
  b = [1,2,3]
  c = ${a.b} [4,5,6]
}";
            Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("a.c")));
        }

        [Fact]
        public void QuestionMarkConcatenateArrayShouldFailSilently()
        {
            var hocon = @"a {
  c = ${?a.b} [4,5,6]
}";
            Assert.True(new[] { 4, 5, 6 }.SequenceEqual(ConfigurationFactory.ParseString(hocon).GetIntList("a.c")));
        }

        [Fact]
        public void QuestionMarkShouldFailSilently()
        {
            var hocon = @"a {
  b = ${?a.c}
}";
            ConfigurationFactory.ParseString(hocon);
        }

        [Fact]
        public void QuestionMarkShouldFallbackToEnvironmentVariables()
        {
            var hocon = @"a {
  b = ${?MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Equal(value, ConfigurationFactory.ParseString(hocon).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }


    }
}
