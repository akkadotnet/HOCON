//-----------------------------------------------------------------------
// <copyright file="TestConfigurationSection.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration.Hocon;
using System.Configuration;

namespace ConfigurationFile
{
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
