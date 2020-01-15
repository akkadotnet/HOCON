// -----------------------------------------------------------------------
// <copyright file="HoconConfigurationElement.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Configuration;

namespace Hocon
{
    /// <summary>
    ///     This class represents a custom HOCON (Human-Optimized Config Object Notation)
    ///     node within a configuration file.
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
    public class HoconConfigurationElement : CDataConfigurationElement
    {
        /// <summary>
        ///     Gets or sets the HOCON configuration string contained in the hocon node.
        /// </summary>
        [ConfigurationProperty(ContentPropertyName, IsRequired = true, IsKey = true)]
        public string Content
        {
            get => (string) base[ContentPropertyName];
            set => base[ContentPropertyName] = value;
        }
    }
}