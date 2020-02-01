// -----------------------------------------------------------------------
// <copyright file="QuotedString.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using FluentAssertions;
using Xunit;

namespace Hocon.Tests
{
    public class QuotedString
    {
        /// <summary>
        /// Per https://github.com/akkadotnet/HOCON/issues/190
        /// </summary>
        [Fact(DisplayName = "Bugfix 190 - should unquoted quoted strings on parse")]
        public void Bugfix_190_should_unquote_quotedstrings_on_parse()
        {
            var hocon = @"
                adapters {
                  gremlin = ""Akka.Remote.Transport.FailureInjectorProvider,Akka.Remote""
                  trttl = ""Akka.Remote.Transport.ThrottlerProvider,Akka.Remote""
                }
            ";

            var parsed = HoconParser.Parse(hocon);
            var unwrapped = parsed.GetObject("adapters").ToDictionary(x => x.Key, v => v.Value.GetString());

            // check to make sure these strings aren't quoted
            unwrapped["gremlin"].Should().Be("Akka.Remote.Transport.FailureInjectorProvider,Akka.Remote");
            unwrapped["trttl"].Should().Be("Akka.Remote.Transport.ThrottlerProvider,Akka.Remote");
        }
    }
}
