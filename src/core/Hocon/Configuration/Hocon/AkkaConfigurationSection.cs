//-----------------------------------------------------------------------
// <copyright file="AkkaConfigurationSection.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;

namespace Akka.Configuration.Hocon
{
    /// <summary>
    /// This class represents a custom akka node within a configuration file.
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="akka" type="Akka.Configuration.Hocon.AkkaConfigurationSection, Akka" />
    ///   </configSections>
    ///   <akka>
    ///   ...
    ///   </akka>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </summary>
    public class AkkaConfigurationSection : ConfigurationSection
    {
        internal const string ConfigurationPropertyName = "hocon";
        internal const string ConfigurationListPropertyName = "hoconlist";
        private Config _akkaConfig;

        /// <summary>
        /// Retrieves a <see cref="Config"/> from the contents of the
        /// custom akka node within a configuration file.
        /// </summary>
        public Config AkkaConfig
        {
            get { return _akkaConfig ?? (_akkaConfig = ConfigurationFactory.FromConfigurationSection(this)); }
        }

        /// <summary>
        /// Retrieves the HOCON (Human-Optimized Config Object Notation)
        /// configuration string from the custom akka node.
        /// <code>
        /// <?xml version="1.0" encoding="utf-8" ?>
        /// <configuration>
        ///   <configSections>
        ///     <section name="akka" type="AkkaConfiguration.Hocon.AkkaConfigurationSection, Akka" />
        ///   </configSections>
        ///   <akka>
        ///      <hocon>
        ///      ...
        ///      </hocon>
        ///   </akka>
        /// </configuration>
        /// </code>
        /// </summary>
        [ConfigurationProperty(ConfigurationPropertyName, IsRequired = false)]
        public HoconConfigurationElement Hocon
        {
            get { return (HoconConfigurationElement) base[ConfigurationPropertyName]; }
            set { base[ConfigurationPropertyName] = value; }
        }

        /// <summary>
        /// Retrieves the list of HOCON sections.
        /// <code>
        /// <?xml version="1.0" encoding="utf-8" ?>
        /// <configuration>
        ///   <configSections>
        ///     <section name="akka" type="AkkaConfiguration.Hocon.AkkaConfigurationSection, Akka" />
        ///   </configSections>
        ///   <akka>
        ///      <hoconlist>
        ///         <hocon>
        ///         ...
        ///         </hocon>
        ///         <hocon>
        ///         ...
        ///         </hocon>
        ///      </hoconlist>
        ///   </akka>
        /// </configuration>
        /// </code>
        /// </summary>
        [ConfigurationProperty(ConfigurationListPropertyName, IsRequired = false)]
        public HoconListConfigurationElementCollection HoconList
        {
            get { return (HoconListConfigurationElementCollection)base[ConfigurationListPropertyName]; }
            set { base[ConfigurationListPropertyName] = value; }
        }
    }
}

