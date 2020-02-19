// -----------------------------------------------------------------------
// <copyright file="ParserBenchmark.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using Hocon.Benchmarks.Configs;
using BenchmarkConfig = BenchmarkDotNet.Attributes.ConfigAttribute;

namespace Hocon.Benchmarks
{
    [BenchmarkConfig(typeof(HoconBenchmarkConfig))]
    public class ParserBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
        }

        [GlobalCleanup]
        public void Cleanup()
        {
        }

        [Benchmark]
        public Config ParseAkkaConfig()
        {
            return ConfigurationFactory.ParseString(TestConfigStrings.AkkaConfigString);
        }
    }
}