﻿//-----------------------------------------------------------------------
// <copyright file="ConfigurationElementSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Akka.Configuration.Hocon;
using NUnit.Framework;
using Akka.Configuration;

namespace Akka.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationElementSpec
    {
        [TestCase]
        public void CanHaveAHoconListElement()
        {
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = @"ConfigurationElementTestData1.config"
            };

            var testConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var section = testConfiguration.GetSection("testsection") as TestConfigurationSection;
            Assert.IsNotNull(section);

            var elementCollection = section.HoconList;
            Assert.IsNotNull(elementCollection);
            Assert.AreEqual(elementCollection.Count, 3);
        }

        [TestCase]
        public void HoconListElementsChainAsFallbacks()
        {
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = @"ConfigurationElementTestData1.config"
            };

            var testConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var section = testConfiguration.GetSection("testsection") as TestConfigurationSection;

            var config = ConfigurationFactory.FromConfigurationElementCollection(section.HoconList);

            string resulta = config.GetString("root.simple-string-a");
            Assert.IsNotNull(resulta);
            Assert.AreEqual(resulta, "A");

            string resultb = config.GetString("root.simple-string-b");
            Assert.IsNotNull(resultb);
            Assert.AreEqual(resultb, "B");

            string resultc = config.GetString("root.simple-string-c");
            Assert.IsNotNull(resultc);
            Assert.AreEqual(resultc, "C");
        }

        public class TestConfigurationSection : ConfigurationSection
        {
            private const string ConfigurationPropertyName = "hoconlist";

            [ConfigurationProperty(ConfigurationPropertyName, IsRequired = false)]
            public HoconListConfigurationElementCollection HoconList
            {
                get { return (HoconListConfigurationElementCollection)base[ConfigurationPropertyName]; }
                set { base[ConfigurationPropertyName] = value; }
            }
        }
    }
}
