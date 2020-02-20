// -----------------------------------------------------------------------
// <copyright file="HoconImmutableObject.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Hocon
{
    public class HoconObject :
        HoconElement,
        IReadOnlyDictionary<string, HoconElement>,
        IEquatable<HoconObject>
    {
        public static readonly HoconObject Empty = new HoconObject();

        private readonly ImmutableSortedDictionary<string, HoconElement> _fields;

        protected HoconObject()
        {
            _fields = ImmutableSortedDictionary<string, HoconElement>.Empty;
        }

        protected HoconObject(HoconElement source)
        {
            if (!(source is HoconObject other))
                throw new HoconException($"Could not create HoconObject from {source.GetType()}");

            _fields = other.ToImmutableSortedDictionary();
        }

        protected HoconObject(IDictionary<string, HoconElement> fields)
        {
            _fields = fields.ToImmutableSortedDictionary();
        }

        /*
        protected HoconObject(IReadOnlyDictionary<string, HoconElement> fields)
        {
            _fields = fields.ToImmutableSortedDictionary();
        }
        */

        public HoconElement this[HoconPath path] => GetValue(path);

        public new HoconElement this[string path] => GetValue(path);

        public IEnumerable<string> Keys => _fields.Keys;
        public IEnumerable<HoconElement> Values => _fields.Values;
        public int Count => _fields.Count;

        public bool HasPath(string path)
        {
            return TryGetValue(path, out _);
        }

        public bool HasPath(HoconPath path)
        {
            return TryGetValue(path, out _);
        }

        public HoconArray ToArray()
        {
            var sortedDict = new SortedDictionary<int, HoconElement>();
            Type type = null;
            foreach (var kvp in _fields)
            {
                if (!int.TryParse(kvp.Key, out var index) || index < 0)
                    continue;
                if (type == null)
                    type = kvp.Value.GetType();
                else if (type != kvp.Value.GetType())
                    throw new HoconException($"Array element mismatch. Expected: {type} Found: {kvp.Value.GetType()}");
                sortedDict[index] = kvp.Value;
            }

            if (sortedDict.Count == 0)
                throw new HoconException(
                    "Object is empty, does not contain any numerically indexed fields, or contains only non-positive integer indices");

            return HoconArray.Create(sortedDict.Values);
        }

        public HoconObject Merge(HoconObject other)
        {
            return new HoconObjectBuilder(this).Merge(other).Build();
        }

        /// <summary>
        ///     Converts the current HoconObject to a pretty printed string.
        /// </summary>
        /// <param name="indentSize">The number of spaces used for each indent.</param>
        /// <returns>A string containing the current configuration.</returns>
        public string PrettyPrint(int indentSize)
        {
            return ToString(1, indentSize);
        }

        public override string ToString(int indent, int indentSize)
        {
            var i = new string(' ', indent * indentSize);
            var sb = new StringBuilder();
            foreach (var field in this)
                sb.Append($"{i}{field.Key} : {field.Value.ToString(indent + 1, indentSize)},{Environment.NewLine}");
            return sb.Length > 2 ? sb.ToString(0, sb.Length - Environment.NewLine.Length - 1) : sb.ToString();
        }

        internal static HoconObject Create(IDictionary<string, HoconElement> fields)
        {
            return new HoconObject(fields);
        }
        /// <summary>
        ///     Wraps any exception into <see cref="HoconValueException" /> with failure path specified
        /// </summary>
        protected T WrapWithValueException<T>(string path, Func<T> func)
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

        protected T WrapWithValueException<T>(HoconPath path, Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }


        #region Interface implementation

        public HoconElement GetValue(string path)
        {
            return GetValue(HoconPath.Parse(path));
        }

        public HoconElement GetValue(HoconPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Count == 0)
                throw new ArgumentException("Path is empty.", nameof(path));

            var pathIndex = 0;
            var currentObject = this;
            while (true)
            {
                var key = path[pathIndex];

                if (!currentObject._fields.TryGetValue(key, out var field))
                    throw new KeyNotFoundException(
                        $"Could not find field with key `{key}` at path `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                if (pathIndex >= path.Count - 1)
                    return field;

                if (!(field is HoconObject obj))
                    throw new HoconException(
                        $"Invalid path, trying to access a key on a non object field. Path: `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                currentObject = obj;
                pathIndex++;
            }
        }

        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        public bool TryGetValue(string key, out HoconElement result)
        {
            result = null;
            return !string.IsNullOrWhiteSpace(key) && TryGetValue(HoconPath.Parse(key), out result);
        }

        public bool TryGetValue(HoconPath path, out HoconElement result)
        {
            result = null;
            if (path == null || path.Count == 0)
                return false;

            var pathIndex = 0;
            var currentObject = this;
            while (true)
            {
                var key = path[pathIndex];

                if (!currentObject._fields.TryGetValue(key, out var field))
                    return false;

                if (pathIndex >= path.Count - 1)
                {
                    result = field;
                    return true;
                }

                if (!(field is HoconObject obj))
                    return false;

                currentObject = obj;
                pathIndex++;
            }
        }

        public IEnumerator<KeyValuePair<string, HoconElement>> GetEnumerator()
        {
            return !_fields.IsEmpty
                ? _fields.GetEnumerator()
                : Enumerable.Empty<KeyValuePair<string, HoconElement>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is HoconObject otherObject))
                return false;
            return Equals(otherObject);
        }

        public bool Equals(HoconObject other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            foreach (var kvp in other)
            {
                if (!TryGetValue(kvp.Key, out var thisValue))
                    return false;
                if (thisValue != kvp.Value)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return -813220765 + 
                EqualityComparer<ImmutableSortedDictionary<string, HoconElement>>
                .Default
                .GetHashCode(_fields);
        }

        #endregion

        #region Casting operators

        public static implicit operator bool[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator sbyte[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator byte[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator short[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator ushort[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator int[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator uint[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator long[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator ulong[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator BigInteger[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator float[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator double[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator decimal[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator TimeSpan[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator string[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator char[](HoconObject obj)
        {
            return obj.ToArray();
        }

        #endregion

        #region Getter functions

        #region Single value getters
        /// <summary>
        ///     Retrieves a string value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The string value defined in the specified path.</returns>
        public virtual string GetString(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual string GetString(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a string value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The string value defined in the specified path.</returns>
        public virtual string GetString(string path, string @default)
        {
            if (TryGetValue(path, out var value))
                if (value.TryGetString(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetString(string,string)" />
        public virtual string GetString(HoconPath path, string @default)
        {
            if (TryGetValue(path, out var value))
                if (value.TryGetString(out var result))
                    return result;

            return @default;
        }

        public virtual bool GetBoolean(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual bool GetBoolean(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a boolean value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The boolean value defined in the specified path.</returns>
        public virtual bool GetBoolean(string path, bool @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetBoolean(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetBoolean(string,bool)" />
        public virtual bool GetBoolean(HoconPath path, bool @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetBoolean(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a long value, optionally suffixed with a 'b', from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The long value defined in the specified path, or null if path was not found.</returns>
        public virtual long? GetByteSize(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetByteSize());
        }

        /// <inheritdoc cref="GetByteSize(string)" />
        public virtual long? GetByteSize(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetByteSize());
        }

        public virtual long? GetByteSize(string path, long? @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetByteSize(out var longValue))
                    return longValue;

            return null;
        }

        /// <inheritdoc cref="GetByteSize(string)" />
        public virtual long? GetByteSize(HoconPath path, long? @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetByteSize(out var longValue))
                    return longValue;

            return null;
        }

        /// <summary>
        ///     Retrieves an integer value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The integer value defined in the specified path.</returns>
        public virtual int GetInt(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual int GetInt(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves an integer value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The integer value defined in the specified path.</returns>
        public virtual int GetInt(string path, int @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetInt(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetInt(string,int)" />
        public virtual int GetInt(HoconPath path, int @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetInt(out var result))
                    return result;

            return @default;
        }

        public virtual long GetLong(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual long GetLong(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a long value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The long value defined in the specified path.</returns>
        public virtual long GetLong(string path, long @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetLong(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetLong(string,long)" />
        public virtual long GetLong(HoconPath path, long @default = 0)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetLong(out var result))
                    return result;

            return @default;
        }

        public virtual byte GetByte(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual byte GetByte(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a byte value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The byte value defined in the specified path.</returns>
        public virtual byte GetByte(string path, byte @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetByte(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetByte(string,byte)" />
        public virtual byte GetByte(HoconPath path, byte @default = 0)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetByte(out var result))
                    return result;

            return @default;
        }

        public virtual float GetFloat(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual float GetFloat(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a float value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The float value defined in the specified path.</returns>
        public virtual float GetFloat(string path, float @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetFloat(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetFloat(string,float)" />
        public virtual float GetFloat(HoconPath path, float @default = 0)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetFloat(out var result))
                    return result;

            return @default;
        }

        public virtual decimal GetDecimal(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual decimal GetDecimal(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a decimal value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The decimal value defined in the specified path.</returns>
        public virtual decimal GetDecimal(string path, decimal @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDecimal(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetDecimal(string,decimal)" />
        public virtual decimal GetDecimal(HoconPath path, decimal @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDecimal(out var result))
                    return result;

            return @default;
        }

        public virtual double GetDouble(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual double GetDouble(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves a double value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The double value defined in the specified path.</returns>
        public virtual double GetDouble(string path, double @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDouble(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetDouble(string,double)" />
        public virtual double GetDouble(HoconPath path, double @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDouble(out var result))
                    return result;

            return @default;
        }

        public virtual HoconObject GetObject(string path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        public virtual HoconObject GetObject(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path]);
        }

        /// <summary>
        ///     Retrieves an object from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The double value defined in the specified path.</returns>
        public virtual InternalHoconObject GetObject(string path, InternalHoconObject @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetObject(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetObject(string,InternalHoconObject)" />
        public virtual InternalHoconObject GetObject(HoconPath path, InternalHoconObject @default = null)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetObject(out var result))
                    return result;

            return @default;
        }
        #endregion

        #region Array value getters
        /// <summary>
        ///     Retrieves a list of boolean values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of boolean values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<bool> GetBooleanList(string path)
        {
            return WrapWithValueException(path, () => { return GetNode(path).GetBooleanList(); });
        }

        public virtual IList<bool> GetBooleanList(HoconPath path)
        {
            return WrapWithValueException(path, () => { return GetNode(path).GetBooleanList(); });
        }

        /// <inheritdoc cref="GetBooleanList(string)" />
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        public virtual IList<bool> GetBooleanList(string path, IList<bool> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetBooleanList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetBooleanList(string)" />
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        public virtual IList<bool> GetBooleanList(HoconPath path, IList<bool> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetBooleanList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of decimal values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of decimal values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<decimal> GetDecimalList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetDecimalList());
        }

        public virtual IList<decimal> GetDecimalList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetDecimalList());
        }

        /// <summary>
        ///     Retrieves a list of decimal values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of decimal values defined in the specified path.</returns>
        public virtual IList<decimal> GetDecimalList(string path, IList<decimal> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDecimalList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetDecimalList(string)" />
        public virtual IList<decimal> GetDecimalList(HoconPath path, IList<decimal> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDecimalList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of float values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of float values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<float> GetFloatList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetFloatList());
        }

        public virtual IList<float> GetFloatList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetFloatList());
        }

        /// <summary>
        ///     Retrieves a list of float values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of float values defined in the specified path.</returns>
        public virtual IList<float> GetFloatList(string path, IList<float> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetFloatList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetFloatList(string)" />
        public virtual IList<float> GetFloatList(HoconPath path, IList<float> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetFloatList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of double values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of double values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<double> GetDoubleList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetDoubleList());
        }

        public virtual IList<double> GetDoubleList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetDoubleList());
        }

        /// <summary>
        ///     Retrieves a list of double values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of double values defined in the specified path.</returns>
        public virtual IList<double> GetDoubleList(string path, IList<double> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDoubleList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetDoubleList(string)" />
        public virtual IList<double> GetDoubleList(HoconPath path, IList<double> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetDoubleList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of int values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of int values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<int> GetIntList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetIntList());
        }

        public virtual IList<int> GetIntList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetIntList());
        }

        /// <summary>
        ///     Retrieves a list of int values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of int values defined in the specified path.</returns>
        public virtual IList<int> GetIntList(string path, IList<int> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetIntList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetIntList(string)" />
        public virtual IList<int> GetIntList(HoconPath path, IList<int> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetIntList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of long values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of long values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<long> GetLongList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetLongList());
        }

        public virtual IList<long> GetLongList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetLongList());
        }

        /// <summary>
        ///     Retrieves a list of long values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of long values defined in the specified path.</returns>
        public virtual IList<long> GetLongList(string path, IList<long> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetLongList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetLongList(string)" />
        public virtual IList<long> GetLongList(HoconPath path, IList<long> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetLongList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of byte values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of byte values defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<byte> GetByteList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetByteList());
        }

        public virtual IList<byte> GetByteList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetByteList());
        }

        /// <summary>
        ///     Retrieves a list of byte values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of byte values defined in the specified path.</returns>
        public virtual IList<byte> GetByteList(string path, IList<byte> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetByteList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetByteList(string)" />
        public virtual IList<byte> GetByteList(HoconPath path, IList<byte> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetByteList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of string values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <returns>The list of string values defined in the specified path.</returns>
        public virtual IList<string> GetStringList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetStringList() ?? new List<string>());
        }

        public virtual IList<string> GetStringList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetStringList() ?? new List<string>());
        }

        /// <summary>
        ///     Retrieves a list of string values from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the values to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of string values defined in the specified path.</returns>
        public virtual IList<string> GetStringList(string path, IList<string> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetStringList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetStringList(string)" />
        public virtual IList<string> GetStringList(HoconPath path, IList<string> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetStringList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a list of objects from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the objects to retrieve.</param>
        /// <returns>The list of objects defined in the specified path.</returns>
        /// <exception cref="HoconParserException">Thrown if path does not exist</exception>
        public virtual IList<InternalHoconObject> GetObjectList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetObjectList());
        }

        public virtual IList<InternalHoconObject> GetObjectList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetObjectList());
        }

        /// <summary>
        ///     Retrieves a list of objects from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the objects to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of objects defined in the specified path.</returns>
        public virtual IList<InternalHoconObject> GetObjectList(string path, IList<InternalHoconObject> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetObjectList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetObjectList(string)" />
        public virtual IList<InternalHoconObject> GetObjectList(HoconPath path, IList<InternalHoconObject> @default = null)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetObjectList(out var result))
                    return result;

            return @default;
        }

        /// <summary>
        ///     Retrieves a <see cref="HoconValue" /> from a specific path.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The <see cref="HoconValue" /> found at the location if one exists, otherwise <c>null</c>.</returns>
        public virtual HoconValue GetValue(string path)
        {
            return WrapWithValueException(path, () => GetNode(path));
        }

        /// <inheritdoc cref="GetValue(string)" />
        public virtual HoconValue GetValue(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path));
        }

        public virtual bool TryGetValue(string path, out HoconValue result)
        {
            return TryGetNode(path, out result);
        }

        public virtual bool TryGetValue(HoconPath path, out HoconValue result)
        {
            return TryGetNode(path, out result);
        }

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(string path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            return WrapWithValueException(path,
                () => GetMillisDuration(HoconPath.Parse(path), @default, allowInfinite));
        }

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(HoconPath path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            return WrapWithValueException(path.ToString(), () => GetTimeSpan(path, @default, allowInfinite));
        }

        /// <summary>
        ///     Retrieves a <see cref="TimeSpan" /> value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <param name="allowInfinite"><c>true</c> if infinite timespans are allowed; otherwise <c>false</c>.</param>
        /// <returns>The <see cref="TimeSpan" /> value defined in the specified path.</returns>
        public virtual TimeSpan GetTimeSpan(string path, bool allowInfinite = true)
        {
            return WrapWithValueException(path, () => GetNode(path).GetTimeSpan(allowInfinite));
        }

        public virtual TimeSpan GetTimeSpan(HoconPath path, bool allowInfinite = true)
        {
            return WrapWithValueException(path, () => GetNode(path).GetTimeSpan(allowInfinite));
        }

        public virtual TimeSpan GetTimeSpan(string path, TimeSpan? @default, bool allowInfinite = true)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetTimeSpan(out var result, allowInfinite))
                    return result;

            return @default.GetValueOrDefault();
        }

        public virtual TimeSpan GetTimeSpan(HoconPath path, TimeSpan? @default, bool allowInfinite = true)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetTimeSpan(out var result, allowInfinite))
                    return result;

            return @default.GetValueOrDefault();
        }
        #endregion        #endregion
    }
}