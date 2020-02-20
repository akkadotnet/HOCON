using Hocon;
using NBench;

namespace Hocon.Tests.Performance
{
    public class ConfigLookupSpec
    {
        private const string ConfigLookupName = "LookupOp";
        private const double MinimumAcceptableOperationsPerSecond = 80000.0d; 

        private Counter _throughputCounter;

        private static readonly Config TestConfig;
        private static readonly Config ConfigWithCache;

        static ConfigLookupSpec()
        {
            Config junk1 = "a.a = junk";
            Config junk2 = "a.b.a = junk";
            Config junk3 = "a.b.c.a = junk";
            Config junk4 = "a.b.c.d.a = junk";
            Config junk5 = "a.b.c.d.e.a = junk";
            Config junk6 = "a.b.c.d.e.f.a = junk";
            Config junk7 = "a.b.c.d.e.f.g.a = junk";
            Config junk8 = "a.b.c.d.e.f.g.h.a = junk";
            Config junk9 = "a.b.c.d.e.f.g.h.i.a = junk";

            TestConfig = junk1
                .WithFallback(junk2)
                .WithFallback(junk3)
                .WithFallback(junk4)
                .WithFallback(junk5)
                .WithFallback(junk6)
                .WithFallback(junk7)
                .WithFallback(junk8)
                .WithFallback(junk9)
                .WithFallback(HoconConfigurationFactory.ParseString(@"
a.b.c.d.e.f.g.h.i.val = 10
a.b.c.val = 10"));
            TestConfig.UseCache = false;

            ConfigWithCache = new Config(TestConfig);
        }

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _throughputCounter = context.GetCounter(ConfigLookupName);
        }
        #region 10 deep spec
        private static readonly string String_10_Deep = "a.b.c.d.e.f.g.h.i.val";
        private static readonly HoconPath HoconPath_10_Deep = new HoconPath(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "val" });

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 10 deep path Get operation using a string path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_String_10_Deep_throughput(BenchmarkContext context)
        {
            TestConfig.GetString(String_10_Deep);
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 10 deep path Get operation using a HoconPath path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_HoconPath_10_Deep_throughput(BenchmarkContext context)
        {
            TestConfig.GetString(HoconPath_10_Deep);
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 10 deep path Get operation using a HoconPath path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_Using_Root_throughput(BenchmarkContext context)
        {
            TestConfig.Root.GetObject().GetValue(HoconPath_10_Deep).GetString();
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 10 deep path Get operation using a HoconPath path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_Using_Cache_throughput(BenchmarkContext context)
        {
            ConfigWithCache.GetString(HoconPath_10_Deep);
            _throughputCounter.Increment();
        }
        #endregion

        #region 4 deep spec
        private static readonly string StringPath_4_Deep = "a.b.c.val";
        private static readonly HoconPath HoconPathPath_4_Deep = new HoconPath(new string[] { "a", "b", "c", "val" });

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 4 deep path Get operation using a string path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_String_4_Deep_throughput(BenchmarkContext context)
        {
            TestConfig.GetString(StringPath_4_Deep);
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 4 deep path Get operation using a HoconPath path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_HoconPath_4_Deep_throughput(BenchmarkContext context)
        {
            TestConfig.GetString(HoconPathPath_4_Deep);
            _throughputCounter.Increment();
        }
        #endregion


    }
}
