// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    public class Config : HoconObject, ISerializable, IEquatable<Config>
    {
        /// <summary>
        ///     Identical to <see cref="HoconConfigurationFactory.Empty" />.
        /// </summary>
        /// <remarks>
        ///     Added for brevity and API backwards-compatibility with Akka.Hocon.
        /// </remarks>
        public new static Config Empty { get; } = new Config();

        public static Config Create(HoconElement root)
        {
            return new Config(root);
        }

        private Config()
        {
            Root = this;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        public Config(HoconElement root):base(root)
        {
            if(root is Config cfg)
            {
                Root = cfg.Root;
                _fallbacks = cfg._fallbacks.ToList();
            }  
            else 
            {
                Root = this;
            }
        }

        protected List<HoconObject> _fallbacks { get; } = new List<HoconObject>();
        public virtual IReadOnlyList<HoconObject> Fallbacks => _fallbacks.ToList().AsReadOnly();

        /// <summary>
        ///     Determines if this root node contains any values
        /// </summary>
        public virtual bool IsEmpty => Count == 0 && _fallbacks.Count == 0;

        public HoconObject Root { get; private set; }

        /// <summary>
        ///     Returns string representation of <see cref="Config" />, allowing to include fallback values
        /// </summary>
        /// <param name="useFallbackValues">If set to <c>true</c>, fallback values are included in the output</param>
        public string ToString(bool useFallbackValues)
        {
            if (!useFallbackValues)
                return ToString();

            return Root.ToString();
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
            if (Root.TryGetValue(path, out var result))
                return new Config(result);
            return Empty;
        }

        public override HoconElement GetValue(HoconPath path)
        {
            if (base.TryGetValue(path, out var result))
                return result;

            foreach (var fallback in _fallbacks)
                if (fallback.TryGetValue(path, out result))
                    return result;

            throw new HoconException($"Could not find path '{path}'.");
        }

        public override bool TryGetValue(HoconPath path, out HoconElement result)
        {
            if (base.TryGetValue(path, out result))
                return true;

            foreach (var fallback in _fallbacks)
                if (fallback.TryGetValue(path, out result))
                    return true;

            return false;
        }

        /// <summary>
        ///     Configure the current configuration with a secondary source.
        ///     If the inserted configuration is already present in the fallback chain, 
        ///     it will be moved to the end of the chain instead.
        /// </summary>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <returns>The current configuration configured with the specified fallback.</returns>
        /// <exception cref="ArgumentException">Config can not have itself as fallback.</exception>
        public virtual Config WithFallback(Config fallback)
        {
            if (ReferenceEquals(fallback, this))
                return this; // no-op

            if (fallback.IsNullOrEmpty())
                return this; // no-op

            if (IsEmpty)
                return fallback;

            var result = new Config(this);
            result.MergeConfig(fallback);

            return result;
        }

        private void MergeConfig(Config other)
        {
            InsertFallbackValue(other);
            foreach (var fallbackValue in other.Fallbacks)
            {
                InsertFallbackValue(fallbackValue);
            }
        }

        private void InsertFallbackValue(HoconObject value)
        {
            foreach(var fallbackValue in _fallbacks)
            {
                if(fallbackValue == value)
                {
                    return;
                }
            }
            _fallbacks.Add(value);
            Root = value.Merge(Root);
        }

        /// <summary>
        ///     Adds the supplied configuration string as a fallback to the supplied configuration.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <param name="fallback">The string used as the fallback configuration.</param>
        /// <returns>The supplied configuration configured with the supplied fallback.</returns>
        public static Config operator +(Config config, string fallback)
        {
            return config.WithFallback(HoconConfigurationFactory.ParseString(fallback));
        }

        /// <summary>
        ///     Adds the supplied configuration as a fallback to the supplied configuration string.
        /// </summary>
        /// <param name="configHocon">The configuration string used as the source.</param>
        /// <param name="fallbackConfig">The configuration used as the fallback.</param>
        /// <returns>A configuration configured with the supplied fallback.</returns>
        public static Config operator +(string configHocon, Config fallbackConfig)
        {
            return HoconConfigurationFactory.ParseString(configHocon).WithFallback(fallbackConfig);
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="Config" />.
        /// </summary>
        /// <param name="str">The string that contains a configuration.</param>
        /// <returns>A configuration based on the supplied string.</returns>
        public static implicit operator Config(string str)
        {
            return HoconConfigurationFactory.ParseString(str);
        }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<string, HoconElement>> AsEnumerable()
        {
            foreach (var kvp in Root)
            {
                yield return kvp;
            }
        }

        public virtual bool Equals(Config other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (IsEmpty && other.IsEmpty) return true;
            if (Root == other.Root) return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Config cfg)
                return Equals(cfg);
            return false;
        }

        public string Serialize()
        {
            return new HoconObjectBuilder()
                .Add("$root", this)
                .Add("$fallbacks", new HoconArrayBuilder(_fallbacks).Build())
                .Build().Raw;
        }

        private void InternalDeserialize(string raw)
        {
            var config = HoconConfigurationFactory.ParseString(raw);
            Fields = config["\"$root\""].ToObject().ToImmutableSortedDictionary();
            foreach (var element in config["\"$fallbacks\""].GetArray())
            {
                InsertFallbackValue(element.ToObject());
            }
        }

        public static Config Deserialize(string raw)
        {
            var result = new Config();
            result.InternalDeserialize(raw);
            return result;
        }

        [Obsolete("Used for serialization only", true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
           info.AddValue("$config", Serialize(), typeof(string));
        }

        [Obsolete("Used for serialization only", true)]
        protected Config(SerializationInfo info, StreamingContext context)
        {
            InternalDeserialize((string)info.GetValue("$config", typeof(string)));
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