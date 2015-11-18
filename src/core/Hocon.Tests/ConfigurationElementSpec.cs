//-----------------------------------------------------------------------
// <copyright file="ConfigurationSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Akka.Configuration.Hocon;

namespace Akka.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationElementSpec
    {
        [TestCase]
        public void HoconListElementIsOptional()
        {
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = @"ConfigurationElementTest1.config"
            };

            var testConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var section = testConfiguration.GetSection("akka") as AkkaConfigurationSection;
            Assert.IsNotNull(section);
            Assert.AreEqual(section.HoconList.Count, 0);

            var config = section.AkkaConfig;
            Assert.IsNotNull(config);
            string result = config.GetString("root.simple-string");
            Assert.AreEqual(result, "A");
        }

        [TestCase]
        public void CanHaveAHoconListElement()
        {
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = @"ConfigurationElementTest2.config"
            };

            var testConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var section = testConfiguration.GetSection("akka") as AkkaConfigurationSection;
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
                ExeConfigFilename = @"ConfigurationElementTest2.config"
            };

            var testConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var section = testConfiguration.GetSection("akka") as AkkaConfigurationSection;

            var config = section.AkkaConfig;

            string result1 = config.GetString("root.simple-string-1");
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1, "1");

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
    }
}
