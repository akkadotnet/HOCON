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
using Hocon.Debugger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public void Config_should_be_serializable()
        {
            var config = ConfigurationFactory.ParseString(@"
                foo{
                  bar.biz = 12
                  baz = ""quoted""
                }");
            var serialized = JsonConvert.SerializeObject(config);
            var deserialized = JsonConvert.DeserializeObject<Config>(serialized);
            config.DumpConfig().Should().Be(deserialized.DumpConfig());
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
        public void Fallback_should_not_be_modified()
        {
            var config1 = ConfigurationFactory.ParseString(@"
	            a {
                    b {
                        c = 5
                        e = 7
                    }
	            }");
            var config2 = ConfigurationFactory.ParseString(@"
	            a {
                    b {
                        d = 3
                        c = 1
                    }
	            }");
            
            var merged = config1.WithFallback(config2).Root.GetObject(); // Perform values loading
            
            config1.GetInt("a.b.c").Should().Be(5);
            config1.GetInt("a.b.e").Should().Be(7);
            config2.GetInt("a.b.c").Should().Be(1);
            config2.GetInt("a.b.d").Should().Be(3);
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
                "akka.actor.deployment.default = { }"
             );
            var config2 = ConfigurationFactory.ParseString(@"
                akka.actor.deployment {
                  ""/weird/*"" {
                    router = round-robin-pool
                    nr-of-instances = 2
                  }
                }
            ");

            var megred = config2.WithFallback(config1).Root;
            // Is throwing at "/weird/*" key parsing
            megred.Invoking(r => r.GetObject()).Should().NotThrow();
        }
        
        [Fact]
        public void Quoted_key_with_dot_should_be_parsed()
        {
            var config1 = ConfigurationFactory.ParseString(
                @"akka.actor.serialization-bindings = {
                    ""System.Byte[]"" : bytes,
                    ""System.Object"" : json
                }"
            );
            var config2 = ConfigurationFactory.ParseString(
                @"akka.actor.serialization-bindings = {
                    ""System.Byte[]"" : bytes,
                    ""System.Object"" : json
                }"
            );

            var megred = config2.WithFallback(config1).Root;
            // Is throwing at "System.Byte[]" key parsing
            megred.Invoking(r => r.GetObject()).Should().NotThrow();
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
        [Fact(Skip = "This is disabled due to temprorary fix for https://github.com/akkadotnet/HOCON/issues/206")]
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