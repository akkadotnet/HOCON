// -----------------------------------------------------------------------
// <copyright file="HoconConfigurationSection.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Configuration;

namespace Hocon
{
    /// <summary>
    ///     This class represents a custom HOCON node within a configuration file.
    ///     <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="hocon" type="Hocon.HoconConfigurationSection, Hocon.Configuration" />
    ///   </configSections>
    ///   <hocon>
    ///   ...
    ///   </hocon>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </summary>
    public class HoconConfigurationSection : ConfigurationSection
    {
        private const string ConfigurationPropertyName = "hocon";
        private Config _config;

        /// <summary>
        ///     Retrieves a <see cref="Config" /> from the contents of the
        ///     custom akka node within a configuration file.
        /// </summary>
        public Config Config {
            get
            {
                if (_config == null && !string.IsNullOrWhiteSpace(Hocon.Content))
                    _config = HoconConfigurationFactory.ParseString(Hocon.Content);
                return _config;
            }
        }

        /// <summary>
        ///     Retrieves the HOCON (Human-Optimized Config Object Notation)
        ///     configuration string from the custom HOCON node.
        ///     <code>
        /// <![CDATA[
        /// <?xml version="1.0" encoding="utf-8" ?>
        /// <configuration>
        ///   <configSections>
        ///     <section name="hocon" type="Hocon.HoconConfigurationSection, Hocon.Configuration" />
        ///   </configSections>
        ///   <hocon>
        ///      <hocon>
        ///      ...
        ///      </hocon>
        ///   </hocon>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </summary>
        [ConfigurationProperty(ConfigurationPropertyName, IsRequired = true)]
        public HoconConfigurationElement Hocon
        {
            get => (HoconConfigurationElement) base[ConfigurationPropertyName];
            set => base[ConfigurationPropertyName] = value;
        }
    }
}