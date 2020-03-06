// -----------------------------------------------------------------------
// <copyright file="HoconImmutableLiteral.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace Hocon
{
    public sealed class HoconLiteral : HoconElement
    {
        public static readonly HoconLiteral Null = new HoconLiteral(null, HoconLiteralType.Null);

        private HoconLiteral(string value)
        {
            _value = value;
            LiteralType =
                value.NeedTripleQuotes() ? HoconLiteralType.TripleQuotedString :
                value.NeedQuotes() ? HoconLiteralType.QuotedString : HoconLiteralType.UnquotedString;
        }

        private HoconLiteral(string value, HoconLiteralType literalType)
        {
            _value = value;
            LiteralType = literalType;
        }

        public override HoconType Type => HoconType.String;
        public HoconLiteralType LiteralType { get; }

        private string _value;
        public new string Value
        {
            get
            {
                if (LiteralType == HoconLiteralType.Null)
                    return null;
                return _value;
            }
        }

        public override string Raw => Value == null ? "null" :
                LiteralType == HoconLiteralType.TripleQuotedString ? Value.AddTripleQuotes() :
                LiteralType == HoconLiteralType.QuotedString ? Value.AddQuotes() : Value;

        public static HoconLiteral Create(string value)
        {
            if (value == null)
                return Null;

            return new HoconLiteral(value);
        }

        internal static HoconLiteral Create(string value, HoconLiteralType literalType)
        {
            if (literalType == HoconLiteralType.Null)
                return Null;

            return new HoconLiteral(value, literalType);
        }

        #region Interface implementations

        public override string ToString()
        {
            return Value;
        }

        public override string ToString(int indent, int indentSize)
        {
            return Raw;
        }

        public override bool Equals(HoconElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is HoconLiteral otherLiteral)) return false;

            return otherLiteral.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        #endregion

        #region Value getters

        /// <summary>
        ///     Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public override long? GetByteSize()
        {
            if (!(this is HoconLiteral lit))
                throw new HoconException(
                    $"Value getter functions can only work on {nameof(HoconLiteral)} type. {GetType()} found instead.");

            var res = lit.Value;
            if (string.IsNullOrEmpty(res))
                return null;
            res = res.Trim();
            var index = res.LastIndexOfAny(Digits);
            if (index == -1 || index + 1 >= res.Length)
                return long.Parse(res);

            var value = res.Substring(0, index + 1);
            var unit = res.Substring(index + 1).Trim();

            foreach (var byteSize in ByteSizes)
                if (byteSize.Suffixes.Any(suffix => string.Equals(unit, suffix, StringComparison.Ordinal)))
                    return (long)(byteSize.Factor * double.Parse(value));

            throw new FormatException($"{unit} is not a valid byte size suffix");
        }

        public override char GetChar()
        {
            return Value?[0] ?? '\0';
        }

        //public static implicit operator char[](HoconLiteral lit)
        public override IList<char> GetCharList()
        {
            return Value?.ToCharArray() ?? new char[] { };
        }

        /// <summary>
        ///     Retrieves the boolean value from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>The boolean value represented by this <see cref="HoconElement" />.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     This exception occurs when the <see cref="HoconElement" /> is not a <see cref="HoconLiteral" />
        ///     and it doesn't
        ///     conform to the standard boolean values: "on", "off", "yes", "no", "true", or "false"
        /// </exception>
        public override bool GetBoolean()
        {
            switch (Value)
            {
                case "on":
                case "true":
                case "yes":
                    return true;
                case "off":
                case "false":
                case "no":
                case null:
                    return false;
                default:
                    throw new HoconException($"Unknown boolean format: {Value}");
            }
        }

        /// <summary>
        ///     Retrieves the signed byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The signed byte value represented by this <see cref="HoconValue" />.</returns>
        public override sbyte GetSByte()
        {
            if (Value.StartsWith("0x"))
                try
                {
                    return Convert.ToSByte(Value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{Value}` to sbyte.", e);
                }

            if (Value.StartsWith("0"))
                try
                {
                    return Convert.ToSByte(Value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{Value}` to sbyte.", e);
                }

            try
            {
                return sbyte.Parse(Value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{Value}` to sbyte.", e);
            }
        }

        /// <summary>
        ///     Retrieves the byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="HoconValue" />.</returns>
        public override byte GetByte()
        {
            if (Value.StartsWith("0x"))
                try
                {
                    return Convert.ToByte(Value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{Value}` to byte.", e);
                }

            if (Value.StartsWith("0"))
                try
                {
                    return Convert.ToByte(Value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{Value}` to byte.", e);
                }

            try
            {
                return byte.Parse(Value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{Value}` to byte.", e);
            }
        }

        /// <summary>
        ///     Retrieves the short value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The short value represented by this <see cref="HoconValue" />.</returns>
        public override short GetShort()
        {
            //var value = lit.Value;
            if (Value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt16(Value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{Value}` to short.", e);
                }

            if (Value.StartsWith("0"))
                try
                {
                    return Convert.ToInt16(Value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{Value}` to short.", e);
                }

            try
            {
                return short.Parse(Value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{Value}` to short.", e);
            }
        }

        /// <summary>
        ///     Retrieves the unsigned short value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned short value represented by this <see cref="HoconValue" />.</returns>
        public override ushort GetUShort()
        {
            //var value = lit.Value;
            if (Value.StartsWith("0x"))
                try
                {
                    return Convert.ToUInt16(Value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{Value}` to ushort.", e);
                }

            if (Value.StartsWith("0"))
                try
                {
                    return Convert.ToUInt16(Value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{Value}` to ushort.", e);
                }

            try
            {
                return ushort.Parse(Value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{Value}` to ushort.", e);
            }
        }

        /// <summary>
        ///     Retrieves the integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="HoconValue" />.</returns>
        public override int GetInt()
        {
            //var value = lit.Value;
            if (Value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt32(Value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{Value}` to int.", e);
                }

            if (Value.StartsWith("0"))
                try
                {
                    return Convert.ToInt32(Value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{Value}` to int.", e);
                }

            try
            {
                return int.Parse(Value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{Value}` to int.", e);
            }
        }

        /// <summary>
        ///     Retrieves the unsigned integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned integer value represented by this <see cref="HoconValue" />.</returns>
        public override uint GetUInt()
        {
            var value = Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToUInt32(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to uint.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToUInt32(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to uint.", e);
                }

            try
            {
                return uint.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to uint.", e);
            }
        }

        /// <summary>
        ///     Retrieves the long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public override long GetLong()
        {
            var value = Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt64(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to long.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToInt64(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to long.", e);
                }

            try
            {
                return long.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to long.", e);
            }
        }

        /// <summary>
        ///     Retrieves the unsigned long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned long value represented by this <see cref="HoconValue" />.</returns>
        public override ulong GetULong()
        {
            var value = Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToUInt64(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to ulong.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToUInt64(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to ulong.", e);
                }

            try
            {
                return ulong.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to ulong.", e);
            }
        }

        //public static implicit operator BigInteger(HoconLiteral lit)
        public override BigInteger GetBigInteger()
        {
            var value = Value;
            if (value.StartsWith("0x"))
                try
                {
                    return BigInteger.Parse(value.Substring(2), NumberStyles.HexNumber);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to ulong.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return value.Substring(1).Aggregate(new BigInteger(), (b, c) => b * 8 + c - '0');
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to ulong.", e);
                }

            try
            {
                return BigInteger.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to ulong.", e);
            }
        }

        /// <summary>
        ///     Retrieves the float value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The float value represented by this <see cref="HoconValue" />.</returns>
        public override float GetFloat()
        {
            var value = Value;
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
        ///     Retrieves the double value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue" />.</returns>
        public override double GetDouble()
        {
            var value = Value;
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
        ///     Retrieves the decimal value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="HoconValue" />.</returns>
        public override decimal GetDecimal()
        {
            var value = Value;
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

        public override string GetString()
        {
            return Value;
        }

        /// <summary>
        ///     Retrieves the time span value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The time span value represented by this <see cref="HoconValue" />.</returns>
        public override TimeSpan GetTimeSpan(bool allowInfinite = true)
        {
            if (Value.TryMatchTimeSpan(out var result))
                return result;

            if (allowInfinite && Value.Equals("infinite", StringComparison.OrdinalIgnoreCase)) //Not in Hocon spec
                return Timeout.InfiniteTimeSpan;

            double value;
            try
            {
                value = double.Parse(Value, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e)
            {
                throw new HoconException($"Failed to parse TimeSpan value from '{Value}'", e);
            }

            if (value < 0)
                throw new HoconException("Expected a positive value instead of " + value);

            return TimeSpan.FromMilliseconds(value);
        }

        #endregion

        #region Helpers
        private struct ByteSize
        {
            public long Factor { get; set; }
            public string[] Suffixes { get; set; }
        }

        // ReSharper disable StringLiteralTypo
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
        // ReSharper restore StringLiteralTypo

        private static char[] Digits { get; } = "0123456789".ToCharArray();

        #endregion
    }
}