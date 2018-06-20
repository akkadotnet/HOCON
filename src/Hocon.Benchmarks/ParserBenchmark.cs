#region copyright
// -----------------------------------------------------------------------
//  <copyright file="ParserBenchmark.cs" company="Hocon Project">
//      Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//      Copyright (C) 2018-2018 Akka.NET project <https://github.com/akkadotnet/hocon>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

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
        public Config ParseHugeConfig()
        {
            return ConfigurationFactory.ParseString(TestConfigStrings.AkkaConfigString);
        }
    }
}