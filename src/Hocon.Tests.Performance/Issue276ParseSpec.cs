using System;
using System.Collections.Generic;
using System.Text;
using NBench;

namespace Hocon.Tests.Performance
{
    public class Issue276ParseSpec
    {
        private const string Hocon = @"contexts {
  name: ""botcop""
  lifespan: 12
  parameters {
    fields {
      key: ""tz""
      value {
        string_value: ""Europe/Warsaw""
      }
    }
  }
}
contexts {
  name: ""__system_counters__""
  lifespan: 1
  parameters {
    fields {
      key: ""no-input""
      value {
        number_value: 0.0
      }
    }
    fields {
      key: ""no-match""
      value {
        number_value: 7.0
      }
    }
  }
}
contexts {
  name: ""botcopy-timezone""
  lifespan: 11
  parameters {
    fields {
      key: ""tz""
      value {
        string_value: ""Europe/Warsaw""
      }
    }
  }
}
contexts {
  name: ""xx""
  lifespan: 2
  parameters {
    fields {
      key: ""xxx""
      value {
        string_value: ""x""
      }
    }
  }
}";

        private const string ConfigParsingName = "ParsingOp";

        private Counter _parsingCounter;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _parsingCounter = context.GetCounter(ConfigParsingName);
        }

        [PerfBenchmark(
            Description = "Tests how much memory does config parsing consume",
            RunMode = RunMode.Throughput, NumberOfIterations = 20, RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigParsingName, MustBe.GreaterThan, 1000)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Config_Parsing(BenchmarkContext context)
        {
            var config = HoconConfigurationFactory.ParseString(Hocon);
            _parsingCounter.Increment();
        }

        [PerfBenchmark(
            Description = "Tests how much memory does hocon parsing consume",
            RunMode = RunMode.Throughput, NumberOfIterations = 20, RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterThroughputAssertion(ConfigParsingName, MustBe.GreaterThan, 1000)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Hocon_Parsing(BenchmarkContext context)
        {
            var config = HoconParser.Parse(Hocon);
            _parsingCounter.Increment();
        }
    }
}
