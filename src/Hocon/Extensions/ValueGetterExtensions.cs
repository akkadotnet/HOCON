// -----------------------------------------------------------------------
// <copyright file="ValueGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

namespace Hocon
{
    public static class ValueGetterExtensions
    {
        #region Value Getter
        public static string GetString(this HoconElement element)
        {
            if (!(element is HoconLiteral lit))
                throw new HoconException(
                    $"Value getter can only work on {nameof(HoconLiteral)} type. {element.GetType()} found instead.");
            return lit.Value;
        }

        public static char GetChar(this HoconElement element)
        {
            if (!(element is HoconLiteral lit))
                throw new HoconException(
                    $"Value getter functions can only work on {nameof(HoconLiteral)} type. {element.GetType()} found instead.");
            return lit.Value[0];
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
        public static bool GetBoolean(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the decimal value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="HoconValue" />.</returns>
        public static decimal GetDecimal(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the float value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The float value represented by this <see cref="HoconValue" />.</returns>
        public static float GetFloat(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the double value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue" />.</returns>
        public static double GetDouble(this HoconElement element)
        {
            return element;
        }

        public static BigInteger GetBigInteger(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned long value represented by this <see cref="HoconValue" />.</returns>
        public static ulong GetULong(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public static long GetLong(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned integer value represented by this <see cref="HoconValue" />.</returns>
        public static uint GetUInt(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="HoconValue" />.</returns>
        public static int GetInt(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned short value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned short value represented by this <see cref="HoconValue" />.</returns>
        public static ushort GetUShort(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the short value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The short value represented by this <see cref="HoconValue" />.</returns>
        public static short GetShort(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the signed byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The signed byte value represented by this <see cref="HoconValue" />.</returns>
        public static sbyte GetSByte(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="HoconValue" />.</returns>
        public static byte GetByte(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the time span value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The time span value represented by this <see cref="HoconValue" />.</returns>
        public static TimeSpan GetTimeSpan(this HoconElement element, bool allowInfinite = true)
        {
            string res = element;

            if (res.TryMatchTimeSpan(out var result))
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

        public static TimeSpan GetTimeSpan(this HoconElement element, TimeSpan? @default, bool allowInfinite = true)
        {
            if (element.TryGetTimeSpan(out var result, allowInfinite))
                return result;
            return @default.GetValueOrDefault();
        }

        /// <summary>
        ///     Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public static long? GetByteSize(this HoconElement element)
        {
            if (!(element is HoconLiteral lit))
                throw new HoconException(
                    $"Value getter functions can only work on {nameof(HoconLiteral)} type. {element.GetType()} found instead.");

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
                    return (long) (byteSize.Factor * double.Parse(value));

            throw new FormatException($"{unit} is not a valid byte size suffix");
        }

        public static HoconObject GetObject(this HoconElement element)
        {
            return element.ToObject();
        }

        public static HoconObject GetObject(this HoconElement element, HoconObject @default)
        {
            if (element.TryGetObject(out var result))
                return result;
            return @default;
        }
        #endregion

        #region TryGet value getter
        public static bool TryGetBoolean(this HoconElement element, out bool result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            switch (lit.Value)
            {
                case null:
                    return false;
                case "on":
                case "true":
                case "yes":
                    result = true;
                    return true;
                case "off":
                case "false":
                case "no":
                    result = false;
                    return true;
                default:
                    return false;
            }

        }

        public static bool TryGetSByte(this HoconElement element, out sbyte result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToSByte(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToSByte(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return sbyte.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetByte(this HoconElement element, out byte result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
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

        public static bool TryGetShort(this HoconElement element, out short result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToInt16(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToInt16(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return short.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetUShort(this HoconElement element, out ushort result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToUInt16(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToUInt16(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return ushort.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetInt(this HoconElement element, out int result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
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

        public static bool TryGetUInt(this HoconElement element, out uint result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToUInt32(value, 16);
                    return true;
                }
                catch
                {
                    return false;
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToUInt32(value, 8);
                    return true;
                }
                catch
                {
                    return false;
                }

            return uint.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetLong(this HoconElement element, out long result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToInt64(value, 16);
                    return true;
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to long.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToInt64(value, 8);
                    return true;
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to long.", e);
                }

            return long.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetULong(this HoconElement element, out ulong result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                try
                {
                    result = Convert.ToUInt64(value, 16);
                    return true;
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert hex value `{value}` to ulong.", e);
                }

            if (value.StartsWith("0"))
                try
                {
                    result = Convert.ToUInt64(value, 8);
                    return true;
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to ulong.", e);
                }

            return ulong.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetBigInteger(this HoconElement element, out BigInteger result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
            if (value.StartsWith("0x"))
                return BigInteger.TryParse(value.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out result);

            if (value.StartsWith("0"))
                try
                {
                    result = value.Substring(1).Aggregate(new BigInteger(), (b, c) => b * 8 + c - '0');
                    return true;
                }
                catch (Exception e)
                {
                    throw new HoconException($"Could not convert octal value `{value}` to ulong.", e);
                }

            return BigInteger.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        public static bool TryGetFloat(this HoconElement element, out float result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
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

        public static bool TryGetDouble(this HoconElement element, out double result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
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

        public static bool TryGetDecimal(this HoconElement element, out decimal result)
        {
            result = default;
            if (!(element is HoconLiteral lit))
                return false;

            var value = lit.Value;
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

        public static bool TryGetTimeSpan(this HoconElement element, out TimeSpan result, bool allowInfinite = true)
        {
            result = default;

            if (!(element is HoconLiteral lit))
                return false;

            var res = lit.Value;

            if (res.TryMatchTimeSpan(out result))
                return true;

            if (allowInfinite && res.Equals("infinite", StringComparison.OrdinalIgnoreCase)) //Not in Hocon spec
            {
                result = Timeout.InfiniteTimeSpan;
                return true;
            }

            if (!double.TryParse(res, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out var value))
                return false;

            if (value < 0)
                return false;

            result = TimeSpan.FromMilliseconds(value);
            return true;
        }

        public static bool TryGetString(this HoconElement element, out string result)
        {
            if (element is HoconLiteral lit)
            {
                result = lit;
                return true;
            }
            result = default;
            return false;
        }

        public static bool TryGetChar(this HoconElement element, out char result)
        {
            if (element is HoconLiteral lit)
            {
                result = lit;
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryGetByteSize(this HoconElement element, out long? result)
        {
            result = null;
            if (!element.TryGetString(out var res))
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

        public static bool TryGetObject(this HoconElement element, out HoconObject result)
        {
            result = default;
            if (element is HoconObject obj)
            {
                result = obj;
                return true;
            }
            return false;
        }
        #endregion

        #region Object value getter

        #region HoconElement
        public static HoconElement GetValue(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static HoconElement GetValue(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetValue(this HoconElement element, string path, out HoconElement result)
        {
            result = default;
            if (element is HoconObject obj)
                return obj.TryGetValue(path, out result);

            return false;
        }

        public static bool TryGetValue(this HoconElement element, HoconPath path, out HoconElement result)
        {
            result = default;
            if (element is HoconObject obj)
                return obj.TryGetValue(path, out result);

            return false;
        }
        #endregion

        #region HoconObject
        public static HoconObject GetObject(this HoconElement element, string path, HoconObject @default)
        {
            if (element.TryGetObject(path, out var result))
                return result;
            return @default;
        }

        public static HoconObject GetObject(this HoconElement element, HoconPath path, HoconObject @default)
        {
            if (element.TryGetObject(path, out var result))
                return result;
            return @default;
        }

        public static HoconObject GetObject(this HoconElement element, string path)
        {
            try
            {
                return element[path].ToObject();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static HoconObject GetObject(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path].ToObject();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetObject(this HoconElement element, string path, out HoconObject result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetObject(out result);

            result = default;
            return false;
        }

        public static bool TryGetObject(this HoconElement element, HoconPath path, out HoconObject result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetObject(out result);

            result = default;
            return false;
        }
        #endregion

        #region String
        public static string GetString(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static string GetString(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetString(this HoconElement element, string path, out string result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetString(out result);

            result = default;
            return false;
        }

        public static bool TryGetString(this HoconElement element, HoconPath path, out string result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetString(out result);

            result = default;
            return false;
        }

        public static string GetString(this HoconElement element, string path, string @default)
        {
            if (element.TryGetString(path, out var result))
                return result;
            return @default;
        }

        public static string GetString(this HoconElement element, HoconPath path, string @default)
        {
            if (element.TryGetString(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Char
        public static char GetChar(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static char GetChar(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetChar(this HoconElement element, string path, out char result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetChar(out result);

            result = default;
            return false;
        }

        public static bool TryGetChar(this HoconElement element, HoconPath path, out char result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetChar(out result);

            result = default;
            return false;
        }

        public static char GetChar(this HoconElement element, string path, char @default)
        {
            if (element.TryGetChar(path, out var result))
                return result;

            return @default;
        }

        public static char GetChar(this HoconElement element, HoconPath path, char @default)
        {
            if (element.TryGetChar(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Boolean
        public static bool GetBoolean(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool GetBoolean(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetBoolean(this HoconElement element, string path, out bool result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetBoolean(out result);

            result = default;
            return false;
        }

        public static bool TryGetBoolean(this HoconElement element, HoconPath path, out bool result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetBoolean(out result);

            result = default;
            return false;
        }

        public static bool GetBoolean(this HoconElement element, string path, bool @default)
        {
            if (element.TryGetBoolean(path, out var result))
                return result;

            return @default;
        }

        public static bool GetBoolean(this HoconElement element, HoconPath path, bool @default)
        {
            if (element.TryGetBoolean(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Decimal
        public static decimal GetDecimal(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static decimal GetDecimal(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetDecimal(this HoconElement element, string path, out decimal result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetDecimal(out result);

            result = default;
            return false;
        }

        public static bool TryGetDecimal(this HoconElement element, HoconPath path, out decimal result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetDecimal(out result);

            result = default;
            return false;
        }

        public static decimal GetDecimal(this HoconElement element, string path, decimal @default)
        {
            if (element.TryGetDecimal(path, out var result))
                return result;

            return @default;
        }

        public static decimal GetDecimal(this HoconElement element, HoconPath path, decimal @default)
        {
            if (element.TryGetDecimal(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Float
        public static float GetFloat(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static float GetFloat(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetFloat(this HoconElement element, string path, out float result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetFloat(out result);

            result = default;
            return false;
        }

        public static bool TryGetFloat(this HoconElement element, HoconPath path, out float result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetFloat(out result);

            result = default;
            return false;
        }

        public static float GetFloat(this HoconElement element, string path, float @default)
        {
            if (element.TryGetFloat(path, out var result))
                return result;

            return @default;
        }

        public static float GetFloat(this HoconElement element, HoconPath path, float @default)
        {
            if (element.TryGetFloat(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Double
        public static double GetDouble(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static double GetDouble(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetDouble(this HoconElement element, string path, out double result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetDouble(out result);

            result = default;
            return false;
        }

        public static bool TryGetDouble(this HoconElement element, HoconPath path, out double result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetDouble(out result);

            result = default;
            return false;
        }

        public static double GetDouble(this HoconElement element, string path, double @default)
        {
            if (element.TryGetDouble(path, out var result))
                return result;

            return @default;
        }

        public static double GetDouble(this HoconElement element, HoconPath path, double @default)
        {
            if (element.TryGetDouble(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region BigInteger
        public static BigInteger GetBigInteger(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static BigInteger GetBigInteger(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetBigInteger(this HoconElement element, string path, out BigInteger result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetBigInteger(out result);

            result = default;
            return false;
        }

        public static bool TryGetBigInteger(this HoconElement element, HoconPath path, out BigInteger result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetBigInteger(out result);

            result = default;
            return false;
        }

        public static BigInteger GetBigInteger(this HoconElement element, string path, BigInteger @default)
        {
            if (element.TryGetBigInteger(path, out var result))
                return result;

            return @default;
        }

        public static BigInteger GetBigInteger(this HoconElement element, HoconPath path, BigInteger @default)
        {
            if (element.TryGetBigInteger(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region ULong
        public static ulong GetULong(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static ulong GetULong(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetULong(this HoconElement element, string path, out ulong result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetULong(out result);

            result = default;
            return false;
        }

        public static bool TryGetULong(this HoconElement element, HoconPath path, out ulong result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetULong(out result);

            result = default;
            return false;
        }

        public static ulong GetULong(this HoconElement element, string path, ulong @default)
        {
            if (element.TryGetULong(path, out var result))
                return result;

            return @default;
        }

        public static ulong GetULong(this HoconElement element, HoconPath path, ulong @default)
        {
            if (element.TryGetULong(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Long
        public static long GetLong(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static long GetLong(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetLong(this HoconElement element, string path, out long result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetLong(out result);

            result = default;
            return false;
        }

        public static bool TryGetLong(this HoconElement element, HoconPath path, out long result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetLong(out result);

            result = default;
            return false;
        }

        public static long GetLong(this HoconElement element, string path, long @default)
        {
            if (element.TryGetLong(path, out var result))
                return result;

            return @default;
        }

        public static long GetLong(this HoconElement element, HoconPath path, long @default)
        {
            if (element.TryGetLong(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region UInt
        public static uint GetUInt(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static uint GetUInt(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetUInt(this HoconElement element, string path, out uint result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetUInt(out result);

            result = default;
            return false;
        }

        public static bool TryGetUInt(this HoconElement element, HoconPath path, out uint result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetUInt(out result);

            result = default;
            return false;
        }

        public static uint GetUInt(this HoconElement element, string path, uint @default)
        {
            if (element.TryGetUInt(path, out var result))
                return result;

            return @default;
        }

        public static uint GetUInt(this HoconElement element, HoconPath path, uint @default)
        {
            if (element.TryGetUInt(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region GetInt
        public static int GetInt(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static int GetInt(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetInt(this HoconElement element, string path, out int result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetInt(out result);

            result = default;
            return false;
        }

        public static bool TryGetInt(this HoconElement element, HoconPath path, out int result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetInt(out result);

            result = default;
            return false;
        }

        public static int GetInt(this HoconElement element, string path, int @default)
        {
            if (element.TryGetInt(path, out var result))
                return result;

            return @default;
        }

        public static int GetInt(this HoconElement element, HoconPath path, int @default)
        {
            if (element.TryGetInt(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region UShort
        public static ushort GetUShort(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static ushort GetUShort(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetUShort(this HoconElement element, string path, out ushort result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetUShort(out result);

            result = default;
            return false;
        }

        public static bool TryGetUShort(this HoconElement element, HoconPath path, out ushort result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetUShort(out result);

            result = default;
            return false;
        }

        public static ushort GetUShort(this HoconElement element, string path, ushort @default)
        {
            if (element.TryGetUShort(path, out var result))
                return result;

            return @default;
        }

        public static ushort GetUShort(this HoconElement element, HoconPath path, ushort @default)
        {
            if (element.TryGetUShort(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Short
        public static short GetShort(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static short GetShort(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetShort(this HoconElement element, string path, out short result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetShort(out result);

            result = default;
            return false;
        }

        public static bool TryGetShort(this HoconElement element, HoconPath path, out short result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetShort(out result);

            result = default;
            return false;
        }

        public static short GetShort(this HoconElement element, string path, short @default)
        {
            if (element.TryGetShort(path, out var result))
                return result;

            return @default;
        }

        public static short GetShort(this HoconElement element, HoconPath path, short @default)
        {
            if (element.TryGetShort(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region SByte
        public static sbyte GetSByte(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static sbyte GetSByte(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetSByte(this HoconElement element, string path, out sbyte result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetSByte(out result);

            result = default;
            return false;
        }

        public static bool TryGetSByte(this HoconElement element, HoconPath path, out sbyte result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetSByte(out result);

            result = default;
            return false;
        }

        public static sbyte GetSByte(this HoconElement element, string path, sbyte @default)
        {
            if (element.TryGetSByte(path, out var result))
                return result;

            return @default;
        }

        public static sbyte GetSByte(this HoconElement element, HoconPath path, sbyte @default)
        {
            if (element.TryGetSByte(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Byte
        public static byte GetByte(this HoconElement element, string path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static byte GetByte(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path];
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetByte(this HoconElement element, string path, out byte result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetByte(out result);

            result = default;
            return false;
        }

        public static bool TryGetByte(this HoconElement element, HoconPath path, out byte result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetByte(out result);

            result = default;
            return false;
        }

        public static byte GetByte(this HoconElement element, string path, byte @default)
        {
            if (element.TryGetByte(path, out var result))
                return result;

            return @default;
        }

        public static byte GetByte(this HoconElement element, HoconPath path, byte @default)
        {
            if (element.TryGetByte(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region TimeSpan
        public static TimeSpan GetTimeSpan(this HoconElement element, string path, bool allowInfinite = true)
        {
            try
            {
                return element[path].GetTimeSpan(allowInfinite);
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static TimeSpan GetTimeSpan(this HoconElement element, HoconPath path, bool allowInfinite = true)
        {
            try
            {
                return element[path].GetTimeSpan(allowInfinite);
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static TimeSpan GetTimeSpan(this HoconElement element, string path, TimeSpan? @default, bool allowInfinite = true)
        {
            if (element.TryGetTimeSpan(path, out var result, allowInfinite))
                return result;
            return @default.GetValueOrDefault();
        }

        public static TimeSpan GetTimeSpan(this HoconElement element, HoconPath path, TimeSpan? @default, bool allowInfinite = true)
        {
            if (element.TryGetTimeSpan(path, out var result, allowInfinite))
                return result;
            return @default.GetValueOrDefault();
        }

        public static bool TryGetTimeSpan(this HoconElement element, string path, out TimeSpan result, bool allowInfinite = true)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetTimeSpan(out result, allowInfinite);

            result = default;
            return false;
        }

        public static bool TryGetTimeSpan(this HoconElement element, HoconPath path, out TimeSpan result, bool allowInfinite = true)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetTimeSpan(out result, allowInfinite);

            result = default;
            return false;
        }

        public static TimeSpan GetByte(this HoconElement element, string path, TimeSpan @default, bool allowInfinite = true)
        {
            if (element.TryGetTimeSpan(path, out var result, allowInfinite))
                return result;

            return @default;
        }

        public static TimeSpan GetByte(this HoconElement element, HoconPath path, TimeSpan @default, bool allowInfinite = true)
        {
            if (element.TryGetTimeSpan(path, out var result, allowInfinite))
                return result;

            return @default;
        }
        #endregion

        #region ByteSize
        public static long? GetByteSize(this HoconElement element, string path)
        {
            try
            {
                return element[path].GetByteSize();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static long? GetByteSize(this HoconElement element, HoconPath path)
        {
            try
            {
                return element[path].GetByteSize();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool TryGetByteSize(this HoconElement element, string path, out long? result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetByteSize(out result);

            result = default;
            return false;
        }

        public static bool TryGetByteSize(this HoconElement element, HoconPath path, out long? result)
        {
            if (element.TryGetValue(path, out var value))
                return value.TryGetByteSize(out result);

            result = default;
            return false;
        }

        public static long? GetByteSize(this HoconElement element, string path, long? @default)
        {
            if (element.TryGetByteSize(path, out var result))
                return result;

            return @default;
        }

        public static long? GetByteSize(this HoconElement element, HoconPath path, long? @default)
        {
            if (element.TryGetByteSize(path, out var result))
                return result;

            return @default;
        }
        #endregion
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