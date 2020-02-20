// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
    public class Config : HoconRoot, ISerializable, IEquatable<Config>
    {
        private static readonly HoconValue EmptyValue;

        private const string SerializedPropertyName = "_data";

        static Config()
        {
            EmptyValue = new HoconValue(null);
            EmptyValue.Add(new HoconObject(EmptyValue));
        }

        [Obsolete("For json serialization/deserialization only", true)]
        protected Config()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        protected Config(HoconValue value)
        {
            Value = (HoconValue)value.Clone(null);
        }

        /// <inheritdoc cref="Config(HoconValue)" />
        protected Config(HoconValue value, Config fallback) : this(value)
        {
            MergeConfig(fallback);
        }

        /// <inheritdoc cref="Config(HoconValue)" />
        /// <param name="root">The root node to base this configuration.</param>
        /// <exception cref="T:System.ArgumentNullException">"The root value cannot be null."</exception>
        public Config(HoconRoot root)
        {
            Value = (HoconValue)root.Value.Clone(null);
            if (!(root is Config cfg))
                return;

            foreach(var value in cfg._fallbacks)
            {
                _fallbacks.Add((HoconValue)value.Clone(null));
            }
        }

        /// <inheritdoc cref="Config(HoconValue)" />
        /// <param name="root">The configuration to use as the primary source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <exception cref="ArgumentNullException">The source configuration cannot be null.</exception>
        public Config(HoconRoot root, Config fallback) : this(root)
        {
            MergeConfig(fallback);
        }

        public bool UseCache { get; set; } = true;
        private ConcurrentDictionary<string, HoconValue> _cache = new ConcurrentDictionary<string, HoconValue>();

        /// <summary>
        ///     Identical to <see cref="HoconConfigurationFactory.Empty" />.
        /// </summary>
        /// <remarks>
        ///     Added for brevity and API backwards-compatibility with Akka.Hocon.
        /// </remarks>
        public static Config Empty => CreateEmpty();

        protected List<HoconValue> _fallbacks { get; } = new List<HoconValue>();
        public virtual IReadOnlyList<HoconValue> Fallbacks => _fallbacks.ToList().AsReadOnly();

        /// <summary>
        ///     Determines if this root node contains any values
        /// </summary>
        public virtual bool IsEmpty => Value == EmptyValue && _fallbacks.Count == 0;

        protected HoconValue _mergedValueCache = null;
        public HoconValue Root
        {
            get
            {
                if (_mergedValueCache == null)
                {
                    _mergedValueCache = (HoconValue)Value.Clone(null);
                    var obj = _mergedValueCache.GetObject();

                    foreach (var fallback in _fallbacks)
                    {
                        obj.FallbackMerge(fallback.GetObject());
                    }
                }

                return _mergedValueCache;
            }
        }

        /// <summary>
        ///     Returns string representation of <see cref="Config" />, allowing to include fallback values
        /// </summary>
        /// <param name="useFallbackValues">If set to <c>true</c>, fallback values are included in the output</param>
        public string ToString(bool useFallbackValues)
        {
            if (!useFallbackValues)
                return Value.ToString();

            return Root.ToString();
        }

        protected override bool TryGetNode(HoconPath path, out HoconValue result)
        {
            result = null;
            string fullPath = path.ToString();

            if (UseCache && _cache.TryGetValue(fullPath, out result))
                return true;

            var currentObject = Value.GetObject();
            if (currentObject == null)
                return false;
            if (currentObject.TryGetValue(path, out result))
            {
                if (UseCache)
                    _cache[fullPath] = result;
                return true;
            }

            foreach (var value in _fallbacks)
            {
                currentObject = value.GetObject();
                if (currentObject == null)
                    return false;
                if (currentObject.TryGetValue(path, out result))
                {
                    if (UseCache)
                        _cache[fullPath] = result;
                    return true;
                }
            }

            return false;
        }

        protected override HoconValue GetNode(HoconPath path)
        {
            HoconValue returnValue;
            string fullPath = path.ToString();

            if (UseCache && _cache.TryGetValue(fullPath, out returnValue))
                return returnValue;

            var currentObject = Value.GetObject();
            if (currentObject.TryGetValue(path, out returnValue))
            {
                if (UseCache)
                    _cache[fullPath] = returnValue;
                return returnValue;
            }

            foreach (var value in _fallbacks)
            {
                currentObject = value.GetObject();
                if (currentObject.TryGetValue(path, out returnValue))
                {
                    if (UseCache)
                        _cache[fullPath] = returnValue;
                    return returnValue;
                }
            }

            throw new HoconException($"Could not find accessible field at path `{path}` in all fallbacks.");
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
            if(Root.GetObject().TryGetValue(path, out var result))
                return new Config(result);
            return Empty;
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
                throw new ArgumentException("Config can not have itself as fallback", nameof(fallback));

            if (fallback.IsNullOrEmpty())
                return this; // no-op

            if (IsEmpty)
                return fallback;

            var result = new Config(Value);
            foreach(var value in _fallbacks)
            {
                result._fallbacks.Add((HoconValue)value.Clone(null));
            }
            result.MergeConfig(fallback);

            _mergedValueCache = null;
            return result;
        }

        private void MergeConfig(Config other)
        {
            InsertFallbackValue(other.Value);
            foreach (var fallbackValue in other.Fallbacks)
            {
                InsertFallbackValue(fallbackValue);
            }
        }

        private void InsertFallbackValue(HoconValue value)
        {
            HoconValue duplicateValue = null;
            foreach(var fallbackValue in _fallbacks)
            {
                if(fallbackValue == value)
                {
                    duplicateValue = fallbackValue;
                    break;
                }
            }
            if (duplicateValue != null)
            {
                _fallbacks.Remove(duplicateValue);
            }
            _fallbacks.Add((HoconValue)value.Clone(null));
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
        public override IEnumerable<KeyValuePair<string, HoconField>> AsEnumerable()
        {
            foreach (var kvp in Root.GetObject())
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
            info.AddValue(SerializedPropertyName, ToString(useFallbackValues: true), typeof(string));
        }

        public virtual bool Equals(Config other)
        {
            if (IsEmpty && other.IsEmpty) return true;
            if (Value == other.Value) return true;
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

        [Obsolete("Used for serialization only", true)]
        public Config(SerializationInfo info, StreamingContext context):base(null)
        {
            var config = HoconConfigurationFactory.ParseString(info.GetValue(SerializedPropertyName, typeof(string)) as string);
            
            Value = config.Value;
            _fallbacks.AddRange(config._fallbacks);
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