// -----------------------------------------------------------------------
// <copyright file="HoconImmutableLiteral.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hocon
{
    [JsonConverter(typeof(HoconLiteral))]
    public sealed class HoconLiteral : HoconElement, IEquatable<HoconLiteral>
    {
        public static readonly HoconLiteral Null = Create(null);

        private HoconLiteral(string value)
        {
            Value = value;
        }

        public new string Value { get; }

        internal static HoconLiteral Create(string value)
        {
            return new HoconLiteral(value);
        }

        #region Interface implementations

        public override string ToString()
        {
            return Value;
        }

        public override string ToString(int indent, int indentSize)
        {
            return Value == null ? "null" :
                Value.NeedTripleQuotes() ? Value.AddTripleQuotes() : 
                Value.NeedQuotes() ? Value.AddQuotes() : Value;
        }

        public bool Equals(HoconLiteral other)
        {
            if (other == null) return false;
            return ReferenceEquals(this, other) || string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is HoconLiteral other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        #endregion

        #region Casting operators

        public static implicit operator char(HoconLiteral lit)
        {
            return lit.Value?[0] ?? '\0';
        }

        public static implicit operator char[](HoconLiteral lit)
        {
            return lit.Value?.ToCharArray() ?? new char[] { };
        }

        public static implicit operator string(HoconLiteral lit)
        {
            return lit.Value;
        }

        public static implicit operator bool(HoconLiteral lit)
        {
            switch (lit.Value)
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
                    throw new HoconException($"Unknown boolean format: {lit.Value}");
            }
        }

        public static implicit operator sbyte(HoconLiteral lit)
        {
            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToSByte(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to sbyte.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToSByte(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to sbyte.", e);
                }

            try
            {
                return sbyte.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to sbyte.", e);
            }
        }

        public static implicit operator byte(HoconLiteral lit)
        {
            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToByte(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to byte.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToByte(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to byte.", e);
                }

            try
            {
                return byte.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to byte.", e);
            }
        }

        public static implicit operator short(HoconLiteral lit)
        {
            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt16(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to short.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToInt16(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to short.", e);
                }

            try
            {
                return short.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to short.", e);
            }
        }

        public static implicit operator ushort(HoconLiteral lit)
        {
            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToUInt16(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to ushort.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToUInt16(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to ushort.", e);
                }

            try
            {
                return ushort.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to ushort.", e);
            }
        }

        public static implicit operator int(HoconLiteral lit)
        {
            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    return Convert.ToInt32(value, 16);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to int.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    return Convert.ToInt32(value, 8);
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to int.", e);
                }

            try
            {
                return int.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new HoconException($"Could not convert `{value}` to int.", e);
            }
        }

        public static implicit operator uint(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator long(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator ulong(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator BigInteger(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator float(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator double(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator decimal(HoconLiteral lit)
        {
            var value = lit.Value;
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

        public static implicit operator TimeSpan(HoconLiteral lit)
        {
            if (lit.Value.TryMatchTimeSpan(out var result))
                return result;

            double value;
            try
            {
                value = double.Parse(lit.Value, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e)
            {
                throw new HoconException($"Failed to parse TimeSpan value from '{lit.Value}'.", e);
            }

            if (value < 0)
                throw new HoconException($"Failed to parse TimeSpan value, expected a positive value instead of {value}.");

            return TimeSpan.FromMilliseconds(value);
        }

        #endregion
    }
}