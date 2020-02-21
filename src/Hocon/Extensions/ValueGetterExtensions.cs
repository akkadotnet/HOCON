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
            result = default;
            if (element is HoconLiteral lit)
            {
                result = lit;
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

        /// <summary>
        ///     Wraps any exception into <see cref="HoconValueException" /> with failure path specified
        /// </summary>
        private static T WrapWithValueException<T>(string path, Func<T> func)
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

        private static T WrapWithValueException<T>(HoconPath path, Func<T> func)
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

        #region Object value getter
        public static HoconElement GetValue(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
       }


        public static string GetString(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static char GetChar(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static bool GetBoolean(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static decimal GetDecimal(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static float GetFloat(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static double GetDouble(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static BigInteger GetBigInteger(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static ulong GetULong(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static long GetLong(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static uint GetUInt(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static int GetInt(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static ushort GetUShort(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static short GetShort(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static sbyte GetSByte(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static byte GetByte(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path]);
        }

        public static TimeSpan GetTimeSpan(this HoconElement element, string path, bool allowInfinite)
        {
            return WrapWithValueException(path, () => element[path].GetTimeSpan(allowInfinite));
        }

        public static long? GetByteSize(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");
            return obj[path];
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