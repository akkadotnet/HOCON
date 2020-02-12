// -----------------------------------------------------------------------
// <copyright file="HoconRoot.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    /// <summary>
    ///     This class represents the root element in a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    /// </summary>
    public class HoconRoot:IEquatable<HoconRoot>
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Hocon.HoconRoot" /> class.
        /// </summary>
        protected HoconRoot()
        {
            Substitutions = Enumerable.Empty<HoconSubstitution>();
        }

        /// <inheritdoc cref="HoconRoot()" />
        /// <param name="value">The value to associate with this element.</param>
        public HoconRoot(HoconValue value) : this(value, Enumerable.Empty<HoconSubstitution>())
        {
        }

        /// <inheritdoc cref="HoconRoot()" />
        /// <param name="value">The value to associate with this element.</param>
        /// <param name="substitutions">An enumeration of substitutions to associate with this element.</param>
        public HoconRoot(HoconValue value, IEnumerable<HoconSubstitution> substitutions)
        {
            _internalValue = value;
            Substitutions = substitutions;
        }

        protected HoconValue _internalValue;
        /// <summary>
        ///     Retrieves the value associated with this element.
        /// </summary>
        public virtual HoconValue Value {
            get => _internalValue;
            protected set => _internalValue = value;
        }

        /// <summary>
        ///     Retrieves an enumeration of substitutions associated with this element.
        /// </summary>
        public IEnumerable<HoconSubstitution> Substitutions { get; private set; }

        protected virtual bool TryGetNode(string path, out HoconValue result)
        {
            result = null;
            if (!HoconPath.TryParse(path, out var hoconPath))
                return false;
            return TryGetNode(hoconPath, out result);
        }

        protected virtual bool TryGetNode(HoconPath path, out HoconValue result)
        {
            result = null;
            if (Value.Type != HoconType.Object)
                return false;

            var obj = Value.GetObject();
            if (obj == null)
                return false;

            return obj.TryGetValue(path, out result);
        }

        protected virtual HoconValue GetNode(string path)
        {
            return GetNode(HoconPath.Parse(path));
        }

        protected virtual HoconValue GetNode(HoconPath path)
        {
            if (Value.Type != HoconType.Object)
                throw new HoconException("Hocon is not an object.");

            return Value.GetObject().GetValue(path);
        }

        /// <summary>
        ///     Determine if a HOCON configuration element exists at the specified location
        /// </summary>
        /// <param name="path">The location to check for a configuration value.</param>
        /// <returns><c>true</c> if a value was found, <c>false</c> otherwise.</returns>
        public virtual bool HasPath(string path)
        {
            return HasPath(HoconPath.Parse(path));
        }

        /// <summary>
        ///     Determine if a HOCON configuration element exists at the specified location
        /// </summary>
        /// <param name="path">The location to check for a configuration value.</param>
        /// <returns><c>true</c> if a value was found, <c>false</c> otherwise.</returns>
        public virtual bool HasPath(HoconPath path)
        {
            return TryGetNode(path, out var _);
        }


        /// <summary>
        ///     Normalize the values inside all Hocon fields to the simplest value possible under Hocon spec.
        ///     NOTE: You might not be able to reproduce a clean reproduction of the original configuration file after this
        ///     normalization.
        /// </summary>
        public HoconRoot Normalize()
        {
            Flatten(Value);
            Substitutions = Enumerable.Empty<HoconSubstitution>();
            return this;
        }

        private static void Flatten(IHoconElement node)
        {

            if (!(node is HoconValue v))
                return;

            switch (v.Type)
            {
                case HoconType.Object:
                    var o = v.GetObject();
                    if(o is HoconMergedObject mo)
                    {
                        o = new HoconObject(v);
                        foreach(var obj in mo.Objects)
                            o.Merge(obj);
                    }

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

                default:
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

        /// <summary>
        ///     Retrieves an enumerable key value pair representation of the current configuration.
        /// </summary>
        /// <returns>The current configuration represented as an enumerable key value pair.</returns>
        public virtual IEnumerable<KeyValuePair<string, HoconField>> AsEnumerable()
        {
            return Value.GetObject();
        }

        /// <summary>
        ///     Converts the current configuration to a string.
        /// </summary>
        /// <returns>A string containing the current configuration.</returns>
        public override string ToString()
        {
            return Value?.ToString() ?? "";
        }

        /// <summary>
        ///     Converts the current configuration to a pretty printed string.
        /// </summary>
        /// <param name="indentSize">The number of spaces used for each indent.</param>
        /// <returns>A string containing the current configuration.</returns>
        public string PrettyPrint(int indentSize)
        {
            return Value.ToString(1, indentSize);
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

        public bool Equals(HoconRoot other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is HoconRoot root && Equals(root);
        }

        #region Value getter methods

        #region Single value getters
        /// <summary>
        ///     Retrieves a string value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <returns>The string value defined in the specified path.</returns>
        public virtual string GetString(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetString());
        }

        public virtual string GetString(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetString());
        }

        /// <summary>
        ///     Retrieves a string value from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The string value defined in the specified path.</returns>
        public virtual string GetString(string path, string @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetString(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetString(string,string)" />
        public virtual string GetString(HoconPath path, string @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetString(out var result))
                    return result;

            return @default;
        }

        public virtual bool GetBoolean(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetBoolean());
        }

        public virtual bool GetBoolean(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetBoolean());
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
            return WrapWithValueException(path, () => GetNode(path).GetInt());
        }

        public virtual int GetInt(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetInt());
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
            return WrapWithValueException(path, () => GetNode(HoconPath.Parse(path)).GetLong());
        }

        public virtual long GetLong(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetLong());
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
            return WrapWithValueException(path, () => GetNode(path).GetByte());
        }

        public virtual byte GetByte(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetByte());
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
            return WrapWithValueException(path, () => GetNode(path).GetFloat());
        }

        public virtual float GetFloat(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetFloat());
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
            return WrapWithValueException(path, () => GetNode(path).GetDecimal());
        }

        public virtual decimal GetDecimal(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetDecimal());
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
            return WrapWithValueException(path, () => GetNode(path).GetDouble());
        }

        public virtual double GetDouble(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetDouble());
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
            return WrapWithValueException(path, () => GetNode(path).GetObject());
        }

        public virtual HoconObject GetObject(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetObject());
        }

        /// <summary>
        ///     Retrieves an object from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the value to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The double value defined in the specified path.</returns>
        public virtual HoconObject GetObject(string path, HoconObject @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetObject(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetObject(string,HoconObject)" />
        public virtual HoconObject GetObject(HoconPath path, HoconObject @default = null)
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
        public virtual IList<HoconObject> GetObjectList(string path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetObjectList());
        }

        public virtual IList<HoconObject> GetObjectList(HoconPath path)
        {
            return WrapWithValueException(path, () => GetNode(path).GetObjectList());
        }

        /// <summary>
        ///     Retrieves a list of objects from the specified path in the configuration.
        /// </summary>
        /// <param name="path">The path that contains the objects to retrieve.</param>
        /// <param name="default">The default value to return if the value doesn't exist.</param>
        /// <returns>The list of objects defined in the specified path.</returns>
        public virtual IList<HoconObject> GetObjectList(string path, IList<HoconObject> @default)
        {
            if (TryGetNode(path, out var value))
                if (value.TryGetObjectList(out var result))
                    return result;

            return @default;
        }

        /// <inheritdoc cref="GetObjectList(string)" />
        public virtual IList<HoconObject> GetObjectList(HoconPath path, IList<HoconObject> @default = null)
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
        #endregion
        #endregion
    }
}