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
    public class Config : HoconRoot, ISerializable
    {
        private static readonly HoconValue EmptyValue;

        public const string SerializedPropertyName = "_dump";

        static Config()
        {
            EmptyValue = new HoconValue(null);
            EmptyValue.Add(new HoconObject(EmptyValue));
        }

        [Obsolete("For json serialization/deserialization only", true)]
        private Config()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        private Config(HoconValue value) : base(value)
        {
            //Root = GetRootValue();
        }

        /// <inheritdoc cref="Config(HoconValue)" />
        private Config(HoconValue value, Config fallback) : base(value)
        {
            Fallback = fallback;
            //Root = GetRootValue();
        }

        /// <inheritdoc cref="Config(HoconValue)" />
        /// <param name="source">The root node to base this configuration.</param>
        /// <exception cref="T:System.ArgumentNullException">"The root value cannot be null."</exception>
        public Config(HoconRoot source) : base(source?.Value)
        {
            //Root = GetRootValue();
        }

        /// <inheritdoc cref="Config(HoconValue)" />
        /// <param name="source">The configuration to use as the primary source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <exception cref="ArgumentNullException">The source configuration cannot be null.</exception>
        public Config(HoconRoot source, Config fallback) : base(source?.Value)
        {
            Fallback = fallback;
            //Root = GetRootValue();
        }

        /// <summary>
        ///     Identical to <see cref="ConfigurationFactory.Empty" />.
        /// </summary>
        /// <remarks>
        ///     Added for brevity and API backwards-compatibility with Akka.Hocon.
        /// </remarks>
        public static Config Empty => CreateEmpty();

        /// <summary>
        ///     The configuration used as a secondary source.
        /// </summary>
        // public Config Fallback { get; }

        /// <summary>
        ///     The root node of this configuration section
        /// </summary>
        // public virtual HoconValue Root { get; }

        /// <summary>
        ///     Determines if this root node contains any values
        /// </summary>
        public virtual bool IsEmpty => base.Value == EmptyValue && Fallback == null;

        public override HoconValue Value
        {
            get
            {
                var value = new HoconValue(null);
                var obj = Value.Clone(value).GetObject();
                var fallback = Fallback;
                while (fallback != null)
                {
                    obj.Merge(fallback.Value.GetObject());
                    fallback = fallback.Fallback;
                }

                return value;
            }

            protected set => throw new InvalidOperationException("Config value can not be set");
        }

        public HoconValue RootValue
        {
            get => base.Value;
            protected set => base.Value = value;
        }

        /// <summary>
        ///     Returns string representation of <see cref="Config" />, allowing to include fallback values
        /// </summary>
        /// <param name="useFallbackValues">If set to <c>true</c>, fallback values are included in the output</param>
        public string ToString(bool useFallbackValues)
        {
            if (!useFallbackValues)
                return base.ToString();

            return Value.ToString();
        }


        /// <summary>
        ///     Generates a deep clone of the current configuration.
        /// </summary>
        /// <returns>A deep clone of the current configuration</returns>
        private Config Copy(Config fallback = null)
        {
            //deep clone
            return fallback == null ? 
                new Config((HoconValue) Value.Clone(null), Fallback?.Copy()) : 
                new Config((HoconValue) Value.Clone(null), fallback.Copy());
        }

        protected override HoconValue GetNode(HoconPath path, bool throwIfNotFound = false)
        {
            var currentObject = Value.GetObject();
            var fallback = Fallback;

            while(fallback != null)
            {
                if (currentObject.TryGetValue(path, out var returnValue))
                    return returnValue;
                fallback = fallback.Fallback;
            }
            return null;
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
            return value == null ? Empty : new Config(new HoconRoot(value));
        }

        /// <summary>
        ///     Configure the current configuration with a secondary source.
        /// </summary>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <returns>The current configuration configured with the specified fallback.</returns>
        /// <exception cref="ArgumentException">Config can not have itself as fallback.</exception>
        public virtual Config WithFallback(Config fallback)
        {
            if (fallback.IsNullOrEmpty())
                return this; // no-op

            if (ReferenceEquals(fallback.Value, Value))
                throw new ArgumentException("Config can not have itself as fallback", nameof(fallback));

            if (fallback.Value == Value)
                return this;

            if (IsEmpty)
                return fallback;

            if (Fallback != null && Fallback.Value == fallback.Value)
                return this;

            // If Fallback is not set - we will set it in new copy
            // If Fallback was set - just use it, but with adding new fallback values
            return Copy(Fallback.SafeWithFallback(fallback));
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
            foreach (var kvp in Value.GetObject())
            {
                yield return kvp;
            }
        }

        private static Config CreateEmpty()
        {
            var value = new HoconValue(null);
            value.Add(new HoconObject(value));
            return new Config(value);
        }

        /// <inheritdoc />
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SerializedPropertyName, this.ToString(useFallbackValues: true), typeof(string));
        }

        [Obsolete("Used for serialization only", true)]
        public Config(SerializationInfo info, StreamingContext context)
        {
            var config = ConfigurationFactory.ParseString(info.GetValue(SerializedPropertyName, typeof(string)) as string);
            
            Value = config.Value;
            Fallback = config.Fallback;
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
            return config.IsNullOrEmpty()
                ? fallback
                : config == fallback
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