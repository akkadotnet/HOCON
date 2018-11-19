#region copyright
// -----------------------------------------------------------------------
//  <copyright file="ParserBenchmark.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
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
        public Config ParseAkkaConfig()
        {
            return ConfigurationFactory.ParseString(TestConfigStrings.AkkaConfigString);
        }
    }
}