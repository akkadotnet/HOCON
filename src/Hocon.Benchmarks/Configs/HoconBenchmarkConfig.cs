// -----------------------------------------------------------------------
// <copyright file="HoconBenchmarkConfig.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;

namespace Hocon.Benchmarks.Configs
{
    public class HoconBenchmarkConfig : ManualConfig
    {
        public HoconBenchmarkConfig()
        {
            Add(MemoryDiagnoser.Default);
            Add(MarkdownExporter.GitHub);
        }
    }
}