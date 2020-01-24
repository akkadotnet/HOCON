// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    /// <summary>
    ///     This class represents the main configuration object used by a project
    ///     when configuring objects within the system. To put it simply, it's
    ///     the internal representation of a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    /// </summary>
    public class Config : HoconRoot
    {
        [Obsolete("For json serialization/deserialization only", true)]
        private Config()
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        private Config(HoconValue value, Config fallback)
        {
            Fallback = fallback;
            Value = value;
            
            Root = GetRootValue();
        }

        /// <inheritdoc cref="Config(HoconValue, Config)" />
        /// <param name="root">The root node to base this configuration.</param>
        /// <exception cref="T:System.ArgumentNullException">"The root value cannot be null."</exception>
        public Config(HoconRoot root) : base(root?.Value, root?.Substitutions ?? Enumerable.Empty<HoconSubstitution>())
        {
            Root = GetRootValue();
        }

        /// <inheritdoc cref="Config(HoconValue, Config)" />
        /// <param name="source">The configuration to use as the primary source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <exception cref="ArgumentNullException">The source configuration cannot be null.</exception>
        public Config(HoconRoot source, Config fallback) : base(source?.Value,
            source?.Substitutions ?? Enumerable.Empty<HoconSubstitution>())
        {
            Fallback = fallback;
            
            Root = GetRootValue();
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
        ///     The root node of this configuration section
        /// </summary>
        public virtual HoconValue Root { get; }

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
        public Config Copy()
        {
            //deep clone
            return new Config((HoconValue) Value.Clone(null), Fallback?.Copy());
        }

        protected override HoconValue GetNode(HoconPath path, bool throwIfNotFound = false)
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

        /// <summary>
        ///     Retrieves a new configuration from the current configuration
        ///     with the root node being the supplied path.
        /// </summary>
        /// <param name="path">The path that contains the configuration to retrieve.</param>
        /// <returns>A new configuration with the root node being the supplied path.</returns>
        public virtual Config GetConfig(string path)
        {
            return GetConfig(HoconPath.Parse(path));
        }

        public virtual Config GetConfig(HoconPath path)
        {
            var value = GetNode(path);
            return value == null ? null : new Config(new HoconRoot(value));
        }

        /// <summary>
        ///     Configure the current configuration with a secondary source.
        /// </summary>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <returns>The current configuration configured with the specified fallback.</returns>
        /// <exception cref="ArgumentException">Config can not have itself as fallback.</exception>
        public virtual Config WithFallback(Config fallback)
        {
            if (fallback == this)
                throw new ArgumentException("Config can not have itself as fallback", nameof(fallback));
            
            // If Fallback is not set - we will set it in new copy
            // If Fallback was set - just use it, but with adding new fallback values
            return new Config((HoconValue) Value.Clone(null), Fallback.SafeWithFallback(fallback));
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

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<string, HoconField>> AsEnumerable()
        {
            var used = new HashSet<string>();
            foreach (var kvp in Root.GetObject())
            {
                if (used.Contains(kvp.Key))
                    continue;

                yield return kvp;
                used.Add(kvp.Key);
            }
        }
        
        /// <summary>
        /// Performs aggregation of Value and all Fallbacks into single <see cref="HoconValue"/> object
        /// </summary>
        private HoconValue GetRootValue()
        {
            var elements = new List<IHoconElement>();

            for (var config = this; config != null; config = config.Fallback?.Copy())
            {
                elements.AddRange(config.Value);
            }

            var aggregated = new HoconValue(null);
            aggregated.AddRange(elements.AsEnumerable().Reverse());

            return aggregated;
        }
    }

    /// <summary>
    ///     This class contains convenience methods for working with <see cref="Config" />.
    /// </summary>
    public static class ConfigExtensions
    {
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
        /// <returns><c>true></c> if the <see cref="Config" /> is null or <see cref="HoconRoot.IsEmpty" />; otherwise <c>false</c>.</returns>
        public static bool IsNullOrEmpty(this Config config)
        {
            return config == null || config.IsEmpty;
        }
    }
}