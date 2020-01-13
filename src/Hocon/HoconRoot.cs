//-----------------------------------------------------------------------
// <copyright file="HoconRoot.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    /// <summary>
    /// This class represents the root element in a HOCON (Human-Optimized Config Object Notation)
    /// configuration string.
    /// </summary>
    public class HoconRoot
    {
        /// <summary>
        /// Retrieves the value associated with this element.
        /// </summary>
        public HoconValue Value { get; protected set; }

        /// <summary>
        /// Retrieves an enumeration of substitutions associated with this element.
        /// </summary>
        public IEnumerable<HoconSubstitution> Substitutions { get; }

        /// <summary>
        /// Determines if this root node contains any values
        /// </summary>
        public virtual bool IsEmpty => Value == null || Value.Type == HoconType.Empty;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Hocon.HoconRoot" /> class.
        /// </summary>
        public HoconRoot() : this(new HoconValue(null), Enumerable.Empty<HoconSubstitution>())
        { }

        /// <inheritdoc cref="HoconRoot()"/>
        /// <param name="value">The value to associate with this element.</param>
        public HoconRoot(HoconValue value) : this(value, Enumerable.Empty<HoconSubstitution>())
        { }

        /// <inheritdoc cref="HoconRoot()"/>
        /// <param name="value">The value to associate with this element.</param>
        /// <param name="substitutions">An enumeration of substitutions to associate with this element.</param>
        public HoconRoot(HoconValue value, IEnumerable<HoconSubstitution> substitutions)
        {
            Value = value;
            Substitutions = substitutions;
        }

        protected virtual HoconValue GetNode(HoconPath path, bool throwIfNotFound = false)
        {
            if(Value.Type != HoconType.Object)
                throw new HoconException("Hocon is not an object.");

            try
            {
                return Value.GetObject().GetValue(path);
            }
            catch (KeyNotFoundException)
            {
                if (throwIfNotFound)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// Determine if a HOCON configuration element exists at the specified location
        /// </summary>
        /// <param name="path">The location to check for a configuration value.</param>
        /// <returns><c>true</c> if a value was found, <c>false</c> otherwise.</returns>
        public virtual bool HasPath(string path)
            => HasPath(HoconPath.Parse(path));

        /// <summary>
        /// Determine if a HOCON configuration element exists at the specified location
        /// </summary>
        /// <param name="path">The location to check for a configuration value.</param>
        /// <returns><c>true</c> if a value was found, <c>false</c> otherwise.</returns>
        public virtual bool HasPath(HoconPath path)
        {
            HoconValue node;
            try
            {
                node = GetNode(path);
            }
            catch
            {
                return false;
            }
            return node != null;
        }
            

        /// <summary>
        /// Normalize the values inside all Hocon fields to the simplest value possible under Hocon spec.
        /// NOTE: You might not be able to reproduce a clean reproduction of the original configuration file after this normalization.
        /// </summary>
        public void Normalize()
        {
            Flatten(Value);
        }

        private static void Flatten(IHoconElement node)
        {
            if (!(node is HoconValue v))
                return;

            switch (v.Type)
            {
                case HoconType.Object:
                    var o = v.GetObject();
                    v.Clear();
                    v.Add(o);
                    foreach (var item in o.Values)
                        Flatten(item);
                    break;

                case HoconType.Array:
                    var a = v.GetArray();
                    v.Clear();
                    var newArray = new HoconArray(v);
                    foreach (var item in a)
                    {
                        Flatten(item);
                        newArray.Add(item);
                    }
                    v.Add(newArray);
                    break;

                case HoconType.Literal:
                    if (v.Count == 1)
                        return;

                    var value = v.GetString();
                    v.Clear();
                    if (value == null)
                        v.Add(new HoconNull(v));
                    else if (value.NeedTripleQuotes())
                        v.Add(new HoconTripleQuotedString(v, value));
                    else if (value.NeedQuotes())
                        v.Add(new HoconQuotedString(v, value));
                    else
                        v.Add(new HoconUnquotedString(v, value));
                    break;
            }
        }

        #region Value getter methods

        /// <summary>
        /// Retrieves a string value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The string value defined in the specified path.</returns>
        public string GetString(string path, string @default = null)
            => WrapWithValueException(path, () => GetString(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetString(string,string)"/>
        public string GetString(HoconPath path, string @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetString();
            });
        }

        /// <summary>
        /// Retrieves a boolean value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The boolean value defined in the specified path.</returns>
        public virtual bool GetBoolean(string path, bool @default = false)
            => WrapWithValueException(path, () => GetBoolean(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetBoolean(string,bool)"/>
        public virtual bool GetBoolean(HoconPath path, bool @default = false)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetBoolean();
            });
        }

        /// <summary>
        /// Retrieves a long value, optionally suffixed with a 'b', from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The long value defined in the specified path, or null if path was not found.</returns>
        public virtual long? GetByteSize(string path)
            => WrapWithValueException(path, () => GetByteSize(HoconPath.Parse(path)));

        /// <inheritdoc cref="GetByteSize(string)"/>
        public virtual long? GetByteSize(HoconPath path)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value?.GetByteSize();
            });
        }

        /// <summary>
        /// Retrieves an integer value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The integer value defined in the specified path.</returns>
        public virtual int GetInt(string path, int @default = 0)
            => WrapWithValueException(path, () => GetInt(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetInt(string,int)"/>
        public virtual int GetInt(HoconPath path, int @default = 0)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetInt();
            });
        }

        /// <summary>
        /// Retrieves a long value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The long value defined in the specified path.</returns>
        public virtual long GetLong(string path, long @default = 0)
            => WrapWithValueException(path, () => GetLong(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetLong(string,long)"/>
        public virtual long GetLong(HoconPath path, long @default = 0)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetLong();
            });
        }

        /// <summary>
        /// Retrieves a byte value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The byte value defined in the specified path.</returns>
        public virtual byte GetByte(string path, byte @default = 0)
            => WrapWithValueException(path, () => GetByte(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetByte(string,byte)"/>
        public virtual byte GetByte(HoconPath path, byte @default = 0)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetByte();
            });
        }

        /// <summary>
        /// Retrieves a float value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The float value defined in the specified path.</returns>
        public virtual float GetFloat(string path, float @default = 0)
            => WrapWithValueException(path, () => GetFloat(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetFloat(string,float)"/>
        public virtual float GetFloat(HoconPath path, float @default = 0)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetFloat();
            });
        }

        /// <summary>
        /// Retrieves a decimal value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The decimal value defined in the specified path.</returns>
        public virtual decimal GetDecimal(string path, decimal @default = 0)
            => WrapWithValueException(path, () => GetDecimal(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetDecimal(string,decimal)"/>
        public virtual decimal GetDecimal(HoconPath path, decimal @default = 0)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetDecimal();
            });
        }

        /// <summary>
        /// Retrieves a double value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The double value defined in the specified path.</returns>
        public virtual double GetDouble(string path, double @default = 0)
            => WrapWithValueException(path, () => GetDouble(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetDouble(string,double)"/>
        public virtual double GetDouble(HoconPath path, double @default = 0)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetDouble();
            });
        }

        /// <summary>
        /// Retrieves an object from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The double value defined in the specified path.</returns>
        public virtual HoconObject GetObject(string path, HoconObject @default = null)
            => WrapWithValueException(path, () => GetObject(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetObject(string,HoconObject)"/>
        public virtual HoconObject GetObject(HoconPath path, HoconObject @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetObject();
            });
        }
        
        /// <summary>
        /// Retrieves a list of boolean values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of boolean values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<Boolean> GetBooleanList(string path)
            => WrapWithValueException(path, () => GetBooleanList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));

        /// <summary>
        /// Retrieves a list of boolean values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of boolean values defined in the specified path.</returns>
        public virtual IList<Boolean> GetBooleanList(string path, IList<Boolean> @default)
            => WrapWithValueException(path, () => GetBooleanList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetBooleanList(string)"/>
        public virtual IList<Boolean> GetBooleanList(HoconPath path, IList<Boolean> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetBooleanList();
            });
        }

        /// <summary>
        /// Retrieves a list of decimal values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of decimal values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<decimal> GetDecimalList(string path)
            => WrapWithValueException(path, () => GetDecimalList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));

        /// <summary>
        /// Retrieves a list of decimal values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of decimal values defined in the specified path.</returns>
        public virtual IList<decimal> GetDecimalList(string path, IList<decimal> @default)
            => WrapWithValueException(path, () => GetDecimalList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetDecimalList(string)"/>
        public virtual IList<decimal> GetDecimalList(HoconPath path, IList<decimal> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetDecimalList();
            });
        }

        /// <summary>
        /// Retrieves a list of float values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of float values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<float> GetFloatList(string path)
            => WrapWithValueException(path, () => GetFloatList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));

        /// <summary>
        /// Retrieves a list of float values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of float values defined in the specified path.</returns>
        public virtual IList<float> GetFloatList(string path, IList<float> @default)
            => WrapWithValueException(path, () => GetFloatList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetFloatList(string)"/>
        public virtual IList<float> GetFloatList(HoconPath path, IList<float> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetFloatList();
            });
        }

        /// <summary>
        /// Retrieves a list of double values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of double values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<double> GetDoubleList(string path)
            => WrapWithValueException(path, () => GetDoubleList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));
        
        /// <summary>
        /// Retrieves a list of double values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of double values defined in the specified path.</returns>
        public virtual IList<double> GetDoubleList(string path, IList<double> @default)
            => WrapWithValueException(path, () => GetDoubleList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetDoubleList(string)"/>
        public virtual IList<double> GetDoubleList(HoconPath path, IList<double> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetDoubleList();
            });
        }

        /// <summary>
        /// Retrieves a list of int values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of int values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<int> GetIntList(string path)
            => WrapWithValueException(path, () => GetIntList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));
        
        /// <summary>
        /// Retrieves a list of int values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of int values defined in the specified path.</returns>
        public virtual IList<int> GetIntList(string path, IList<int> @default)
            => WrapWithValueException(path, () => GetIntList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetIntList(string)"/>
        public virtual IList<int> GetIntList(HoconPath path, IList<int> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetIntList();
            });
        }

        /// <summary>
        /// Retrieves a list of long values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of long values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<long> GetLongList(string path)
            => WrapWithValueException(path, () => GetLongList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));

        /// <summary>
        /// Retrieves a list of long values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of long values defined in the specified path.</returns>
        public virtual IList<long> GetLongList(string path, IList<long> @default)
            => WrapWithValueException(path, () => GetLongList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetLongList(string)"/>
        public virtual IList<long> GetLongList(HoconPath path, IList<long> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetLongList();
            });
        }

        /// <summary>
        /// Retrieves a list of byte values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of byte values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<byte> GetByteList(string path)
            => WrapWithValueException(path, () => GetByteList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));
        
        /// <summary>
        /// Retrieves a list of byte values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of byte values defined in the specified path.</returns>
        public virtual IList<byte> GetByteList(string path, IList<byte> @default)
            => WrapWithValueException(path, () => GetByteList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetByteList(string)"/>
        public virtual IList<byte> GetByteList(HoconPath path, IList<byte> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetByteList();
            });
        }

        /// <summary>
        /// Retrieves a list of string values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of string values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<string> GetStringList(string path)
            => WrapWithValueException(path, () => GetStringList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));
        
        /// <summary>
        /// Retrieves a list of string values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of string values defined in the specified path.</returns>
        public virtual IList<string> GetStringList(string path, IList<string> @default)
            => WrapWithValueException(path, () => GetStringList(HoconPath.Parse(path), @default));

        /// <inheritdoc cref="GetStringList(string)"/>
        public virtual IList<string> GetStringList(HoconPath path, IList<string> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetStringList();
            });
        }

        /// <summary>
        /// Retrieves a list of objects from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the objects to retrieve.</param>
        /// <returns>The list of objects defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<HoconObject> GetObjectList(string path)
            => WrapWithValueException(path, () => GetObjectList(HoconPath.Parse(path)) ?? throw new HoconParserException($"Hocon path {path} does not exist."));

        /// <summary>
        /// Retrieves a list of objects from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the objects to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of objects defined in the specified path.</returns>
        public virtual IList<HoconObject> GetObjectList(string path, IList<HoconObject> @default)
            => WrapWithValueException(path, () => GetObjectList(HoconPath.Parse(path), @default));
        
        /// <inheritdoc cref="GetObjectList(string)"/>
        public virtual IList<HoconObject> GetObjectList(HoconPath path, IList<HoconObject> @default = null)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default : value.GetObjectList();
            });
        }

        /// <summary>
        /// Retrieves a <see cref="HoconValue"/> from a specific path.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The <see cref="HoconValue"/> found at the location if one exists, otherwise <c>null</c>.</returns>
        public virtual HoconValue GetValue(string path)
            => WrapWithValueException(path, () => GetValue(HoconPath.Parse(path)));

        /// <inheritdoc cref="GetValue(string)"/>
        public virtual HoconValue GetValue(HoconPath path)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value;
            });
        }

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(string path, TimeSpan? @default = null, bool allowInfinite = true)
            => WrapWithValueException(path, () => GetMillisDuration(HoconPath.Parse(path), @default, allowInfinite));

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(HoconPath path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            return WrapWithValueException(path.ToString(), () => GetTimeSpan(path, @default, allowInfinite));
        }

        /// <summary>
        /// Retrieves a <see cref="TimeSpan"/> value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <param name="allowInfinite"><c>true</c> if infinite timespans are allowed; otherwise <c>false</c>.</param>
        /// <returns>The <see cref="TimeSpan"/> value defined in the specified path.</returns>
        public virtual TimeSpan GetTimeSpan(string path, TimeSpan? @default = null, bool allowInfinite = true)
            => WrapWithValueException(path, () => GetTimeSpan(HoconPath.Parse(path), @default, allowInfinite));

        /// <inheritdoc cref="GetTimeSpan(string,System.Nullable{System.TimeSpan},bool)"/>
        public virtual TimeSpan GetTimeSpan(HoconPath path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            return WrapWithValueException(path.ToString(), () =>
            {
                var value = GetNode(path);
                return value == null ? @default.GetValueOrDefault() : value.GetTimeSpan(allowInfinite);
            });
        }
        #endregion

        /// <summary>
        /// Retrieves an enumerable key value pair representation of the current configuration.
        /// </summary>
        /// <returns>The current configuration represented as an enumerable key value pair.</returns>
        public virtual IEnumerable<KeyValuePair<string, HoconField>> AsEnumerable()
        {
            return Value.GetObject();
        }

        /// <summary>
        /// Converts the current configuration to a string.
        /// </summary>
        /// <returns>A string containing the current configuration.</returns>
        public override string ToString()
            => Value?.ToString() ?? "";

        /// <summary>
        /// Converts the current configuration to a pretty printed string.
        /// </summary>
        /// <param name="indentSize">The number of spaces used for each indent.</param>
        /// <returns>A string containing the current configuration.</returns>
        public string PrettyPrint(int indentSize)
            => Value.ToString(1, indentSize);

        /// <summary>
        /// Wraps any exception into <see cref="HoconValueException"/> with failure path specified
        /// </summary>
        private T WrapWithValueException<T>(string path, Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }
    }
}

