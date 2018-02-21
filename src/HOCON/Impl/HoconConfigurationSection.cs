//-----------------------------------------------------------------------
// <copyright file="HoconConfigurationSection.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

#if CONFIGURATION
using System.Configuration;

namespace Hocon
{
   /// <summary>
   /// This class represents a custom HOCON node within a configuration file.
   /// <code>
   /// <?xml version="1.0" encoding="utf-8" ?>
   /// <configuration>
   ///   <configSections>
   ///     <section name="hocon" type="Hocon.HoconConfigurationSection, Hocon" />
   ///   </configSections>
   ///   <hocon>
   ///   ...
   ///   </hocon>
   /// </configuration>
   /// </code>
   /// </summary>
   public class HoconConfigurationSection : ConfigurationSection
   {
      private const string ConfigurationPropertyName = "hocon";
      private Config _config;

      /// <summary>
      /// Retrieves a <see cref="Config"/> from the contents of the
      /// custom akka node within a configuration file.
      /// </summary>
      public Config Config
      {
         get { return _config ?? (_config = ConfigurationFactory.ParseString(Hocon.Content)); }
      }

      /// <summary>
      /// Retrieves the HOCON (Human-Optimized Config Object Notation)
      /// configuration string from the custom HOCON node.
      /// <code>
      /// <?xml version="1.0" encoding="utf-8" ?>
      /// <configuration>
      ///   <configSections>
      ///     <section name="hocon" type="Hocon.HoconConfigurationSection, Hocon" />
      ///   </configSections>
      ///   <hocon>
      ///      <hocon>
      ///      ...
      ///      </hocon>
      ///   </hocon>
      /// </configuration>
      /// </code>
      /// </summary>
      [ConfigurationProperty(ConfigurationPropertyName, IsRequired = true)]
      public HoconConfigurationElement Hocon
      {
          get
          {
              return (HoconConfigurationElement)base[ConfigurationPropertyName];
          }
          set
          {
              base[ConfigurationPropertyName] = value;
          }
      }
   }
}
#endif