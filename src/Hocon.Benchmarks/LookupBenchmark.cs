// -----------------------------------------------------------------------
// <copyright file="LookupBenchmark.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using BenchmarkDotNet.Attributes;
using Hocon.Benchmarks.Configs;
using BenchmarkConfig = BenchmarkDotNet.Attributes.ConfigAttribute;

namespace Hocon.Benchmarks
{
    [BenchmarkConfig(typeof(HoconBenchmarkConfig))]
    public class LookupBenchmark
    {
        private Config config;

        [GlobalSetup]
        public void Setup()
        {
            config = HoconConfigurationFactory.ParseString(TestConfigStrings.AkkaConfigString);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
        }

        [Benchmark]
        public Config LookupPathNest1()
        {
            return config.GetConfig("akka");
        }

        [Benchmark]
        public Config LookupPathNest3()
        {
            return config.GetConfig("akka.actor.deployment");
        }

        [Benchmark]
        public Config LookupPathNest5()
        {
            return config.GetConfig("akka.actor.deployment.default.resizer");
        }

        [Benchmark]
        public string LookupString()
        {
            return config.GetString("akka.actor.provider");
        }

        [Benchmark]
        public int LookupInt()
        {
            return config.GetInt("akka.actor.reaper-interval");
        }

        [Benchmark]
        public bool LookupBoolean()
        {
            return config.GetBoolean("akka.actor.serialize-creators");
        }

        [Benchmark]
        public TimeSpan LookupTimeSpan()
        {
            return config.GetTimeSpan("akka.actor.creation-timeout");
        }

        [Benchmark]
        public double LookupDouble()
        {
            return config.GetDouble("akka.actor.reaper-interval");
        }
    }
}