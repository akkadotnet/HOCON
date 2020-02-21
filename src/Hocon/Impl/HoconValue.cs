// -----------------------------------------------------------------------
// <copyright file="HoconValue.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Hocon
{
    /// <summary>
    ///     This class represents the root type for a HOCON (Human-Optimized Config Object Notation)
    ///     configuration object.
    /// </summary>
    internal class HoconValue : List<IHoconElement>, IHoconElement
    {
        private static readonly Regex TimeSpanRegex = new Regex(
            @"^(?<value>([0-9]+(\.[0-9]+)?))\s*(?<unit>(nanoseconds|nanosecond|nanos|nano|ns|microseconds|microsecond|micros|micro|us|milliseconds|millisecond|millis|milli|ms|seconds|second|s|minutes|minute|m|hours|hour|h|days|day|d))$",
            RegexOptions.Compiled);

        /// <summary>
        ///     Initializes a new instance of the <see cref="HoconValue" /> class.
        /// </summary>
        public HoconValue(IHoconElement parent)
        {
            if (parent != null && !(parent is HoconField) && !(parent is InternalHoconArray))
                throw new HoconException("HoconValue parent must be HoconField, HoconArray, or null");
            Parent = parent;
        }

        public ReadOnlyCollection<IHoconElement> Children => AsReadOnly();

        public IHoconElement Parent { get; }

        public virtual HoconType Type { get; private set; } = HoconType.Empty;

        /// <inheritdoc />
        public virtual InternalHoconObject GetObject()
        {
            if (Type == HoconType.Empty)
                return null;

            if (Type != HoconType.Object)
                throw new HoconException($"Could not convert HoconValue of type {Type} into {nameof(HoconType.Object)}");

            var objects = this
                .Where(value => value.Type == HoconType.Object)
                .Select(value => value.GetObject()).ToList();

            switch (objects.Count)
            {
                case 0:
                    return null;
                case 1:
                    return objects[0];
                default:
                    return new HoconMergedObject(this, objects);
            }
        }

        public virtual bool TryGetObject(out InternalHoconObject result)
        {
            result = null;
            if (Type != HoconType.Object)
                return false;

            var objects = this
                .Where(value => value.Type == HoconType.Object)
                .Select(value => value.GetObject()).ToList();

            switch (objects.Count)
            {
                case 0:
                    break;
                case 1:
                    result = objects[0];
                    break;
                default:
                    result = new HoconMergedObject(this, objects);
                    break;
            }
            return true;
        }

        /// <summary>
        ///     Retrieves the string value from this <see cref="T:Hocon.HoconValue" />.
        /// </summary>
        /// <returns>The string value represented by this <see cref="T:Hocon.HoconValue" />.</returns>
        public virtual string GetString()
        {
            return Type.IsLiteral() ? 
                ConcatString() : 
                throw new HoconException($"Could not convert '{Type}' to string.");
        }

        public virtual bool TryGetString(out string result)
        {
            result = null;
            if (Type.IsLiteral())
            {
                result = ConcatString();
                return true;
            }
            return false;
        }

        public virtual string Raw
            => !Type.IsLiteral() ? null : ConcatRawString();

        /// <summary>
        ///     Retrieves a list of values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of values represented by this <see cref="HoconValue" />.</returns>
        public virtual IList<HoconValue> GetArray()
        {
            switch (Type)
            {
                case HoconType.Array:
                    var result = new List<HoconValue>();
                    foreach (var element in this)
                        if (element.Type == HoconType.Array)
                            result.AddRange(element.GetArray());

                    return result;

                case HoconType.Object:
                    return GetObject().GetArray();

                case HoconType.Boolean:
                case HoconType.String:
                case HoconType.Number:
                    throw new HoconException("Hocon literal could not be converted to array.");

                case HoconType.Empty:
                    throw new HoconException("HoconValue is empty.");

                default:
                    throw new Exception($"Unknown HoconType: {Type}");
            }
        }

        public virtual bool TryGetArray(out List<HoconValue> result)
        {
            result = new List<HoconValue>();
            switch (Type)
            {
                case HoconType.Array:
                    foreach (var element in this)
                        if (element.Type == HoconType.Array)
                            result.AddRange(element.GetArray());

                    return true;

                case HoconType.Object:
                    if (TryGetObject(out var obj))
                        return obj.TryGetArray(out result);
                    return false;

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public virtual string ToString(int indent, int indentSize)
        {
            switch (Type)
            {
                case HoconType.Boolean:
                case HoconType.Number:
                case HoconType.String:
                    return ConcatRawString();
                case HoconType.Object:
                    return
                        $"{{{Environment.NewLine}{GetObject().ToString(indent, indentSize)}{Environment.NewLine}{new string(' ', (indent - 1) * indentSize)}}}";
                case HoconType.Array:
                    return $"[{string.Join(",", GetArray().Select(e => e.ToString(indent, indentSize)))}]";
                case HoconType.Empty:
                    return "<<Empty>>";
                default:
                    return null;
            }
        }

        public virtual IHoconElement Clone(IHoconElement newParent)
        {
            var clone = new HoconValue(newParent);
            foreach (var element in this) clone.Add(element.Clone(clone));
            return clone;
        }

        public new void Clear()
        {
            Type = HoconType.Empty;
            base.Clear();
        }

        /// <summary>
        ///     Wraps this <see cref="HoconValue" /> into a new <see cref="InternalHoconObject" /> at the specified key.
        /// </summary>
        /// <param name="key">The key designated to be the new root element.</param>
        /// <returns>A new HOCON root.</returns>
        /// <remarks>
        ///     Immutable. Performs a deep copy on this <see cref="HoconValue" /> first.
        /// </remarks>
        public HoconRoot AtKey(string key)
        {
            var value = new HoconValue(null);
            var obj = new InternalHoconObject(value);
            var field = new HoconField(key, obj);
            field.SetValue(Clone(field) as HoconValue);
            obj.Add(key, field);
            value.Add(obj);
            
            return new HoconRoot(value);
        }

        /// <summary>
        ///     Merge an <see cref="IHoconElement" /> into this <see cref="HoconValue" />.
        /// </summary>
        /// <param name="value">The <see cref="IHoconElement" /> value to be merged into this <see cref="HoconValue" /></param>
        /// <exception cref="HoconParserException">
        ///     Throws when the merged <see cref="IHoconElement.Type" /> type did not match <see cref="HoconValue.Type" />,
        ///     if <see cref="HoconValue.Type" /> is not <see cref="HoconType.Empty" />.
        /// </exception>
        public new virtual void Add(IHoconElement value)
        {
            if (Type == HoconType.Empty)
            {
                Type = value.Type;
            }
            else
            {
                if (!value.IsSubstitution() && !this.IsMergeable(value))
                    throw new HoconException(
                        $"Hocon value merge mismatch. Existing value: {Type}, merged item: {value.Type}");
            }

            base.Add(value);
        }

        public new virtual void AddRange(IEnumerable<IHoconElement> values)
        {
            foreach (var value in values) Add(value);
        }

        private string ConcatString()
        {
            var array = this.Select(l => l.GetString()).ToArray();
            if (array.All(value => value == null))
                return null;

            var sb = new StringBuilder();
            foreach (var s in array) sb.Append(s);

            return sb.ToString();
        }

        private string ConcatRawString()
        {
            var array = this.Select(l => l.Raw).ToArray();
            if (array.All(value => value == null))
                return null;

            var sb = new StringBuilder();
            foreach (var s in array) sb.Append(s);

            return sb.ToString();
        }

        internal List<HoconSubstitution> GetSubstitutions()
        {
            var x = from v in this
                where v is HoconSubstitution
                select v;

            return x.Cast<HoconSubstitution>().ToList();
        }

        internal void ResolveValue(HoconSubstitution child)
        {
            var index = IndexOf(child);
            Remove(child);

            if (child.Type != HoconType.Empty)
            {
                if (Type == HoconType.Empty)
                    Type = child.Type;
                else if (!Type.IsMergeable(child.Type))
                    throw HoconParserException.Create(child, child.Path,
                        "Invalid substitution, substituted type be must be mergeable with its sibling type. " +
                        $"Sibling type:{Type}, substitution type:{child.Type}");

                var clonedValue = (HoconValue)child.ResolvedValue.Clone(Parent);
                switch (Type)
                {
                    case HoconType.Object:
                        Insert(index, clonedValue.GetObject());
                        break;
                    case HoconType.Array:
                        var hoconArray = new InternalHoconArray(this);
                        hoconArray.AddRange(clonedValue.GetArray());
                        Insert(index, hoconArray);
                        break;
                    case HoconType.Boolean:
                    case HoconType.Number:
                    case HoconType.String:
                        var elementList = new List<IHoconElement>();
                        foreach(var element in clonedValue)
                            elementList.Add(element);
                        InsertRange(index, elementList);
                        break;
                }
            }

            switch (Parent)
            {
                case HoconField v:
                    v.ResolveValue(this);
                    break;
                case InternalHoconArray a:
                    a.ResolveValue(child);
                    break;
                default:
                    throw new Exception($"Invalid parent type while resolving substitution:{Parent.GetType()}");
            }
        }

        internal void ReParent(HoconValue value)
        {
            foreach (var element in value) Add(element.Clone(this));
        }

        /// <summary>
        ///     Returns a HOCON string representation of this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A HOCON string representation of this <see cref="HoconValue" />.</returns>
        public override string ToString()
        {
            return ToString(1, 2);
        }

        // HoconValue is an aggregate of same typed objects, so there are possibilities 
        // where it can match with any other same typed objects (ie. a HoconLiteral can 
        // have the same value as a HoconValue).
        public virtual bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Type != other.Type) return false;

            switch (Type)
            {
                case HoconType.Empty:
                    return other.Type == HoconType.Empty;
                case HoconType.Array:
                    return GetArray().SequenceEqual(other.GetArray());
                case HoconType.Boolean:
                case HoconType.String:
                case HoconType.Number:
                    return string.Equals(GetString(), other.GetString());
                case HoconType.Object:
                    return GetObject() == other.GetObject();
                default:
                    throw new HoconException($"Unknown enumeration value: {Type}");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is IHoconElement value && Equals(value);
        }

        public override int GetHashCode()
        {
            const int seed = 613;
            const int modifier = 41;

            unchecked
            {
                return this.Aggregate(seed, (current, item) => current * modifier + item.GetHashCode());
            }
        }

        public static bool operator ==(HoconValue left, HoconValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconValue left, HoconValue right)
        {
            return !Equals(left, right);
        }

        #region Value Getter methods

        /// <summary>
        ///     Retrieves the boolean value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The boolean value represented by this <see cref="HoconValue" />.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     This exception occurs when the <see cref="HoconValue" /> doesn't
        ///     conform to the standard boolean values: "on", "off", "true", or "false"
        /// </exception>
        public bool GetBoolean()
        {
            switch (GetString())
            {
                case "on":
                case "true":
                case "yes":
                    return true;
                case "off":
                case "false":
                case "no":
                    return false;
                default:
                    throw new HoconException($"Unknown boolean format: {GetString()}");
            }
        }

        public bool TryGetBoolean(out bool result)
        {
            result = default(bool);
            if (!TryGetString(out var value))
                return false;

            switch (value)
            {
                case "on":
                case "true":
                case "yes":
                    result = true;
                    return true;
                default:
                    result = false;
                    return true;
            }
        }

        /// <summary>
        ///     Retrieves the decimal value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="HoconValue" />.</returns>
        public decimal GetDecimal()
        {
            var value = GetString();
            switch (value)
            {
                case "+Infinity":
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    throw new HoconException(
                        $"Could not convert `{value}` to decimal. Decimal does not support Infinity and NaN");
                default:
                    try
                    {
                        return decimal.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new HoconException($"Could not convert `{value}` to decimal.", e);
                    }
            }
        }

        public bool TryGetDecimal(out decimal result)
        {
            result = default(decimal);
            if (!TryGetString(out var value))
                return false;

            switch (value)
            {
                case "+Infinity":
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    return false;
                default:
                    return decimal.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result);
            }
        }

        /// <summary>
        ///     Retrieves the float value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The float value represented by this <see cref="HoconValue" />.</returns>
        public float GetFloat()
        {
            var value = GetString();
            switch (value)
            {
                case "+Infinity":
                case "Infinity":
                    return float.PositiveInfinity;
                case "-Infinity":
                    return float.NegativeInfinity;
                case "NaN":
                    return float.NaN;
                default:
                    try
                    {
                        return float.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new HoconException($"Could not convert `{value}` to float.", e);
                    }
            }
        }

        public bool TryGetFloat(out float result)
        {
            result = default(float);
            if (!TryGetString(out var value))
                return false;
            switch (value)
            {
                case "+Infinity":
                case "Infinity":
                    result = float.PositiveInfinity;
                    return true;
                case "-Infinity":
                    result = float.NegativeInfinity;
                    return true;
                case "NaN":
                    result = float.NaN;
                    return true;
                default:
                    return float.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result);
            }
        }

        /// <summary>
        ///     Retrieves the double value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue" />.</returns>
        public double GetDouble()
        {
            var value = GetString();
            switch (value)
            {
                case "+Infinity":
                case "Infinity":
                    return double.PositiveInfinity;
                case "-Infinity":
                    return double.NegativeInfinity;
                case "NaN":
                    return double.NaN;
                default:
                    try
                    {
                        return double.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new HoconException($"Could not convert `{value}` to double.", e);
                    }
            }
        }

        public bool TryGetDouble(out double result)
        {
            result = default(double);
            if (!TryGetString(out var value))
                return false;

            switch (value)
            {
                case "+Infinity":
                case "Infinity":
                    result = double.PositiveInfinity;
                    return true;
                case "-Infinity":
                    result = double.NegativeInfinity;
                    return true;
                case "NaN":
                    result = double.NaN;
                    return true;
                default:
                    return double.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result);
            }
        }

        /// <summary>
        ///     Retrieves the long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public long GetLong()
        {
            var value = GetString();
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt64(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to long.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToInt64(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{GetString()}` to long.", e);
                }

            try
            {
                return long.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{GetString()}` to long.", e);
            }
        }

        public bool TryGetLong(out long result)
        {
            result = default(long);
            if (!TryGetString(out var value))
                return false;

            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToInt64(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToInt64(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return long.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        /// <summary>
        ///     Retrieves the integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="HoconValue" />.</returns>
        public int GetInt()
        {
            var value = GetString();
            if (string.IsNullOrEmpty(value))
                throw new HoconException($"Could not convert a {Type} into int.");

            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt32(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to int.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToInt32(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{GetString()}` to int.", e);
                }

            try
            {
                return int.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{GetString()}` to int.", e);
            }
        }

        public bool TryGetInt(out int result)
        {
            result = default(int);
            if (!TryGetString(out var value))
                return false;

            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToInt32(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToInt32(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return int.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        /// <summary>
        ///     Retrieves the byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="HoconValue" />.</returns>
        public byte GetByte()
        {
            var value = GetString();
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToByte(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to byte.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToByte(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{GetString()}` to byte.", e);
                }

            try
            {
                return byte.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{GetString()}` to byte.", e);
            }
        }

        public bool TryGetByte(out byte result)
        {
            result = default(byte);
            if (!TryGetString(out var value))
                return false;

            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToByte(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToByte(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return byte.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        /// <summary>
        ///     Retrieves a list of byte values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of byte values represented by this <see cref="HoconValue" />.</returns>
        public IList<byte> GetByteList()
        {
            return GetArray().Select(v => v.GetByte()).ToList();
        }

        public bool TryGetByteList(out IList<byte> result)
        {
            result = default(List<byte>);
            if(TryGetArray(out var arr))
            {
                var list = new List<byte>();
                foreach(var val in arr)
                {
                    if (!val.TryGetByte(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of integer values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of integer values represented by this <see cref="HoconValue" />.</returns>
        public IList<int> GetIntList()
        {
            return GetArray().Select(v => v.GetInt()).ToList();
        }

        public bool TryGetIntList(out IList<int> result)
        {
            result = default(List<int>);
            if (TryGetArray(out var arr))
            {
                var list = new List<int>();
                foreach (var val in arr)
                {
                    if (!val.TryGetInt(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of long values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of long values represented by this <see cref="HoconValue" />.</returns>
        public IList<long> GetLongList()
        {
            return GetArray().Select(v => v.GetLong()).ToList();
        }

        public bool TryGetLongList(out IList<long> result)
        {
            result = default(List<long>);
            if (TryGetArray(out var arr))
            {
                var list = new List<long>();
                foreach (var val in arr)
                {
                    if (!val.TryGetLong(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of boolean values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconValue" />.</returns>
        public IList<bool> GetBooleanList()
        {
            return GetArray().Select(v => v.GetBoolean()).ToList();
        }

        public bool TryGetBooleanList(out IList<bool> result)
        {
            result = default(List<bool>);
            if (TryGetArray(out var arr))
            {
                var list = new List<bool>();
                foreach (var val in arr)
                {
                    if (!val.TryGetBoolean(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of float values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of float values represented by this <see cref="HoconValue" />.</returns>
        public IList<float> GetFloatList()
        {
            return GetArray().Select(v => v.GetFloat()).ToList();
        }

        public bool TryGetFloatList(out IList<float> result)
        {
            result = default(List<float>);
            if (TryGetArray(out var arr))
            {
                var list = new List<float>();
                foreach (var val in arr)
                {
                    if (!val.TryGetFloat(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconValue" />.</returns>
        public IList<double> GetDoubleList()
        {
            return GetArray().Select(v => v.GetDouble()).ToList();
        }

        public bool TryGetDoubleList(out IList<double> result)
        {
            result = default(List<double>);
            if (TryGetArray(out var arr))
            {
                var list = new List<double>();
                foreach (var val in arr)
                {
                    if (!val.TryGetDouble(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of decimal values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of decimal values represented by this <see cref="HoconValue" />.</returns>
        public IList<decimal> GetDecimalList()
        {
            return GetArray().Select(v => v.GetDecimal()).ToList();
        }

        public bool TryGetDecimalList(out IList<decimal> result)
        {
            result = default(List<decimal>);
            if (TryGetArray(out var arr))
            {
                var list = new List<decimal>();
                foreach (var val in arr)
                {
                    if (!val.TryGetDecimal(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of string values from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconValue" />.</returns>
        public IList<string> GetStringList()
        {
            return GetArray().Select(v => v.GetString()).ToList();
        }

        public bool TryGetStringList(out IList<string> result)
        {
            result = default(List<string>);
            if (TryGetArray(out var arr))
            {
                var list = new List<string>();
                foreach (var val in arr)
                {
                    if (!val.TryGetString(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Retrieves a list of objects from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconValue" />.</returns>
        public IList<InternalHoconObject> GetObjectList()
        {
            return GetArray().Select(v => v.GetObject()).ToList();
        }

        public bool TryGetObjectList(out IList<InternalHoconObject> result)
        {
            result = default(List<InternalHoconObject>);
            if (TryGetArray(out var arr))
            {
                var list = new List<InternalHoconObject>();
                foreach (var val in arr)
                {
                    if (!val.TryGetObject(out var res))
                        return false;
                    list.Add(res);
                }
                result = list;
                return true;
            }
            return false;
        }

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(bool allowInfinite = true)
        {
            return GetTimeSpan(allowInfinite);
        }

        /// <summary>
        ///     Retrieves the time span value from this <see cref="HoconValue" />.
        /// </summary>
        /// <param name="allowInfinite">A flag used to set inifinite durations.</param>
        /// <returns>The time span value represented by this <see cref="HoconValue" />.</returns>
        public TimeSpan GetTimeSpan(bool allowInfinite = true)
        {
            string res = GetString();

            if (TryMatchTimeSpan(res, out var result))
                return result;

            if (allowInfinite && res.Equals("infinite", StringComparison.OrdinalIgnoreCase)) //Not in Hocon spec
                return Timeout.InfiniteTimeSpan;

            double value;
            try
            {
                value = double.Parse(res, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e)
            {
                throw new HoconException($"Failed to parse TimeSpan value from '{res}'", e);
            }

            if (value < 0)
                throw new HoconException("Expected a positive value instead of " + value);

            return TimeSpan.FromMilliseconds(value);
        }

        public bool TryGetTimeSpan(out TimeSpan result, bool allowInfinite = true)
        {
            result = default(TimeSpan);
            if (!TryGetString(out var res))
                return false;

            if (TryMatchTimeSpan(res, out result))
                return true;

            if (allowInfinite && res.Equals("infinite", StringComparison.OrdinalIgnoreCase)) //Not in Hocon spec
            {
                result = Timeout.InfiniteTimeSpan;
                return true;
            }

            if (!double.TryParse(res, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out var doubleValue))
                return false;

            if (doubleValue < 0) return false;

            result = TimeSpan.FromMilliseconds(doubleValue);
            return true;
        }

        private bool TryMatchTimeSpan(string value, out TimeSpan result)
        {
            result = default(TimeSpan);

            var match = TimeSpanRegex.Match(value);
            if (!match.Success)
                return false;

            var u = match.Groups["unit"].Value;
            if (!double.TryParse(
                    match.Groups["value"].Value,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    NumberFormatInfo.InvariantInfo,
                    out var v))
                return false;

            if (v < 0) return false;

            switch (u)
            {
                case "nanoseconds":
                case "nanosecond":
                case "nanos":
                case "nano":
                case "ns":
                    result = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * v / 1000000.0));
                    return true;
                case "microseconds":
                case "microsecond":
                case "micros":
                case "micro":
                    result = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * v / 1000.0));
                    return true;
                case "milliseconds":
                case "millisecond":
                case "millis":
                case "milli":
                case "ms":
                    result = TimeSpan.FromMilliseconds(v);
                    return true;
                case "seconds":
                case "second":
                case "s":
                    result = TimeSpan.FromSeconds(v);
                    return true;
                case "minutes":
                case "minute":
                case "m":
                    result = TimeSpan.FromMinutes(v);
                    return true;
                case "hours":
                case "hour":
                case "h":
                    result = TimeSpan.FromHours(v);
                    return true;
                case "days":
                case "day":
                case "d":
                    result = TimeSpan.FromDays(v);
                    return true;
            }
            return false;
        }

        private struct ByteSize
        {
            public long Factor { get; set; }
            public string[] Suffixes { get; set; }
        }

        private static ByteSize[] ByteSizes { get; } =
        {
            new ByteSize
            {
                Factor = 1000L * 1000L * 1000L * 1000L * 1000L * 1000L, Suffixes = new[] {"EB", "exabyte", "exabytes"}
            },
            new ByteSize
            {
                Factor = 1024L * 1024L * 1024L * 1024L * 1024L * 1024L,
                Suffixes = new[] {"E", "e", "Ei", "EiB", "exbibyte", "exbibytes"}
            },
            new ByteSize
            {
                Factor = 1000L * 1000L * 1000L * 1000L * 1000L * 1000L, Suffixes = new[] {"EB", "exabyte", "exabytes"}
            },
            new ByteSize
            {
                Factor = 1024L * 1024L * 1024L * 1024L * 1024L,
                Suffixes = new[] {"P", "p", "Pi", "PiB", "pebibyte", "pebibytes"}
            },
            new ByteSize
                {Factor = 1000L * 1000L * 1000L * 1000L * 1000L, Suffixes = new[] {"PB", "petabyte", "petabytes"}},
            new ByteSize
            {
                Factor = 1024L * 1024L * 1024L * 1024L,
                Suffixes = new[] {"T", "t", "Ti", "TiB", "tebibyte", "tebibytes"}
            },
            new ByteSize {Factor = 1000L * 1000L * 1000L * 1000L, Suffixes = new[] {"TB", "terabyte", "terabytes"}},
            new ByteSize
                {Factor = 1024L * 1024L * 1024L, Suffixes = new[] {"G", "g", "Gi", "GiB", "gibibyte", "gibibytes"}},
            new ByteSize {Factor = 1000L * 1000L * 1000L, Suffixes = new[] {"GB", "gigabyte", "gigabytes"}},
            new ByteSize {Factor = 1024L * 1024L, Suffixes = new[] {"M", "m", "Mi", "MiB", "mebibyte", "mebibytes"}},
            new ByteSize {Factor = 1000L * 1000L, Suffixes = new[] {"MB", "megabyte", "megabytes"}},
            new ByteSize {Factor = 1024L, Suffixes = new[] {"K", "k", "Ki", "KiB", "kibibyte", "kibibytes"}},
            new ByteSize {Factor = 1000L, Suffixes = new[] {"kB", "kilobyte", "kilobytes"}},
            new ByteSize {Factor = 1, Suffixes = new[] {"b", "B", "byte", "bytes"}}
        };

        private static char[] Digits { get; } = "0123456789".ToCharArray();

        /// <summary>
        ///     Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public long? GetByteSize()
        {
            var res = GetString();
            if (string.IsNullOrEmpty(res))
                return null;
            res = res.Trim();
            var index = res.LastIndexOfAny(Digits);
            if (index == -1 || index + 1 >= res.Length)
                return long.Parse(res);

            var value = res.Substring(0, index + 1);
            var unit = res.Substring(index + 1).Trim();

            foreach (var byteSize in ByteSizes)
                foreach (var suffix in byteSize.Suffixes)
                    if (string.Equals(unit, suffix, StringComparison.Ordinal))
                        return (long)(byteSize.Factor * double.Parse(value));

            throw new FormatException($"{unit} is not a valid byte size suffix");
        }

        public bool TryGetByteSize(out long? result)
        {
            result = null;
            if (!TryGetString(out var res))
                return false;
            if (string.IsNullOrEmpty(res))
                return false;

            res = res.Trim();
            var index = res.LastIndexOfAny(Digits);
            if (index == -1 || index + 1 >= res.Length)
            {
                if (!long.TryParse(res, out var longParse))
                    return false;
                result = longParse;
                return true;
            }

            var value = res.Substring(0, index + 1);
            var unit = res.Substring(index + 1).Trim();

            if (!double.TryParse(value, out var doubleParse))
                return false;

            foreach (var byteSize in ByteSizes)
                foreach (var suffix in byteSize.Suffixes)
                    if (string.Equals(unit, suffix, StringComparison.Ordinal))
                    {
                        result = (long)(byteSize.Factor * doubleParse);
                        return true;
                    }
            return false;
        }
        #endregion
    }
}