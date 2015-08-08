//-----------------------------------------------------------------------
// <copyright file="ConfigurationSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Configuration;
using System.Linq;
using NUnit;
using NUnit.Framework;
using Configuration.Hocon;
using Configuration;

namespace Akka.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationSpec
    {

        //[TestCase]
        //public void DeserializesHoconConfigurationFromNetConfigFile()
        //{
        //    var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
        //    Assert.NotNull(section);
        //    Assert.False(string.IsNullOrEmpty(section.Hocon.Content));
        //    var akkaConfig = section.AkkaConfig;
        //    Assert.NotNull(akkaConfig);
        //}

        [TestCase]
        public void CanCreateConfigFromSourceObject()
        {
            var source = new MyObjectConfig
            {
                StringProperty = "aaa",
                BoolProperty = true,
                IntergerArray = new[]{1,2,3,4 }
            };

            var config = ConfigurationFactory.FromObject(source);

            Assert.AreEqual("aaa", config.GetString("StringProperty"));
            Assert.AreEqual(true, config.GetBoolean("BoolProperty"));

            Assert.AreEqual(new[] { 1, 2, 3, 4 }, config.GetIntList("IntergerArray").ToArray());
        }

        [TestCase]
        public void CanMergeObjects()
        {
            var hocon1 = @"
a {
    b = 123
    c = 456
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

            var root1 = Parser.Parse(hocon1,null);
            var root2 = Parser.Parse(hocon2, null);

            var obj1 = root1.Value.GetObject();
            var obj2 = root2.Value.GetObject();
            obj1.Merge(obj2);

            var config = new Config(root1);

            Assert.AreEqual(123, config.GetInt("a.b"));
            Assert.AreEqual(456, config.GetInt("a.c"));
            Assert.AreEqual(789, config.GetInt("a.d"));
            Assert.AreEqual(888, config.GetInt("a.e"));
            Assert.AreEqual(888, config.GetInt("a.e"));
            Assert.AreEqual(123, config.GetInt("a.sub.aa"));
            Assert.AreEqual(456, config.GetInt("a.sub.bb"));

        }

        public class MyObjectConfig
        {
            public string StringProperty { get; set; }
            public bool BoolProperty { get; set; }
            public int[] IntergerArray { get; set; }
        }
   }
}

