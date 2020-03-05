using System;
using System.Collections.Generic;
using System.Text;
using NBench;

namespace Hocon.Tests.Performance
{
    public class ConfigFallbackSpecs
    {
        private const string CreateFallbackCounterName = "CreateFallbackOp";

        private Counter _createFallbackCounter;

        public static readonly Config C1 = @"
            akka{
                provider = cluster
                remote.dot-netty.tcp.port = 8110
            }
        ";

        public static readonly Config C2 = @"
            akka{
                loglevel = DEBUG
                remote.dot-netty.tcp.hostname = 0.0.0.0
            }
        ";

        public static readonly Config C3 = @"
            akka{
                remote.dot-netty.tcp.public-hostname = localhost
            }
        ";

        private static readonly Config L1 = C1.WithFallback(C2);

        private static readonly Config L2 = C1.WithFallback(C3);

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _createFallbackCounter = context.GetCounter(CreateFallbackCounterName);
        }

        [PerfBenchmark(
            Description = "Tests how quickly Config can add a single fallback",
            RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement(CreateFallbackCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Create_Config_Fallback_1_Deep(BenchmarkContext context)
        {
            var newConfig = C1.WithFallback(C2);
            _createFallbackCounter.Increment();
        }

        [PerfBenchmark(
            Description = "Tests how quickly Config can add a two fallbacks",
            RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement(CreateFallbackCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Create_Config_Fallback_2_Deep(BenchmarkContext context)
        {
            var newConfig = C1.WithFallback(C2).WithFallback(C3);
            _createFallbackCounter.Increment();
        }
    }

    public class ConfigFallbackLookupSpecs
    {
        private const string LookupFallbackCounterName = "LookupFallbackOp";

        private Counter _lookupFallbackOp;

        public static readonly Config C1 = @"
            akka{
                provider = cluster
                remote.dot-netty.tcp.port = 8110
            }
        ";

        public static readonly Config C2 = @"
            akka{
                loglevel = DEBUG
                remote.dot-netty.tcp.hostname = 0.0.0.0
            }
        ";

        public static readonly Config C3 = @"
            akka{
                remote.dot-netty.tcp.public-hostname = localhost
            }
        ";

        private static readonly Config L1 = C1.WithFallback(C2);

        private static readonly Config L2 = C1.WithFallback(C3);

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _lookupFallbackOp = context.GetCounter(LookupFallbackCounterName);
        }

        [PerfBenchmark(
            Description = "Tests how quickly Config can be looked up from the second fallback",
            RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement(LookupFallbackCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Lookup_Config_Fallback_2_Deep(BenchmarkContext context)
        {
            L1.GetString("akka.loglevel");
            _lookupFallbackOp.Increment();
        }

        [PerfBenchmark(
            Description = "Tests how quickly Config can be looked up from the third  fallback",
            RunMode = RunMode.Throughput, NumberOfIterations = 13, RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement(LookupFallbackCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Lookup_Config_Fallback_3_Deep(BenchmarkContext context)
        {
            L2.GetString("akka.remote.dot-netty.tcp.public-hostname");
            _lookupFallbackOp.Increment();
        }
    }
}
