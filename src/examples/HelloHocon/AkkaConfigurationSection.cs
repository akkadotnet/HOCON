using Akka.Configuration;
using System.Configuration;
using Akka.Configuration.Hocon;

namespace ExternalIncludes
{
	public class AkkaConfigurationSection : ConfigurationSection
	{
		private const string ConfigurationPropertyName = "hocon";
		private Config _akkaConfig;

		/// <summary>
		/// Retrieves a <see cref="Config"/> from the contents of the
		/// custom akka node within a configuration file.
		/// </summary>
		public Config AkkaConfig
		{
			get { return _akkaConfig ?? (_akkaConfig = ConfigurationFactory.ParseString(Hocon.Content)); }
		}

		/// <summary>
		/// Retrieves the HOCON (Human-Optimized Config Object Notation)
		/// configuration string from the custom akka node.
		/// <code>
		/// <?xml version="1.0" encoding="utf-8" ?>
		/// <configuration>
		///   <configSections>
		///     <section name="akka" type="Akka.Configuration.Hocon.AkkaConfigurationSection, Akka" />
		///   </configSections>
		///   <akka>
		///      <hocon>
		///      ...
		///      </hocon>
		///   </akka>
		/// </configuration>
		/// </code>
		/// </summary>
		[ConfigurationProperty(ConfigurationPropertyName, IsRequired = true)]
		public HoconConfigurationElement Hocon
		{
			get { return (HoconConfigurationElement)base[ConfigurationPropertyName]; }
			set { base[ConfigurationPropertyName] = value; }
		}
	}
}
