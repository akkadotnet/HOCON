//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Hocon.Configuration
{
    /// <summary>
    /// This class represents the main configuration object used by a project
    /// when configuring objects within the system. To put it simply, it's
    /// the internal representation of a HOCON (Human-Optimized Config Object Notation)
    /// configuration string.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config()
        {
        }

        /// <summary>
        /// The configuration used as a secondary source.
        /// </summary>
        public Config Fallback { get; private set; }

        /// <summary>
        /// Determines if this root node contains any values
        /// </summary>
        public virtual bool IsEmpty => Root == null || Root.Type == HoconType.Empty;

        /// <summary>
        /// The root node of this configuration section
        /// </summary>
        public virtual HoconValue Root { get; private set; }

        /// <summary>
        /// An enumeration of substitutions values
        /// </summary>
        internal IEnumerable<HoconSubstitution> Substitutions { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="root">The root node to base this configuration.</param>
        /// <exception cref="ArgumentNullException">"The root value cannot be null."</exception>
        public Config(HoconRoot root)
        {
            if(root == null)
                throw new ArgumentNullException(nameof(root));

            Root = root.Value ?? throw new ArgumentNullException(nameof(root.Value));
            Substitutions = root.Substitutions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="source">The configuration to use as the primary source.</param>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <exception cref="ArgumentNullException">The source configuration cannot be null.</exception>
        public Config(Config source, Config fallback)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Root = source.Root;
            Fallback = fallback;
        }

        /// <summary>
        /// Generates a deep clone of the current configuration.
        /// </summary>
        /// <returns>A deep clone of the current configuration</returns>
        protected Config Copy()
        {
            //deep clone
            return new Config
            {
                Fallback = Fallback?.Copy(),
                Root = Root,
                Substitutions = Substitutions
            };
        }

        private HoconValue GetNode(HoconPath path)
        {
            HoconValue currentNode = Root;
            if (currentNode == null)
            {
                throw new InvalidOperationException("Current node should not be null");
            }
            foreach (string key in path)
            {
                currentNode = currentNode.GetChildObject(key);
                if (currentNode == null || currentNode.Type == HoconType.Empty)
                {
                    return Fallback != null ? Fallback.GetNode(path) : HoconValue.Undefined;
                }
            }
            return currentNode.Type == HoconType.Empty ? HoconValue.Undefined : currentNode;
        }

        /// <summary>
        /// Retrieves a new configuration from the current configuration
        /// with the root node being the supplied path.
        /// </summary>
        /// <param name="path">The path that contains the configuration to retrieve.</param>
        /// <returns>A new configuration with the root node being the supplied path.</returns>
        public Config GetConfig(string path)
            => GetConfig(HoconPath.Parse(path));

        public virtual Config GetConfig(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (Fallback != null)
            {
                Config f = Fallback.GetConfig(path);
                if (ReferenceEquals(value, HoconValue.Undefined) && f == null)
                    return null;
                if (ReferenceEquals(value, HoconValue.Undefined))
                    return f;

                return new Config(new HoconRoot(value)).WithFallback(f);
            }

            if (ReferenceEquals(value, HoconValue.Undefined))
                return null;

            return new Config(new HoconRoot(value));
        }

        #region Value getter methods

        /// <summary>
        /// Retrieves a string value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The string value defined in the specified path.</returns>
        public string GetString(string path, string @default = null)
            => GetString(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetString(string,string)"/>
        public virtual string GetString(HoconPath path, string @default = null)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetString();
        }

        /// <summary>
        /// Retrieves a boolean value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The boolean value defined in the specified path.</returns>
        public bool GetBoolean(string path, bool @default = false)
            => GetBoolean(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetBoolean(string,bool)"/>
        public virtual bool GetBoolean(HoconPath path, bool @default = false)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetBoolean();
        }

        /// <summary>
        /// Retrieves a long value, optionally suffixed with a 'b', from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The long value defined in the specified path.</returns>
        public long? GetByteSize(string path)
            => GetByteSize(HoconPath.Parse(path));

        /// <inheritdoc cref="GetByteSize(string)"/>
        public virtual long? GetByteSize(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return null;
            return value.GetByteSize();
        }

        /// <summary>
        /// Retrieves an integer value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The integer value defined in the specified path.</returns>
        public int GetInt(string path, int @default = 0)
            => GetInt(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetInt(string,int)"/>
        public virtual int GetInt(HoconPath path, int @default = 0)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetInt();
        }

        /// <summary>
        /// Retrieves a long value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The long value defined in the specified path.</returns>
        public long GetLong(string path, long @default = 0)
            => GetLong(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetLong(string,long)"/>
        public virtual long GetLong(HoconPath path, long @default = 0)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetLong();
        }

        /// <summary>
        /// Retrieves a float value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The float value defined in the specified path.</returns>
        public float GetFloat(string path, float @default = 0)
            => GetFloat(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetFloat(string,float)"/>
        public virtual float GetFloat(HoconPath path, float @default = 0)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetFloat();
        }

        /// <summary>
        /// Retrieves a decimal value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The decimal value defined in the specified path.</returns>
        public decimal GetDecimal(string path, decimal @default = 0)
            => GetDecimal(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetDecimal(string,decimal)"/>
        public virtual decimal GetDecimal(HoconPath path, decimal @default = 0)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetDecimal();
        }

        /// <summary>
        /// Retrieves a double value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The double value defined in the specified path.</returns>
        public double GetDouble(string path, double @default = 0)
            => GetDouble(HoconPath.Parse(path), @default);

        /// <inheritdoc cref="GetDouble(string,double)"/>
        public virtual double GetDouble(HoconPath path, double @default = 0)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                return @default;

            return value.GetDouble();
        }

        /// <summary>
        /// Retrieves a list of boolean values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of boolean values defined in the specified path.</returns>
        public IList<Boolean> GetBooleanList(string path)
            => GetBooleanList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetBooleanList(string)"/>
        public virtual IList<Boolean> GetBooleanList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetBooleanList();
        }

        /// <summary>
        /// Retrieves a list of decimal values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of decimal values defined in the specified path.</returns>
        public IList<decimal> GetDecimalList(string path)
            => GetDecimalList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetDecimalList(string)"/>
        public virtual IList<decimal> GetDecimalList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetDecimalList();
        }

        /// <summary>
        /// Retrieves a list of float values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of float values defined in the specified path.</returns>
        public IList<float> GetFloatList(string path)
            => GetFloatList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetFloatList(string)"/>
        public virtual IList<float> GetFloatList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetFloatList();
        }

        /// <summary>
        /// Retrieves a list of double values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of double values defined in the specified path.</returns>
        public IList<double> GetDoubleList(string path)
            => GetDoubleList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetDoubleList(string)"/>
        public virtual IList<double> GetDoubleList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetDoubleList();
        }

        /// <summary>
        /// Retrieves a list of int values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of int values defined in the specified path.</returns>
        public IList<int> GetIntList(string path)
            => GetIntList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetIntList(string)"/>
        public virtual IList<int> GetIntList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetIntList();
        }

        /// <summary>
        /// Retrieves a list of long values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of long values defined in the specified path.</returns>
        public IList<long> GetLongList(string path)
            => GetLongList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetLongList(string)"/>
        public virtual IList<long> GetLongList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetLongList();
        }

        /// <summary>
        /// Retrieves a list of byte values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of byte values defined in the specified path.</returns>
        public IList<byte> GetByteList(string path)
            => GetByteList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetByteList(string)"/>
        public virtual IList<byte> GetByteList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetByteList();
        }

        /// <summary>
        /// Retrieves a list of string values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of string values defined in the specified path.</returns>
        public IList<string> GetStringList(string path)
            => GetStringList(HoconPath.Parse(path));

        /// <inheritdoc cref="GetStringList(string)"/>
        public virtual IList<string> GetStringList(HoconPath path)
        {
            HoconValue value = GetNode(path);
            if (ReferenceEquals(value, HoconValue.Undefined))
                throw new HoconParserException($"Hocon path {path} was not an array.");

            return value.GetStringList();
        }

        /// <summary>
        /// Retrieves a <see cref="HoconValue"/> from a specific path.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The <see cref="HoconValue"/> found at the location if one exists, otherwise <c>null</c>.</returns>
        public HoconValue GetValue(string path)
            => GetValue(HoconPath.Parse(path));

        /// <inheritdoc cref="GetValue(string)"/>
        public HoconValue GetValue(HoconPath path)
        {
            HoconValue value = GetNode(path);
            return value;
        }

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(string path, TimeSpan? @default = null, bool allowInfinite = true)
            => GetMillisDuration(HoconPath.Parse(path), @default, allowInfinite);

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(HoconPath path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            return GetTimeSpan(path, @default, allowInfinite);
        }

        /// <summary>
        /// Retrieves a <see cref="TimeSpan"/> value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <param name="allowInfinite"><c>true</c> if infinite timespans are allowed; otherwise <c>false</c>.</param>
        /// <returns>The <see cref="TimeSpan"/> value defined in the specified path.</returns>
        public virtual TimeSpan GetTimeSpan(string path, TimeSpan? @default = null, bool allowInfinite = true)
            => GetTimeSpan(HoconPath.Parse(path), @default, allowInfinite);

        /// <inheritdoc cref="GetTimeSpan(string,System.Nullable{System.TimeSpan},bool)"/>
        public virtual TimeSpan GetTimeSpan(HoconPath path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            HoconValue value = GetNode(path);
            if (value == null)
                return @default.GetValueOrDefault();

            return value.GetTimeSpan(allowInfinite);
        }
        #endregion

        /// <summary>
        /// Normalize the values inside all Hocon fields to the simplest value possible under Hocon spec.
        /// NOTE: You might not be able to reproduce a clean reproduction of the original configuration file after this normalization.
        /// </summary>
        public void Normalize()
        {
            Flatten(Root);
        }

        private void Flatten(IHoconElement node)
        {
            if (!(node is HoconValue v))
                return;

            switch (v.Type)
            {
                case HoconType.Object:
                    var o = v.GetObject();
                    v.Values.Clear();
                    v.Values.Add(o);
                    foreach (var item in o.Values)
                        Flatten(item);
                    break;

                case HoconType.Array:
                    var a = v.GetArray();
                    v.Values.Clear();
                    var newArray = new HoconArray(v);
                    foreach (var item in a)
                    {
                        Flatten(item);
                        newArray.Add(item);
                    }
                    v.Values.Add(newArray);
                    break;

                case HoconType.Literal:
                    if (v.Values.Count == 1)
                        return;

                    var value = v.GetString();
                    v.Values.Clear();
                    if (value == null)
                        v.Values.Add(new HoconNull(v));
                    else if (value.NeedTripleQuotes())
                        v.Values.Add(new HoconTripleQuotedString(v, value));
                    else if (value.NeedQuotes())
                        v.Values.Add(new HoconQuotedString(v, value));
                    else
                        v.Values.Add(new HoconUnquotedString(v, value));
                    break;
            }
        }

        /// <summary>
        /// Configure the current configuration with a secondary source.
        /// </summary>
        /// <param name="fallback">The configuration to use as a secondary source.</param>
        /// <returns>The current configuration configured with the specified fallback.</returns>
        /// <exception cref="ArgumentException">Config can not have itself as fallback.</exception>
        public virtual Config WithFallback(Config fallback)
        {
            if (fallback == this)
                throw new ArgumentException("Config can not have itself as fallback", nameof(fallback));

            Config clone = Copy();

            Config current = clone;
            while (current.Fallback != null)
            {
                current = current.Fallback;
            }
            current.Fallback = fallback;

            return clone;
        }

        /// <summary>
        /// Determine if a HOCON configuration element exists at the specified location
        /// </summary>
        /// <param name="path">The location to check for a configuration value.</param>
        /// <returns><c>true</c> if a value was found, <c>false</c> otherwise.</returns>
        public bool HasPath(string path)
            => HasPath(HoconPath.Parse(path));

        /// <summary>
        /// Determine if a HOCON configuration element exists at the specified location
        /// </summary>
        /// <param name="path">The location to check for a configuration value.</param>
        /// <returns><c>true</c> if a value was found, <c>false</c> otherwise.</returns>
        public virtual bool HasPath(HoconPath path)
            => !ReferenceEquals(GetNode(path), HoconValue.Undefined);

        /// <summary>
        /// Adds the supplied configuration string as a fallback to the supplied configuration.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <param name="fallback">The string used as the fallback configuration.</param>
        /// <returns>The supplied configuration configured with the supplied fallback.</returns>
        public static Config operator +(Config config, string fallback)
            => config.WithFallback(ConfigurationFactory.ParseString(fallback));

        /// <summary>
        /// Adds the supplied configuration as a fallback to the supplied configuration string.
        /// </summary>
        /// <param name="configHocon">The configuration string used as the source.</param>
        /// <param name="fallbackConfig">The configuration used as the fallback.</param>
        /// <returns>A configuration configured with the supplied fallback.</returns>
        public static Config operator +(string configHocon, Config fallbackConfig)
            => ConfigurationFactory.ParseString(configHocon).WithFallback(fallbackConfig);

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Config"/>.
        /// </summary>
        /// <param name="str">The string that contains a configuration.</param>
        /// <returns>A configuration based on the supplied string.</returns>
        public static implicit operator Config(string str)
            => ConfigurationFactory.ParseString(str);

        /// <summary>
        /// Retrieves an enumerable key value pair representation of the current configuration.
        /// </summary>
        /// <returns>The current configuration represented as an enumerable key value pair.</returns>
        public virtual IEnumerable<KeyValuePair<string, HoconValue>> AsEnumerable()
        {
            var used = new HashSet<string>();
            var current = this;
            while (current != null)
            {
                foreach (var kvp in current.Root.GetObject())
                {
                    if (!used.Contains(kvp.Key))
                    {
                        yield return kvp;
                        used.Add(kvp.Key);
                    }
                }
                current = current.Fallback;
            }
        }

        /// <summary>
        /// Converts the current configuration to a string.
        /// </summary>
        /// <returns>A string containing the current configuration.</returns>
        public override string ToString()
            => Root?.ToString() ?? "";

        /// <summary>
        /// Converts the current configuration to a pretty printed string.
        /// </summary>
        /// <param name="indentSize">The number of spaces used for each indent.</param>
        /// <returns>A string containing the current configuration.</returns>
        public string PrettyPrint(int indentSize)
            => Root.ToString(0, indentSize);
    }

    /// <summary>
    /// This class contains convenience methods for working with <see cref="Config"/>.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        /// Retrieves the current configuration or the fallback
        /// configuration if the current one is null.
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
        /// Determines if the supplied configuration has any usable content period.
        /// </summary>
        /// <param name="config">The configuration used as the source.</param>
        /// <returns><c>true></c> if the <see cref="Config" /> is null or <see cref="Config.IsEmpty" />; otherwise <c>false</c>.</returns>
        public static bool IsNullOrEmpty(this Config config)
            => config == null || config.IsEmpty;
    }
}
