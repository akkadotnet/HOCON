using System;
using FluentAssertions;
using Hocon;
using Newtonsoft.Json;

namespace SerializationDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            var hocon1 = @"
                    foo{
                      bar.biz = 12
                      baz = ""quoted""
                      some.string: ""with a backslash (\\) in it""
                    }";

            ShouldSerializeHocon(hocon1, string.Empty, string.Empty);
        }

        public static void ShouldSerializeHocon(string hocon, string fallback1, string fallback2)
        {
            var hocon1 = HoconConfigurationFactory.ParseString(hocon);
            var fb1 = string.IsNullOrEmpty(fallback1) ? Config.Empty : HoconConfigurationFactory.ParseString(fallback1);
            var fb2 = string.IsNullOrEmpty(fallback1) ? Config.Empty : HoconConfigurationFactory.ParseString(fallback2);

            var final = hocon1.WithFallback(fb1).WithFallback(fb2);

            VerifySerialization(final);
        }

        public static void VerifySerialization(Config config)
        {
            var serialized = JsonConvert.SerializeObject(config);
            var deserialized = JsonConvert.DeserializeObject<Config>(serialized);
            config.DumpConfig().Should().Be(deserialized.DumpConfig());
        }
    }
}
