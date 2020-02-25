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

        [Theory]
        [MemberData(nameof(HoconGenerator))]
        public void ShouldSerializeHocon(string hocon, string fallback1, string fallback2)
        {
            var hocon1 = HoconConfigurationFactory.ParseString(hocon);
            var fb1 = string.IsNullOrEmpty(fallback1) ? Config.Empty : HoconConfigurationFactory.ParseString(fallback1);
            var fb2 = string.IsNullOrEmpty(fallback1) ? Config.Empty : HoconConfigurationFactory.ParseString(fallback2);

            var final = hocon1.WithFallback(fb1).WithFallback(fb2);

            VerifySerialization(final);
        }

        private void VerifySerialization(Config config)
        {
            var serialized = JsonConvert.SerializeObject(config);
            var deserialized = JsonConvert.DeserializeObject<Config>(serialized);
            config.DumpConfig().Should().Be(deserialized.DumpConfig());
        }

        [Fact]
        public void CanSerializeEmptyConfig()
        {
            var config = Config.Empty;
            var serialized = JsonConvert.SerializeObject(config);
            var deserialized = JsonConvert.DeserializeObject<Config>(serialized);
            deserialized.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void ShouldResolveEmptyToEmpty()
        {
            HoconConfigurationFactory.Empty.IsEmpty.Should().BeTrue();
            HoconConfigurationFactory.ParseString("{}").IsEmpty.Should().BeTrue();
            Config.Empty.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void QuotedKeyWithInvalidCharactersShouldSerializeProperly()
        {
            var hoconString = @"this.""should[]"".work = true";
            var config = HoconConfigurationFactory.ParseString(hoconString);
            var serialized = JsonConvert.SerializeObject(config);
            var deserialized = JsonConvert.DeserializeObject<Config>(serialized);

            Assert.True(config.GetBoolean("this.\"should[]\".work"));
            Assert.True(deserialized.GetBoolean("this.\"should[]\".work"));
            Assert.Equal(config, deserialized);
        }

    }
}
