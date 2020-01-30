﻿// -----------------------------------------------------------------------
// <copyright file="ValueGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Hocon
{
    public static class ValueGetterExtensions
    {
        public static HoconObject GetObject(this HoconElement element)
        {
            if(!(element is HoconObject obj))
                throw new HoconException(
                    $"GetObject() can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");
            return obj;
        }

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
        ///     Retrieves the decimal value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="InternalHoconValue" />.</returns>
        public static decimal GetDecimal(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the float value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The float value represented by this <see cref="InternalHoconValue" />.</returns>
        public static float GetFloat(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the double value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The double value represented by this <see cref="InternalHoconValue" />.</returns>
        public static double GetDouble(this HoconElement element)
        {
            return element;
        }

        public static BigInteger GetBigInteger(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned long value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The unsigned long value represented by this <see cref="InternalHoconValue" />.</returns>
        public static ulong GetULong(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the long value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="InternalHoconValue" />.</returns>
        public static long GetLong(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned integer value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The unsigned integer value represented by this <see cref="InternalHoconValue" />.</returns>
        public static uint GetUInt(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the integer value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="InternalHoconValue" />.</returns>
        public static int GetInt(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned short value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The unsigned short value represented by this <see cref="InternalHoconValue" />.</returns>
        public static ushort GetUShort(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the short value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The short value represented by this <see cref="InternalHoconValue" />.</returns>
        public static short GetShort(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the signed byte value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The signed byte value represented by this <see cref="InternalHoconValue" />.</returns>
        public static sbyte GetSByte(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the byte value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="InternalHoconValue" />.</returns>
        public static byte GetByte(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the time span value from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The time span value represented by this <see cref="InternalHoconValue" />.</returns>
        public static TimeSpan GetTimeSpan(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the long value, optionally suffixed with a 'b', from this <see cref="InternalHoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="InternalHoconValue" />.</returns>
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

        public static HoconElement GetValue(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static HoconElement GetValue(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static string GetString(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static string GetString(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static char GetChar(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static char GetChar(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static bool GetBoolean(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static bool GetBoolean(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static decimal GetDecimal(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static decimal GetDecimal(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static float GetFloat(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static float GetFloat(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static double GetDouble(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static double GetDouble(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static BigInteger GetBigInteger(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static BigInteger GetBigInteger(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static ulong GetULong(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static ulong GetULong(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static long GetLong(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static long GetLong(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static uint GetUInt(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static uint GetUInt(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static int GetInt(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static int GetInt(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static ushort GetUShort(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static ushort GetUShort(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static short GetShort(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static short GetShort(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static sbyte GetSByte(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static sbyte GetSByte(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static byte GetByte(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static byte GetByte(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static TimeSpan GetTimeSpan(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public static TimeSpan GetTimeSpan(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return obj[path];
            }
            catch (HoconException ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        public static long? GetByteSize(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return ParseByteSize(obj[path]);
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }

        }

        public static long? GetByteSize(this HoconElement element, HoconPath path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            try
            {
                return ParseByteSize(obj[path]);
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path.ToString(), ex);
            }
        }

        private static long? ParseByteSize(string res)
        {
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

        #region Helpers

        private static readonly Regex TimeSpanRegex = new Regex(
            @"^(?<value>([0-9]+(\.[0-9]+)?))\s*(?<unit>(nanoseconds|nanosecond|nanos|nano|ns|microseconds|microsecond|micros|micro|us|milliseconds|millisecond|millis|milli|ms|seconds|second|s|minutes|minute|m|hours|hour|h|days|day|d))$",
            RegexOptions.Compiled);

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