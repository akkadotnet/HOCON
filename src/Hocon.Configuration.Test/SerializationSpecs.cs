// -----------------------------------------------------------------------
// <copyright file="SerializationSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Hocon.Configuration.Tests
{
    public class SerializationSpecs
    {
        public static IEnumerable<object[]> HoconGenerator()
        {
            yield return new object[]
            {
                @"
                    foo{
                      bar.biz = 12
                      baz = ""quoted""
                    }
                ", string.Empty, string.Empty
            };
        }

        [Theory(Skip = "Doesn't work right now")]
        [MemberData(nameof(HoconGenerator))]
        public void ShouldSerializeHocon(string hocon, string fallback1, string fallback2)
        {
            var hocon1 = ConfigurationFactory.ParseString(hocon);
            var fb1 = string.IsNullOrEmpty(fallback1) ? Config.Empty : ConfigurationFactory.ParseString(fallback1);
            var fb2 = string.IsNullOrEmpty(fallback1) ? Config.Empty : ConfigurationFactory.ParseString(fallback2);

            var final = hocon1.WithFallback(fb1).WithFallback(fb2);

            VerifySerialization(final);
        }

        private void VerifySerialization(Config config)
        {
            var serialized = JsonConvert.SerializeObject(config);
            var deserialized = (Config)JsonConvert.DeserializeObject(serialized);
            config.DumpConfig().Should().Be(deserialized.DumpConfig());
        }
    }
}
