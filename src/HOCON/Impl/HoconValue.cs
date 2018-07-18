//-----------------------------------------------------------------------
// <copyright file="HoconValue.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Hocon
{
    /// <summary>
    /// This class represents the root type for a HOCON (Human-Optimized Config Object Notation)
    /// configuration object.
    /// </summary>
    public class HoconValue : List<IHoconElement>, IHoconElement
    {
        public static readonly HoconValue Undefined;

        static HoconValue()
        {
            Undefined = new HoconEmptyValue(null);
        }

        public IHoconElement Parent { get; }

        public virtual HoconType Type { get; private set; } = HoconType.Empty;

        public ReadOnlyCollection<IHoconElement> Childrens => AsReadOnly();

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconValue"/> class.
        /// </summary>
        public HoconValue(IHoconElement parent)
        {
            Parent = parent;
        }

        public new void Clear()
        {
            Type = HoconType.Empty;
            base.Clear();
        }

        /// <summary>
        /// Merge an <see cref="IHoconElement"/> into this <see cref="HoconValue"/>.
        /// </summary>
        /// <param name="value">The <see cref="IHoconElement"/> value to be merged into this <see cref="HoconValue"/></param>
        /// <exception cref="HoconParserException">
        /// Throws when the merged <see cref="IHoconElement.Type"/> type did not match <see cref="HoconValue.Type"/>, 
        /// if <see cref="HoconValue.Type"/> is not <see cref="HoconType.Empty"/>.
        /// </exception>
        public new virtual void Add(IHoconElement value)
        {
            if (this.IsSubstitution() || Type == HoconType.Empty)
            {
                Type = value.Type;
            }
            else if (!value.IsSubstitution())
            {
                if(value.Type == HoconType.Empty)
                    return;
                if(Type != value.Type)
                    throw new HoconException($"Hocon value merge mismatch. Existing value: {Type}, merged item: {value.Type}");
            }

            base.Add(value);
        }

        public new virtual void AddRange(IEnumerable<IHoconElement> values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }

        /// <inheritdoc />
        public virtual HoconObject GetObject()
        {
            List<HoconObject> objects = this.Select(value => value.GetObject()).ToList();

            switch (objects.Count)
            {
                case 0:
                    return null;
                case 1:
                    return objects[0];
                default:
                    return new HoconMergedObject(Parent, objects);
            }
        }

        /// <summary>
        /// Retrieves the string value from this <see cref="T:Hocon.HoconValue" />.
        /// </summary>
        /// <returns>The string value represented by this <see cref="T:Hocon.HoconValue" />.</returns>
        public virtual string GetString()
            => Type != HoconType.Literal ? null : ConcatString();

        public virtual string Raw
            => Type != HoconType.Literal ? null : ConcatRawString();

        private string ConcatString()
        {
            var array = this.Select(l => l.GetString()).ToArray();
            return array.All(value => value == null) ? null : string.Join("", array);
        }

        private string ConcatRawString()
        {
            var array = this.Select(l => l.Raw).ToArray();
            return array.All(value => value == null) ? "null" : string.Join("", array);
        }

        /// <summary>
        /// Retrieves a list of values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of values represented by this <see cref="HoconValue"/>.</returns>
        public virtual List<HoconValue> GetArray()
        {
            IEnumerable<HoconValue> x = from value in this
                where value.Type == HoconType.Array
                from e in value.GetArray()
                select e;

            return x.ToList();
        }

        internal List<HoconSubstitution> GetSubstitutions()
        {
            var x = from v in this
                where v is HoconSubstitution
                select v;

            return x.Cast<HoconSubstitution>().ToList();
        }

        #region Value Getter methods

        /// <summary>
        /// Retrieves the boolean value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The boolean value represented by this <see cref="HoconValue"/>.</returns>
        /// <exception cref="System.NotSupportedException">
        /// This exception occurs when the <see cref="HoconValue"/> doesn't
        /// conform to the standard boolean values: "on", "off", "true", or "false"
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

        /// <summary>
        /// Retrieves the decimal value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="HoconValue"/>.</returns>
        public decimal GetDecimal()
        {
            var value = GetString();
            switch (value)
            {
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    throw new HoconException($"Could not convert `{value}` to decimal. Decimal does not support Infinity and NaN");
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

        /// <summary>
        /// Retrieves the float value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The float value represented by this <see cref="HoconValue"/>.</returns>
        public float GetFloat()
        {
            var value = GetString();
            switch (value)
            {
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

        /// <summary>
        /// Retrieves the double value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue"/>.</returns>
        public double GetDouble()
        {
            var value = GetString();
            switch (value)
            {
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

        /// <summary>
        /// Retrieves the long value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue"/>.</returns>
        public long GetLong()
        {
            var value = GetString();
            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToInt64(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to long.", e);
                }
            }

            if (value.StartsWith("0"))
            {
                try
                {
                    return Convert.ToInt64(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{GetString()}` to long.", e);
                }
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

        /// <summary>
        /// Retrieves the integer value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="HoconValue"/>.</returns>
        public int GetInt()
        {
            var value = GetString();
            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToInt32(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to long.", e);
                }
            }

            if (value.StartsWith("0"))
            {
                try
                {
                    return Convert.ToInt32(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{GetString()}` to long.", e);
                }
            }

            try
            {
                return int.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{GetString()}` to long.", e);
            }
        }

        /// <summary>
        /// Retrieves the byte value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="HoconValue"/>.</returns>
        public byte GetByte()
        {
            var value = GetString();
            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToByte(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to long.", e);
                }
            }

            if (value.StartsWith("0"))
            {
                try
                {
                    return Convert.ToByte(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{GetString()}` to long.", e);
                }
            }

            try
            {
                return byte.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{GetString()}` to long.", e);
            }
        }

        /// <summary>
        /// Retrieves a list of byte values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of byte values represented by this <see cref="HoconValue"/>.</returns>
        public IList<byte> GetByteList()
        {
            return GetArray().Select(v => v.GetByte()).ToList();
        }

        /// <summary>
        /// Retrieves a list of integer values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of integer values represented by this <see cref="HoconValue"/>.</returns>
        public IList<int> GetIntList()
        {
            return GetArray().Select(v => v.GetInt()).ToList();
        }

        /// <summary>
        /// Retrieves a list of long values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of long values represented by this <see cref="HoconValue"/>.</returns>
        public IList<long> GetLongList()
        {
            return GetArray().Select(v => v.GetLong()).ToList();
        }

        /// <summary>
        /// Retrieves a list of boolean values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconValue"/>.</returns>
        public IList<bool> GetBooleanList()
        {
            return GetArray().Select(v => v.GetBoolean()).ToList();
        }

        /// <summary>
        /// Retrieves a list of float values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of float values represented by this <see cref="HoconValue"/>.</returns>
        public IList<float> GetFloatList()
        {
            return GetArray().Select(v => v.GetFloat()).ToList();
        }

        /// <summary>
        /// Retrieves a list of double values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconValue"/>.</returns>
        public IList<double> GetDoubleList()
        {
            return GetArray().Select(v => v.GetDouble()).ToList();
        }

        /// <summary>
        /// Retrieves a list of decimal values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of decimal values represented by this <see cref="HoconValue"/>.</returns>
        public IList<decimal> GetDecimalList()
        {
            return GetArray().Select(v => v.GetDecimal()).ToList();
        }

        /// <summary>
        /// Retrieves a list of string values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconValue"/>.</returns>
        public IList<string> GetStringList()
        {
            return GetArray().Select(v => v.GetString()).ToList();
        }

        [Obsolete("Use GetTimeSpan instead")]
        public TimeSpan GetMillisDuration(bool allowInfinite = true)
        {
            return GetTimeSpan(allowInfinite);
        }

        /// <summary>
        /// Retrieves the time span value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <param name="allowInfinite">A flag used to set inifinite durations.</param>
        /// <returns>The time span value represented by this <see cref="HoconValue"/>.</returns>
        public TimeSpan GetTimeSpan(bool allowInfinite = true)
        {
            string res = GetString();
            //TODO: Add support for ns, us, and non abbreviated versions (second, seconds and so on) see https://github.com/typesafehub/config/blob/master/HOCON.md#duration-format
            if (res.EndsWith("ms"))
            {
                var v = res.Substring(0, res.Length - 2);
                return TimeSpan.FromMilliseconds(ParsePositiveValue(v));
            }
            if (res.EndsWith("s"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromSeconds(ParsePositiveValue(v));
            }
            if (res.EndsWith("m"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromMinutes(ParsePositiveValue(v));
            }
            if (res.EndsWith("h"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromHours(ParsePositiveValue(v));
            }
            if (res.EndsWith("d"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromDays(ParsePositiveValue(v));
            }
            if (allowInfinite && res.Equals("infinite", StringComparison.OrdinalIgnoreCase))  //Not in Hocon spec
            {
                return Timeout.InfiniteTimeSpan;
            }

            return TimeSpan.FromMilliseconds(ParsePositiveValue(res));
        }

        private static double ParsePositiveValue(string v)
        {
            var value = double.Parse(v, NumberFormatInfo.InvariantInfo);
            if (value < 0)
                throw new FormatException("Expected a positive value instead of " + value);
            return value;
        }

        /// <summary>
        /// Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue"/>.</returns>
        public long? GetByteSize()
        {
            var res = GetString();
            if (res.EndsWith("b"))
            {
                var v = res.Substring(0, res.Length - 1);
                return long.Parse(v);
            }

            return long.Parse(res);
        }

        #endregion

        internal void ResolveValue(IHoconElement child)
        {
            if (child.Type == HoconType.Empty)
            {
                Remove(child);
            }
            else if (Type == HoconType.Empty)
            {
                Type = child.Type;
            }
            else if (Type != child.Type)
            {
                var sub = (HoconSubstitution) child;
                throw HoconParserException.Create(sub, sub.Path, 
                    "Invalid substitution, substituted type must match its sibling type. " +
                    $"Sibling type:{Type}, substitution type:{child.Type}");
            }

            ((HoconField) Parent).ResolveValue(this);
        }

        /// <summary>
        /// Returns a HOCON string representation of this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A HOCON string representation of this <see cref="HoconValue"/>.</returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        /// <inheritdoc />
        public virtual string ToString(int indent, int indentSize)
        {
            switch (Type)
            {
                case HoconType.Literal:
                    return ConcatRawString();
                case HoconType.Object:
                    return $"{{{Environment.NewLine}{GetObject().ToString(indent, indentSize)}{Environment.NewLine}{new string(' ', (indent - 1) * indentSize)}}}";
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
            foreach (var value in this)
            {
                clone.Add(value.Clone(clone));
            }
            return clone;
        }
    }
}

