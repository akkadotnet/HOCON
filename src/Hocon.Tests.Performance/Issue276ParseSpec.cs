using System;
using System.Collections.Generic;
using System.Text;
using NBench;

namespace Hocon.Tests.Performance
{
    public class Issue276ParseSpec
    {
        private const string Hocon = @"id: ""17952a2b-5962-46dd-9183-605cfabe7661-266f04e0""
lang: ""en""
session_id: ""170ec254-0e34-4c18-85a0-83cd7f61c435""
timestamp: ""2020-04-07T10:15:55.921Z""
result {
  source: ""agent""
  resolved_query: ""kkk""
  action: ""input.unknown""
  score: 1.0
  parameters {
  }
  contexts {
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
  metadata {
    intent_id: ""56390419-decd-45e2-b194-5f54f50393fa""
    intent_name: ""Default Fallback Intent""
    webhook_used: ""false""
    webhook_for_slot_filling_used: ""false""
    is_fallback_intent: ""true""
  }
  fulfillment {
    speech: ""Sorry, I didn't get that. Can you rephrase?""
    messages {
      lang: ""en""
      type {
        number_value: 0.0
      }
      speech {
        string_value: ""Sorry, I didn't get that. Can you rephrase?""
      }
    }
  }
}
status {
  code: 200
  error_type: ""success""
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
