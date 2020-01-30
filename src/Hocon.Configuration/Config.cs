// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Hocon
{
    /// <summary>
    ///     This class represents the main configuration object used by a project
    ///     when configuring objects within the system. To put it simply, it's
    ///     the internal representation of a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    /// </summary>
    [Serializable]
    public class Config : HoconObject
    {
        /// <summary>
        /// INTERNAL API
        ///
        /// Special case for empty configurations. Immutable and can't be added as a fallback.
        /// </summary>
        internal sealed class EmptyConfig : Config
        {
            public static EmptyConfig Instance = new EmptyConfig();

            private EmptyConfig()
            { }
        }

        public const string SerializedPropertyName = "_dump";
        
        private Config()
        { }

        /// <inheritdoc />
        /// <param name="source">The root node to base this configuration.</param>
        /// <exception cref="T:System.ArgumentNullException">"The root value cannot be null."</exception>
        public Config(IReadOnlyDictionary<string, HoconElement> source) : base(source)
        { }

        /// <inheritdoc cref="Config(IReadOnlyDictionary, Config)" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        /// <param name="source">The configuration to use as the primary source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <exception cref="ArgumentNullException">The source configuration cannot be null.</exception>
        private Config(IReadOnlyDictionary<string, HoconElement> source, Config fallback):base(source)
        {
            Fallback = fallback;
        }

        private Config(IDictionary<string, HoconElement> fields):base(fields)
        { }

        /// <inheritdoc cref="Config(HoconObject, Config)" />
        /// <param name="source">The configuration to use as the primary source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <exception cref="ArgumentNullException">The source configuration cannot be null.</exception>
        public Config(IDictionary<string, HoconElement> source, Config fallback) : base(source)
        {
            Fallback = fallback;
        }

        private Config(HoconElement source) : base(source)
        { }

        internal static Config Create(IReadOnlyDictionary<string, HoconElement> fields, Config fallback)
        {
            return new Config(fields, fallback);
        }

        internal static Config Create(IReadOnlyDictionary<string, HoconElement> fields)
        {
            return new Config(fields);
        }

        internal static Config Create(IDictionary<string, HoconElement> fields, Config fallback)
        {
            return new Config(fields, fallback);
        }

        internal static Config Create(IDictionary<string, HoconElement> fields)
        {
            return new Config(fields);
        }

        /// <summary>
        ///     Identical to <see cref="ConfigurationFactory.Empty" />.
        /// </summary>
        /// <remarks>
        ///     Added for brevity and API backwards-compatibility with Akka.Hocon.
        /// </remarks>
        public static Config Empty => ConfigurationFactory.Empty;

        /// <summary>
        ///     The configuration used as a secondary source.
        /// </summary>
        public Config Fallback { get; }

        /// <summary>
        ///     Determines if this config contains any values
        /// </summary>
        public bool IsEmpty => this == Config.Empty;

        /*
        /// <summary>
        ///     Returns string representation of <see cref="Config" />, allowing to include fallback values
        /// </summary>
        /// <param name="useFallbackValues">If set to <c>true</c>, fallback values are included in the output</param>
        public string ToString(bool useFallbackValues)
        {
            if (!useFallbackValues)
                return base.ToString();
            
            return Root.ToString();
        }

        /// <summary>
        ///     Generates a deep clone of the current configuration.
        /// </summary>
        /// <returns>A deep clone of the current configuration</returns>
        protected virtual Config Copy(Config fallback = null)
        {
            //deep clone
            return new Config((InternalHoconValue) Value.Clone(null), fallback?.Copy() ?? Fallback?.Copy());
        }

        protected override InternalHoconValue GetNode(HoconPath path, bool throwIfNotFound = false)
        {
            try
            {
                return Root.GetObject().GetValue(path);
            }
            catch
            {
                if (throwIfNotFound)
                    throw;

                return null;
            }
        }
        */

        /// <summary>
        ///     Retrieves a new configuration from the current configuration
        ///     with the root node being the supplied path.
        /// </summary>
        /// <param name="path">The path that contains the configuration to retrieve.</param>
        /// <returns>A new configuration with the root node being the supplied path.</returns>
        public Config GetConfig(string path)
        {
            return GetConfig(HoconPath.Parse(path));
        }

        public Config GetConfig(HoconPath path)
        {
            var value = this[path];
            return value == null ? null : new Config(value);
        }

        /// <summary>
        ///     Adds the supplied configuration string as a fallback to the supplied configuration.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <param name="fallback">The string used as the fallback configuration.</param>
        /// <returns>The supplied configuration configured with the supplied fallback.</returns>
        public static Config operator +(Config config, string fallback)
        {
            return config.WithFallback(ConfigurationFactory.ParseString(fallback));
        }

        /// <summary>
        ///     Adds the supplied configuration as a fallback to the supplied configuration string.
        /// </summary>
        /// <param name="configHocon">The configuration string used as the source.</param>
        /// <param name="fallbackConfig">The configuration used as the fallback.</param>
        /// <returns>A configuration configured with the supplied fallback.</returns>
        public static Config operator +(string configHocon, Config fallbackConfig)
        {
            return ConfigurationFactory.ParseString(configHocon).WithFallback(fallbackConfig);
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="Config" />.
        /// </summary>
        /// <param name="str">The string that contains a configuration.</param>
        /// <returns>A configuration based on the supplied string.</returns>
        public static implicit operator Config(string str)
        {
            return ConfigurationFactory.ParseString(str);
        }

        /*
        /// <summary>
        /// Performs aggregation of Value and all Fallbacks into single <see cref="HoconElement"/> object
        /// </summary>
        private HoconElement GetRootValue()
        {
            var elements = new List<IInternalHoconElement>();

            for (var config = this; config != null; config = config.Fallback?.Copy())
            {
                elements.AddRange(config.Value);
            }

            var aggregated = new InternalHoconValue(null);
            aggregated.AddRange(elements.AsEnumerable().Reverse());

            return aggregated;
        }

        /// <inheritdoc />
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SerializedPropertyName, this.ToString(useFallbackValues: true), typeof(string));
        }
        */
    }

    /// <summary>
    ///     This class contains convenience methods for working with <see cref="Config" />.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        ///     Configure the current configuration with a secondary source.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <returns>The current configuration configured with the specified fallback.</returns>
        /// <exception cref="ArgumentException">Config can not have itself as fallback.</exception>
        public static Config WithFallback(this Config config, Config fallback)
        {
            if (fallback == config)
                throw new ArgumentException("Config can not have itself as fallback", nameof(fallback));

            if (fallback == Config.Empty)
                return config; // no-op

            // If Fallback is not set - we will set it in new copy
            // If Fallback was set - just use it, but with adding new fallback values
            return Config.Create(config, config.Fallback != null ? config.Fallback.SafeWithFallback(fallback) : fallback);
        }

        /// <summary>
        ///     Retrieves the current configuration or the fallback
        ///     configuration if the current one is null.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <returns>The current configuration or the fallback configuration if the current one is null.</returns>
        public static Config SafeWithFallback(this Config config, Config fallback)
        {
            return config == null
                ? fallback
                : ReferenceEquals(config, fallback)
                    ? config
                    : config.WithFallback(fallback);
        }

        /// <summary>
        ///     Determines if the supplied configuration has any usable content period.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <returns><c>true></c> if the <see cref="Config" /> is null or <see cref="Config.IsEmpty" />; otherwise <c>false</c>.</returns>
        public static bool IsNullOrEmpty(this Config config)
        {
            return config == null || config.IsEmpty;
        }
    }
}