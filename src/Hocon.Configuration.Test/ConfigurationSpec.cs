// -----------------------------------------------------------------------
// <copyright file="ConfigurationSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Hocon.Configuration.Tests
{
    public class ConfigurationSpec
    {
        /// <summary>
        ///     Is <c>true</c> if we're running on a Mono VM. <c>false</c> otherwise.
        /// </summary>
        public static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

        public class MyObjectConfig
        {
            public string StringProperty { get; set; }
            public bool BoolProperty { get; set; }
            public int[] IntergerArray { get; set; }
        }

        [Fact]
        public void CanEnumerateQuotedKeys()
        {
            var hocon = @"
a {
   ""some quoted, key"": 123
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            var config2 = config.GetConfig("a");
            var enumerable = config2.AsEnumerable();

            Assert.Equal("some quoted, key", enumerable.Select(kvp => kvp.Key).First());
        }

        /// <summary>
        ///     Should follow the load order rules specified in https://github.com/akkadotnet/HOCON/issues/151
        /// </summary>
        [Fact]
        public void CanLoadDefaultConfig()
        {
            var defaultConf = ConfigurationFactory.Default();
            defaultConf.Should().NotBe(ConfigurationFactory.Empty);
            defaultConf.HasPath("root.simple-string").Should().BeTrue();
            defaultConf.GetString("root.simple-string").Should().Be("Hello HOCON2");
        }

        [Fact]
        public void CanMergeObjects()
        {
            var hocon1 = @"
a {
    b = 123
    c = ---
    d = 789
    sub {
        aa = 123
    }
}
";

            var hocon2 = @"
a {
    c = 999
    e = 888
    sub {
        bb = 456
    }
}
";

            var root1 = Parser.Parse(hocon1);
            var root2 = Parser.Parse(hocon2);

            var obj1 = root1.Value.GetObject();
            var obj2 = root2.Value.GetObject();
            obj1.Merge(obj2);

            var config = new Config(root1);

            Assert.Equal(123, config.GetInt("a.b"));
            Assert.Equal(999, config.GetInt("a.c"));
            Assert.Equal(789, config.GetInt("a.d"));
            Assert.Equal(888, config.GetInt("a.e"));
            Assert.Equal(888, config.GetInt("a.e"));
            Assert.Equal(123, config.GetInt("a.sub.aa"));
            Assert.Equal(456, config.GetInt("a.sub.bb"));
        }

        [Fact]
        public void CanParseQuotedKeys()
        {
            var hocon = @"
a {
   ""some quoted, key"": 123
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.Equal(123, config.GetInt("a.\"some quoted, key\""));
        }

        [Fact]
        public void CanParseSerializersAndBindings()
        {
            var hocon = @"
akka.actor {
    serializers {
      akka-containers = ""Akka.Remote.Serialization.MessageContainerSerializer, Akka.Remote""
      proto = ""Akka.Remote.Serialization.ProtobufSerializer, Akka.Remote""
      daemon-create = ""Akka.Remote.Serialization.DaemonMsgCreateSerializer, Akka.Remote""
    }

    serialization-bindings {
# Since com.google.protobuf.Message does not extend Serializable but
# GeneratedMessage does, need to use the more specific one here in order
# to avoid ambiguity
      ""Akka.Actor.ActorSelectionMessage"" = akka-containers
      ""Akka.Remote.DaemonMsgCreate, Akka.Remote"" = daemon-create
    }

}";

            var config = ConfigurationFactory.ParseString(hocon);

            var serializersConfig = config.GetConfig("akka.actor.serializers").AsEnumerable().ToList();
            var serializerBindingConfig = config.GetConfig("akka.actor.serialization-bindings").AsEnumerable().ToList();

            Assert.Equal(
                "Akka.Remote.Serialization.MessageContainerSerializer, Akka.Remote",
                serializersConfig.Select(kvp => kvp.Value)
                    .First()
                    .GetString()
            );
            Assert.Equal(
                "Akka.Remote.DaemonMsgCreate, Akka.Remote",
                serializerBindingConfig.Select(kvp => kvp.Key).Last()
            );
        }

        [Fact]
        public void CanParseSubConfig()
        {
            var hocon = @"
a {
   b {
     c = 1
     d = true
   }
}";
            var config = ConfigurationFactory.ParseString(hocon);
            var subConfig = config.GetConfig("a");
            Assert.Equal(1, subConfig.GetInt("b.c"));
            Assert.True(subConfig.GetBoolean("b.d"));
        }

        [Fact]
        public void CanSubstituteArrayCorrectly()
        {
            var hocon = @"
 c: {
    q: {
        a: [2, 5]
    }
}
c: {
    m: ${c.q} {a: [6]}
}
";
            var config = ConfigurationFactory.ParseString(hocon);
            var unchanged = config.GetIntList("c.q.a");
            unchanged.Should().Equal(2, 5);
            var changed = config.GetIntList("c.m.a");
            changed.Should().Equal(6);
        }


        [Fact]
        public void CanUseFallback()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);

            var config = config1.WithFallback(config2);

            Assert.Equal(123, config.GetInt("foo.bar.a"));
            Assert.Equal(2, config.GetInt("foo.bar.b"));
            Assert.Equal(3, config.GetInt("foo.bar.c"));
        }

        [Fact]
        public void CanUseFallbackInSubConfig()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);

            var config = config1.WithFallback(config2).GetConfig("foo.bar");

            Assert.Equal(123, config.GetInt("a"));
            Assert.Equal(2, config.GetInt("b"));
            Assert.Equal(3, config.GetInt("c"));
        }

        [Fact]
        public void CanUseFallbackString()
        {
            var hocon1 = @"
foo {
   bar {
      a=123str
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1str
      b=2str
      c=3str
   }
   car = ""bar""
}
dar = d";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);

            var config = config1.WithFallback(config2);

            Assert.Equal("123str", config.GetString("foo.bar.a"));
            Assert.Equal("2str", config.GetString("foo.bar.b"));
            Assert.Equal("3str", config.GetString("foo.bar.c"));
            Assert.Equal("bar", config.GetString("foo.car"));
            Assert.Equal("d", config.GetString("dar"));
        }

        [Fact]
        public void CanUseFallbackWithEmpty()
        {
            var config1 = Config.Empty;
            var hocon2 = @"
foo {
   bar {
      a=1str
      b=2str
      c=3str
   }
   car = ""bar""
}
dar = d";
            var config2 = ConfigurationFactory.ParseString(hocon2);

            var config = config1.SafeWithFallback(config2);

            Assert.Equal("1str", config.GetString("foo.bar.a"));
            Assert.Equal("2str", config.GetString("foo.bar.b"));
            Assert.Equal("3str", config.GetString("foo.bar.c"));
            Assert.Equal("bar", config.GetString("foo.car"));
            Assert.Equal("d", config.GetString("dar"));
        }

        [Fact]
        public void CanUseFluentMultiLevelFallback()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";
            var hocon3 = @"
foo {
   bar {
      a=99
      zork=555
   }
}";
            var hocon4 = @"
foo {
   bar {
      borkbork=-1
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);
            var config3 = ConfigurationFactory.ParseString(hocon3);
            var config4 = ConfigurationFactory.ParseString(hocon4);

            var config = config1.WithFallback(config2).WithFallback(config3).WithFallback(config4);

            Assert.Equal(123, config.GetInt("foo.bar.a"));
            Assert.Equal(2, config.GetInt("foo.bar.b"));
            Assert.Equal(3, config.GetInt("foo.bar.c"));
            Assert.Equal(555, config.GetInt("foo.bar.zork"));
            Assert.Equal(-1, config.GetInt("foo.bar.borkbork"));
        }

        [Fact]
        public void CanUseMultiLevelFallback()
        {
            var hocon1 = @"
foo {
   bar {
      a=123
   }
}";
            var hocon2 = @"
foo {
   bar {
      a=1
      b=2
      c=3
   }
}";
            var hocon3 = @"
foo {
   bar {
      a=99
      zork=555
   }
}";
            var hocon4 = @"
foo {
   bar {
      borkbork=-1
   }
}";

            var config1 = ConfigurationFactory.ParseString(hocon1);
            var config2 = ConfigurationFactory.ParseString(hocon2);
            var config3 = ConfigurationFactory.ParseString(hocon3);
            var config4 = ConfigurationFactory.ParseString(hocon4);

            var config = config1.WithFallback(config2.WithFallback(config3.WithFallback(config4)));

            Assert.Equal(123, config.GetInt("foo.bar.a"));
            Assert.Equal(2, config.GetInt("foo.bar.b"));
            Assert.Equal(3, config.GetInt("foo.bar.c"));
            Assert.Equal(555, config.GetInt("foo.bar.zork"));
            Assert.Equal(-1, config.GetInt("foo.bar.borkbork"));
        }

        [Fact]
        public void Config_Empty_is_Empty()
        {
            ConfigurationFactory.Empty.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void HoconValue_GetObject_should_use_fallback_values()
        {
            var config1 = ConfigurationFactory.ParseString("a = 5");
            var config2 = ConfigurationFactory.ParseString("b = 3");
            var config = config1.WithFallback(config2);
            var rootObject = config.Root.GetObject();
            rootObject.ContainsKey("a").Should().BeTrue();
            rootObject["a"].Raw.Should().Be("5");
            rootObject.ContainsKey("b").Should().BeTrue();
            rootObject["b"].Raw.Should().Be("3");
        }

        [Fact]
        public void Quoted_key_should_be_parsed()
        {
            var config1 = ConfigurationFactory.ParseString(
                "akka : { version : \"0.0.1 Akka\", home : \"\", loggers : [\"Akka.TestKit.TestEventListener, Akka.TestKit\"], loggers-dispatcher : \"akka.actor.default-dispatcher\", logger-startup-timeout : 5s, loglevel : WARNING, suppress-json-serializer-warning : true, stdout-loglevel : WARNING, log-config-on-start : off, log-dead-letters : 10, log-dead-letters-during-shutdown : on, extensions : [], daemonic : off, actor : { provider : \"Akka.Actor.LocalActorRefProvider\", guardian-supervisor-strategy : \"Akka.Actor.DefaultSupervisorStrategy\", creation-timeout : 20s, reaper-interval : 5, serialize-messages : off, serialize-creators : off, unstarted-push-timeout : 10s, ask-timeout : infinite, typed : { timeout : 5 }, inbox : { inbox-size : 1000, default-timeout : 5s }, router : { type-mapping : { from-code : \"Akka.Routing.NoRouter\", round-robin-pool : \"Akka.Routing.RoundRobinPool\", round-robin-group : \"Akka.Routing.RoundRobinGroup\", random-pool : \"Akka.Routing.RandomPool\", random-group : \"Akka.Routing.RandomGroup\", smallest-mailbox-pool : \"Akka.Routing.SmallestMailboxPool\", broadcast-pool : \"Akka.Routing.BroadcastPool\", broadcast-group : \"Akka.Routing.BroadcastGroup\", scatter-gather-pool : \"Akka.Routing.ScatterGatherFirstCompletedPool\", scatter-gather-group : \"Akka.Routing.ScatterGatherFirstCompletedGroup\", consistent-hashing-pool : \"Akka.Routing.ConsistentHashingPool\", consistent-hashing-group : \"Akka.Routing.ConsistentHashingGroup\", tail-chopping-pool : \"Akka.Routing.TailChoppingPool\", tail-chopping-group : \"Akka.Routing.TailChoppingGroup\", my-router : \"Akka.Tests.Routing.ConfiguredLocalRoutingSpec+MyRouter, Akka.Tests\" } }, deployment : { default : { dispatcher : \"\", mailbox : \"\", router : \"from-code\", nr-of-instances : 1, within : 5 s, virtual-nodes-factor : 10, routees : { paths : [] }, resizer : { enabled : off, lower-bound : 1, upper-bound : 10, pressure-threshold : 1, rampup-rate : 0.2, backoff-threshold : 0.3, backoff-rate : 0.1, messages-per-resize : 10 } },   /config : { router : random-pool, nr-of-instances : 4, pool-dispatcher : { type : ForkJoinDispatcher, throughput : 100, dedicated-thread-pool : { thread-count : 4, deadlock-timeout : 3s, threadtype : background } } },   /paths : { router : random-group, routees : { paths : [\" /user /service1\",\" /user /service2\"] } },  /weird : { router : round-robin-pool, nr-of-instances : 3 } }, synchronized-dispatcher : { type : \"SynchronizedDispatcher\", executor : \"current-context-executor\", throughput : 10 }, task-dispatcher : { type : \"TaskDispatcher\", executor : \"task-executor\", throughput : 30 }, default-fork-join-dispatcher : { type : ForkJoinDispatcher, executor : fork-join-executor, throughput : 30, dedicated-thread-pool : { thread-count : 3, threadtype : background } }, default-dispatcher : { type : ForkJoinDispatcher, executor : \"default-executor\", default-executor : { }, thread-pool-executor : { }, fork-join-executor : { dedicated-thread-pool : { thread-count : 3, threadtype : background } }, current-context-executor : { }, shutdown-timeout : 1s, throughput : 100, throughput-deadline-time : 0ms, attempt-teamwork : on, mailbox-requirement : \"\", dedicated-thread-pool : { thread-count : 4, deadlock-timeout : 3s, threadtype : background } }, default-mailbox : { mailbox-type : \"Akka.Dispatch.UnboundedMailbox\", mailbox-capacity : 1000, mailbox-push-timeout-time : 10s, stash-capacity : -1 }, mailbox : { requirements : { Akka.Dispatch.IUnboundedMessageQueueSemantics : akka.actor.mailbox.unbounded-queue-based, Akka.Dispatch.IBoundedMessageQueueSemantics : akka.actor.mailbox.bounded-queue-based, Akka.Dispatch.IDequeBasedMessageQueueSemantics : akka.actor.mailbox.unbounded-deque-based, Akka.Dispatch.IUnboundedDequeBasedMessageQueueSemantics : akka.actor.mailbox.unbounded-deque-based, Akka.Dispatch.IBoundedDequeBasedMessageQueueSemantics : akka.actor.mailbox.bounded-deque-based, Akka.Dispatch.IMultipleConsumerSemantics : akka.actor.mailbox.unbounded-queue-based, Akka.Event.ILoggerMessageQueueSemantics : akka.actor.mailbox.logger-queue }, unbounded-queue-based : { mailbox-type : \"Akka.Dispatch.UnboundedMailbox\" }, bounded-queue-based : { mailbox-type : \"Akka.Dispatch.BoundedMailbox\" }, unbounded-deque-based : { mailbox-type : \"Akka.Dispatch.UnboundedDequeBasedMailbox\" }, bounded-deque-based : { mailbox-type : \"Akka.Dispatch.BoundedDequeBasedMailbox\" }, logger-queue : { mailbox-type : \"Akka.Event.LoggerMailboxType\" } }, debug : { receive : off, autoreceive : off, lifecycle : off, fsm : off, event-stream : off, unhandled : off, router-misconfiguration : off }, serializers : { json : \"Akka.Serialization.NewtonSoftJsonSerializer, Akka\", bytes : \"Akka.Serialization.ByteArraySerializer, Akka\" }, serialization-bindings : { \"System.Byte[]\" : bytes, \"System.Object\" : json }, serialization-identifiers : { \"Akka.Serialization.ByteArraySerializer, Akka\" : 4, \"Akka.Serialization.NewtonSoftJsonSerializer, Akka\" : 1 }, serialization-settings : { } }, scheduler : { tick-duration : 10ms, ticks-per-wheel : 512, implementation : \"Akka.Actor.HashedWheelTimerScheduler\", shutdown-timeout : 5s }, io : { pinned-dispatcher : { type : \"PinnedDispatcher\", executor : \"fork-join-executor\" }, tcp : { direct-buffer-pool : { class : \"Akka.IO.Buffers.DirectBufferPool, Akka\", buffer-size : 512, buffers-per-segment : 500, initial-segments : 1, buffer-pool-limit : 1024 }, disabled-buffer-pool : { class : \"Akka.IO.Buffers.DisabledBufferPool, Akka\", buffer-size : 512 }, buffer-pool : \"akka.io.tcp.disabled-buffer-pool\", max-channels : 256000, selector-association-retries : 10, batch-accept-limit : 10, register-timeout : 5s, max-received-message-size : unlimited, trace-logging : off, selector-dispatcher : \"akka.io.pinned-dispatcher\", worker-dispatcher : \"akka.actor.default-dispatcher\", management-dispatcher : \"akka.actor.default-dispatcher\", file-io-dispatcher : \"akka.actor.default-dispatcher\", file-io-transferTo-limit : 524288, finish-connect-retries : 5, windows-connection-abort-workaround-enabled : off, outgoing-socket-force-ipv4 : false }, udp : { direct-buffer-pool : { class : \"Akka.IO.Buffers.DirectBufferPool, Akka\", buffer-size : 512, buffers-per-segment : 500, initial-segments : 1, buffer-pool-limit : 1024 }, buffer-pool : \"akka.io.udp.direct-buffer-pool\", nr-of-socket-async-event-args : 32, max-channels : 4096, select-timeout : infinite, selector-association-retries : 10, receive-throughput : 3, direct-buffer-size : 18432, direct-buffer-pool-limit : 1000, received-message-size-limit : unlimited, trace-logging : off, selector-dispatcher : \"akka.io.pinned-dispatcher\", worker-dispatcher : \"akka.actor.default-dispatcher\", management-dispatcher : \"akka.actor.default-dispatcher\" }, udp-connected : { direct-buffer-pool : { class : \"Akka.IO.Buffers.DirectBufferPool, Akka\", buffer-size : 512, buffers-per-segment : 500, initial-segments : 1, buffer-pool-limit : 1024 }, buffer-pool : \"akka.io.udp-connected.direct-buffer-pool\", nr-of-socket-async-event-args : 32, max-channels : 4096, select-timeout : infinite, selector-association-retries : 10, receive-throughput : 3, direct-buffer-size : 18432, direct-buffer-pool-limit : 1000, received-message-size-limit : unlimited, trace-logging : off, selector-dispatcher : \"akka.io.pinned-dispatcher\", worker-dispatcher : \"akka.actor.default-dispatcher\", management-dispatcher : \"akka.actor.default-dispatcher\" }, dns : { dispatcher : \"akka.actor.default-dispatcher\", resolver : \"inet-address\", inet-address : { provider-object : \"Akka.IO.InetAddressDnsProvider\", positive-ttl : 30s, negative-ttl : 10s, cache-cleanup-interval : 120s, use-ipv6 : true } } }, coordinated-shutdown : { default-phase-timeout : 5 s, terminate-actor-system : on, exit-clr : off, run-by-clr-shutdown-hook : on, phases : { before-service-unbind : { }, service-unbind : { depends-on : [before-service-unbind] }, service-requests-done : { depends-on : [service-unbind] }, service-stop : { depends-on : [service-requests-done] }, before-cluster-shutdown : { depends-on : [service-stop] }, cluster-sharding-shutdown-region : { timeout : 10 s, depends-on : [before-cluster-shutdown] }, cluster-leave : { depends-on : [cluster-sharding-shutdown-region] }, cluster-exiting : { timeout : 10 s, depends-on : [cluster-leave] }, cluster-exiting-done : { depends-on : [cluster-exiting] }, cluster-shutdown : { depends-on : [cluster-exiting-done] }, before-actor-system-terminate : { depends-on : [cluster-shutdown] }, actor-system-terminate : { timeout : 10 s, depends-on : [before-actor-system-terminate] } } }, test : { timefactor : 1.0, filter-leeway : 3s, single-expect-default : 3s, default-timeout : 5s, calling-thread-dispatcher : { type : \"Akka.TestKit.CallingThreadDispatcherConfigurator, Akka.TestKit\", throughput : 2147483647 }, test-actor : { dispatcher : { type : \"Akka.TestKit.CallingThreadDispatcherConfigurator, Akka.TestKit\", throughput : 2147483647 } } }, serialize-messages : on, remote : { dot-netty : { tcp : { port : 0 } } }}"
             );
            var config2 = ConfigurationFactory.ParseString(
                "akka : { loggers : [\"Akka.TestKit.TestEventListener, Akka.TestKit\"], suppress-json-serializer-warning : true, test : { timefactor : 1.0, filter-leeway : 3s, single-expect-default : 3s, default-timeout : 5s, calling-thread-dispatcher : { type : \"Akka.TestKit.CallingThreadDispatcherConfigurator, Akka.TestKit\", throughput : 2147483647 }, test-actor : { dispatcher : { type : \"Akka.TestKit.CallingThreadDispatcherConfigurator, Akka.TestKit\", throughput : 2147483647 } } }}"
            );
            var config3 = ConfigurationFactory.ParseString(
                "akka : { loglevel : WARNING, stdout-loglevel : WARNING, serialize-messages : on, actor : { }, remote : { dot-netty : { tcp : { port : 0 } } }}"
            );
            var config4 = ConfigurationFactory.ParseString(
                "akka : { loglevel : WARNING, stdout-loglevel : WARNING, serialize-messages : on, actor : { }, remote : { dot-netty : { tcp : { port : 0 } } }}"
            );
            var config5 = ConfigurationFactory.ParseString(@"
                akka {
                  actor {
                    default-dispatcher {
                        type = ForkJoinDispatcher
                        throughput = 100
                        dedicated-thread-pool {
                            thread-count = 4
                            deadlock-timeout = 3s
                            threadtype = background
                        }
                    }
                    router.type-mapping {
                        my-router = ""Akka.Tests.Routing.ConfiguredLocalRoutingSpec+MyRouter, Akka.Tests""
                    }
                    deployment {
                      /config {
                        router = random-pool
                        nr-of-instances = 4
                        pool-dispatcher = ${akka.actor.default-dispatcher}
                      }
                      /paths {
                        router = random-group
                        routees.paths = [""/user/service1"", ""/user/service2""]
                      }
                      /weird {
                        router = round-robin-pool
                        nr-of-instances = 3
                      }
                      ""/weird/*"" {
                        router = round-robin-pool
                        nr-of-instances = 2
                      }
                      /myrouter {
                        router = ""my-router""
                        foo = bar
                      }
                      /sys-parent/round {
                        router = round-robin-pool
                        nr-of-instances = 6
                      }
                    }
                  }
                }
            ");
            
            // Is throwing at "/weird/*" key parsing
            config5.WithFallback(config4).WithFallback(config3).WithFallback(config2).WithFallback(config1).Root.Invoking(r => r.GetObject()).Should().NotThrow();
        }
        
        [Fact]
        public void HoconValue_GetObject_should_use_fallback_values_with_complex_objects()
        {
            var config1 = ConfigurationFactory.ParseString(@"
	            akka.actor.deployment {
		            /worker1 {
			            router = round-robin-group1
			            routees.paths = [""/user/testroutes/1""]
		            }
	            }");
            var config2 = ConfigurationFactory.ParseString(@"
	            akka.actor.deployment {
		            /worker2 {
			            router = round-robin-group2
			            routees.paths = [""/user/testroutes/2""]
		            }
	            }");
            var configWithFallback = config1.WithFallback(config2);
            
            var config = configWithFallback.GetConfig("akka.actor.deployment");
            var rootObj = config.Root.GetObject();
            rootObj.Unwrapped.Should().ContainKeys("/worker1", "/worker2");
            rootObj["/worker1.router"].Raw.Should().Be("round-robin-group1");
            rootObj["/worker1.router"].Raw.Should().Be("round-robin-group1");
            rootObj["/worker1.routees.paths"].Value[0].GetArray()[0].Raw.Should().Be(@"""/user/testroutes/1""");
            rootObj["/worker2.routees.paths"].Value[0].GetArray()[0].Raw.Should().Be(@"""/user/testroutes/2""");
        }


#if !NETCORE
        [Fact]
        public void DeserializesHoconConfigurationFromNetConfigFile()
        {
            /*
             * BUG: as of 3-14-2019, this code throws a bunch of scary
             * serialization exceptions on Mono.
             *
             */
            if (IsMono) return;

            var raw = ConfigurationManager.GetSection("akka");
            var section = (HoconConfigurationSection) raw;
            Assert.NotNull(section);
            Assert.False(string.IsNullOrEmpty(section.Hocon.Content));
            var config = section.Config;
            Assert.NotNull(config);
        }
#endif

        [Fact]
        public void ShouldSerializeFallbackValues()
        {
            var a = ConfigurationFactory.ParseString(@" akka : {
                some-key : value
            }");
            var b = ConfigurationFactory.ParseString(@"akka : {
                other-key : 42
            }");

            var c = a.WithFallback(b);
            
            c.GetInt("akka.other-key").Should().Be(42, "Fallback value should exist as data");
            c.ToString().Should().NotContain("other-key", "Fallback values are ignored by default");
            c.ToString(true).Should().Contain("other-key", "Fallback values should be displayed when requested");
            
            c.GetString("akka.some-key").Should().Be("value", "Original value should remain");
            c.ToString().Should().Contain("some-key", "Original values are shown by default");
            c.ToString(true).Should().Contain("some-key", "Original values should be displayed always");

        }

        /// <summary>
        /// Source issue: https://github.com/akkadotnet/HOCON/issues/175
        /// </summary>
        [Fact]
        public void ShouldDeserializeFromJson()
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter>(),
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new AkkaContractResolver()
            };
            
            var json = "{\"$id\":\"1\",\"$type\":\"Hocon.Config, Hocon.Configuration\",\"Root\":{\"$type\":\"Hocon.HoconEmptyValue, Hocon\",\"$values\":[]},\"Value\":{\"$type\":\"Hocon.HoconEmptyValue, Hocon\",\"$values\":[]},\"Substitutions\":{\"$type\":\"Hocon.HoconSubstitution[], Hocon\",\"$values\":[]},\"IsEmpty\":true}";
            var restored = JsonConvert.DeserializeObject<Config>(json, settings);
            restored.IsEmpty.Should().BeTrue();
        }
        
        internal class AkkaContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (!prop.Writable)
                {
                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        var hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }

                return prop;
            }
        }
    }
}