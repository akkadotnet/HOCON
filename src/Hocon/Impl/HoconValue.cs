//-----------------------------------------------------------------------
// <copyright file="HoconValue.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Hocon.Extensions;

namespace Hocon
{
    /// <summary>
    /// This class represents the root type for a HOCON (Human-Optimized Config Object Notation)
    /// configuration object.
    /// </summary>
    public class HoconValue : List<IHoconElement>, IHoconElement 
    {
        private static readonly Regex TimeSpanRegex = new Regex(@"^(?<value>([0-9]+(\.[0-9]+)?))\s*(?<unit>(nanoseconds|nanosecond|nanos|nano|ns|microseconds|microsecond|micros|micro|us|milliseconds|millisecond|millis|milli|ms|seconds|second|s|minutes|minute|m|hours|hour|h|days|day|d))$", RegexOptions.Compiled);

        public IHoconElement Parent { get; }

        public virtual HoconType Type { get; private set; } = HoconType.Empty;

        public ReadOnlyCollection<IHoconElement> Children => AsReadOnly();

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconValue"/> class.
        /// </summary>
        public HoconValue(IHoconElement parent)
        {
            if(parent != null && !(parent is HoconField) && !(parent is HoconArray))
                throw new HoconException("HoconValue parent must be HoconField, HoconArray, or null");
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
            if (Type == HoconType.Empty)
                Type = value.Type;
            else
            {
                if(!value.IsSubstitution() && Type != value.Type)
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
            if (array.All(value => value == null))
                return null;

            var sb = new StringBuilder();
            foreach (var s in array)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }

        private string ConcatRawString()
        {
            var array = this.Select(l => l.Raw).ToArray();
            if (array.All(value => value == null))
                return null;

            var sb = new StringBuilder();
            foreach (var s in array)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retrieves a list of values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of values represented by this <see cref="HoconValue"/>.</returns>
        public virtual List<HoconValue> GetArray()
        {
            switch (Type)
            {
                case HoconType.Array:
                    var result = new List<HoconValue>();
                    foreach (var element in this)
                    {
                        if(element.Type == HoconType.Array)
                            result.AddRange(element.GetArray());
                    }

                    return result;
                /*
                IEnumerable<HoconValue> x = from value in this
                    where value.Type == HoconType.Array
                    from e in value.GetArray()
                    select e;

                return x.ToList();
                */

                case HoconType.Object:
                    return GetObject().GetArray();

                case HoconType.Literal:
                    return null;
                    // TODO: this should throw according to spec
                    // throw new HoconException("Hocon literal could not be converted to array.");

                case HoconType.Empty:
                    throw new HoconException("HoconValue is empty.");

                default:
                    throw new Exception($"Unknown HoconType: {Type}");
            }
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
                case "+Infinity":
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

        /// <summary>
        /// Retrieves the double value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue"/>.</returns>
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
            if(string.IsNullOrEmpty(value))
                throw new HoconException($"Could not convert a {Type} into int.");

            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToInt32(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{GetString()}` to int.", e);
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
                    throw new HoconException($"Could not convert octal value `{GetString()}` to int.", e);
                }
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
                    throw new HoconException($"Could not convert hex value `{GetString()}` to byte.", e);
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
                    throw new HoconException($"Could not convert octal value `{GetString()}` to byte.", e);
                }
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
            // TODO: this should throw, remove the ?
            return GetArray()?.Select(v => v.GetString()).ToList();
        }

        /// <summary>
        /// Retrieves a list of objects from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconValue"/>.</returns>
        public IList<HoconObject> GetObjectList()
        {
            return GetArray().Select(v => v.GetObject()).ToList();
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

            var match = TimeSpanRegex.Match(res);
            if (match.Success)
            {
                var u = match.Groups["unit"].Value;
                var v = ParsePositiveValue(match.Groups["value"].Value);

                switch (u)
                {
                    case "nanoseconds":
                    case "nanosecond":
                    case "nanos":
                    case "nano":
                    case "ns":
                        return TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * v / 1000000.0));
                    case "microseconds":
                    case "microsecond":
                    case "micros":
                    case "micro":
                        return TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * v / 1000.0));
                    case "milliseconds":
                    case "millisecond":
                    case "millis":
                    case "milli":
                    case "ms":
                        return TimeSpan.FromMilliseconds(v);
                    case "seconds":
                    case "second":
                    case "s":
                        return TimeSpan.FromSeconds(v);
                    case "minutes":
                    case "minute":
                    case "m":
                        return TimeSpan.FromMinutes(v);
                    case "hours":
                    case "hour":
                    case "h":
                        return TimeSpan.FromHours(v);
                    case "days":
                    case "day":
                    case "d":
                        return TimeSpan.FromDays(v);
                }
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

        private struct ByteSize
        {
            public long Factor { get; set; }
            public string[] Suffixes { get; set; }
        }

        private static ByteSize[] ByteSizes { get; } =
            {
                new ByteSize { Factor = 1000L * 1000L * 1000L * 1000L * 1000L * 1000L, Suffixes = new[] { "EB", "exabyte", "exabytes" } },
                new ByteSize { Factor = 1024L * 1024L * 1024L * 1024L * 1024L * 1024L, Suffixes = new[] { "E", "e", "Ei", "EiB", "exbibyte", "exbibytes" } },
                new ByteSize { Factor = 1000L * 1000L * 1000L * 1000L * 1000L * 1000L, Suffixes = new[] { "EB", "exabyte", "exabytes" } },
                new ByteSize { Factor = 1024L * 1024L * 1024L * 1024L * 1024L, Suffixes = new[] { "P", "p", "Pi", "PiB", "pebibyte", "pebibytes" } },
                new ByteSize { Factor = 1000L * 1000L * 1000L * 1000L * 1000L, Suffixes = new[] { "PB", "petabyte", "petabytes" } },
                new ByteSize { Factor = 1024L * 1024L * 1024L * 1024L, Suffixes = new[] { "T", "t", "Ti", "TiB", "tebibyte", "tebibytes" } },
                new ByteSize { Factor = 1000L * 1000L * 1000L * 1000L, Suffixes = new[] { "TB", "terabyte", "terabytes" } },
                new ByteSize { Factor = 1024L * 1024L * 1024L, Suffixes = new[] { "G", "g", "Gi", "GiB", "gibibyte", "gibibytes" } },
                new ByteSize { Factor = 1000L * 1000L * 1000L, Suffixes = new[] { "GB", "gigabyte", "gigabytes" } },
                new ByteSize { Factor = 1024L * 1024L, Suffixes = new[] { "M", "m", "Mi", "MiB", "mebibyte", "mebibytes" } },
                new ByteSize { Factor = 1000L * 1000L, Suffixes = new[] { "MB", "megabyte", "megabytes" } },
                new ByteSize { Factor = 1024L, Suffixes = new[] { "K", "k", "Ki", "KiB", "kibibyte", "kibibytes" } },
                new ByteSize { Factor = 1000L, Suffixes = new[] { "kB", "kilobyte", "kilobytes" } },
                new ByteSize { Factor = 1, Suffixes = new[] { "b", "B", "byte", "bytes" } }
            };

        private static char[] Digits { get; } = "0123456789".ToCharArray();

        /// <summary>
        /// Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue"/>.</returns>
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
            {
                foreach (var suffix in byteSize.Suffixes)
                {
                    if (string.Equals(unit, suffix, StringComparison.Ordinal))
                        return (long)(byteSize.Factor * double.Parse(value));
                }
            }

            throw new FormatException($"{unit} is not a valid byte size suffix");
        }

        #endregion

        internal void ResolveValue(HoconSubstitution child)
        {
            if (child.Type == HoconType.Empty)
            {
                Remove(child);
            }
            else
            {
                if (Type == HoconType.Empty)
                {
                    Type = child.Type;
                }
                else if (Type != child.Type)
                {
                    throw HoconParserException.Create(child, child.Path,
                        "Invalid substitution, substituted type must match its sibling type. " +
                        $"Sibling type:{Type}, substitution type:{child.Type}");
                }
            }

            switch (Parent)
            {
                case HoconField v:
                    v.ResolveValue(this);
                    break;
                case HoconArray a:
                    a.ResolveValue(child);
                    break;
                default:
                    throw new Exception($"Invalid parent type while resolving substitution:{Parent.GetType()}");
            }
        }

        internal void ReParent(HoconValue value)
        {
            foreach (var element in value)
            {
                Add(element.Clone(this));
            }
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
            foreach (var element in this)
            {
                clone.Add(element.CloneValue(clone));
            }
            return clone;
        }

        protected bool Equals(HoconValue other)
        {
            return Type == other.Type && GetString() == other.GetString();
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
                case HoconType.Literal:
                    return string.Equals(GetString(), other.GetString());
                case HoconType.Object:
                    return GetObject().AsEnumerable().SequenceEqual(other.GetObject().AsEnumerable());
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
                return this.Aggregate(seed, (current, item) => (current * modifier) + item.GetHashCode());
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
    }
}

