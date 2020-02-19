using Hocon;
using NBench;

namespace Hocon.Tests.Performance
{
    public class ConfigLookupSpec
    {
        private const string ConfigLookupName = "LookupOp";
        private const double MinimumAcceptableOperationsPerSecond = 80000.0d; 

        private Counter _throughputCounter;

        private static readonly Config Config_10_Deep = "a.b.c.d.e.f.g.h.i.j = 10";
        private static readonly string StringPath_10_Deep = "a.b.c.d.e.f.g.h.i.j";
        private static readonly HoconPath HoconPathPath_10_Deep = new HoconPath(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" });

        private static readonly Config Config_4_Deep = "a.b.c.d = 10";
        private static readonly string StringPath_4_Deep = "a.b.c.d";
        private static readonly HoconPath HoconPathPath_4_Deep = new HoconPath(new string[] { "a", "b", "c", "d"});

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _throughputCounter = context.GetCounter(ConfigLookupName);
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 10 deep path lookup using a string path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_String_10_Deep_throughput(BenchmarkContext context)
        {
            Config_10_Deep.GetString(StringPath_10_Deep);
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 10 deep path lookup using a HoconPath path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_HoconPath_10_Deep_throughput(BenchmarkContext context)
        {
            Config_10_Deep.GetString(HoconPathPath_10_Deep);
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 4 deep path lookup using a string path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_String_4_Deep_throughput(BenchmarkContext context)
        {
            Config_4_Deep.GetString(StringPath_4_Deep);
            _throughputCounter.Increment();
        }

        [PerfBenchmark(
             Description = "Tests how quickly Config can perform a 4 deep path lookup using a HoconPath path",
             RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
             TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigLookupName, MustBe.GreaterThan, MinimumAcceptableOperationsPerSecond)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void Config_Lookup_HoconPath_4_Deep_throughput(BenchmarkContext context)
        {
            Config_4_Deep.GetString(HoconPathPath_4_Deep);
            _throughputCounter.Increment();
        }
    }
}
