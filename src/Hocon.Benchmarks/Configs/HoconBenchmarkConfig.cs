#region copyright
// -----------------------------------------------------------------------
//  <copyright file="HoconBenchmarkConfig.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

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