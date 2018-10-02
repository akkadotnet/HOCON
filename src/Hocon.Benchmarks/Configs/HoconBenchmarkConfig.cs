#region copyright
// -----------------------------------------------------------------------
//  <copyright file="HoconBenchmarkConfig.cs" company="Hocon Project">
//      Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//      Copyright (C) 2018-2018 Akka.NET project <https://github.com/akkadotnet/hocon>
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