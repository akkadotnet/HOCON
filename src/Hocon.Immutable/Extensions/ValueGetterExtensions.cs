// -----------------------------------------------------------------------
// <copyright file="ValueGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Numerics;

namespace Hocon.Immutable.Extensions
{
    public static class ValueGetterExtensions
    {
        public static string GetString(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException(
                    $"Value getter can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");
            return lit.Value;
        }

        public static char GetChar(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException(
                    $"Value getter functions can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");
            return lit.Value[0];
        }

        /// <summary>
        ///     Retrieves the boolean value from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>The boolean value represented by this <see cref="HoconImmutableElement" />.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     This exception occurs when the <see cref="HoconImmutableElement" /> is not a <see cref="HoconImmutableLiteral" />
        ///     and it doesn't
        ///     conform to the standard boolean values: "on", "off", "yes", "no", "true", or "false"
        /// </exception>
        public static bool GetBoolean(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the decimal value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="HoconValue" />.</returns>
        public static decimal GetDecimal(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the float value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The float value represented by this <see cref="HoconValue" />.</returns>
        public static float GetFloat(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the double value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue" />.</returns>
        public static double GetDouble(this HoconImmutableElement element)
        {
            return element;
        }

        public static BigInteger GetBigInteger(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned long value represented by this <see cref="HoconValue" />.</returns>
        public static ulong GetULong(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the long value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public static long GetLong(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned integer value represented by this <see cref="HoconValue" />.</returns>
        public static uint GetUInt(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the integer value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="HoconValue" />.</returns>
        public static int GetInt(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the unsigned short value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The unsigned short value represented by this <see cref="HoconValue" />.</returns>
        public static ushort GetUShort(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the short value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The short value represented by this <see cref="HoconValue" />.</returns>
        public static short GetShort(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the signed byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The signed byte value represented by this <see cref="HoconValue" />.</returns>
        public static sbyte GetSByte(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the byte value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="HoconValue" />.</returns>
        public static byte GetByte(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the time span value from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The time span value represented by this <see cref="HoconValue" />.</returns>
        public static TimeSpan GetTimeSpan(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue" />.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue" />.</returns>
        public static long? GetByteSize(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException(
                    $"Value getter functions can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");

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

        public static HoconImmutableElement GetValue(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static string GetString(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static char GetChar(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static bool GetBoolean(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static decimal GetDecimal(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static float GetFloat(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static double GetDouble(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static BigInteger GetBigInteger(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static ulong GetULong(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static long GetLong(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static uint GetUInt(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static int GetInt(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static ushort GetUShort(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static short GetShort(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static sbyte GetSByte(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static byte GetByte(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static TimeSpan GetTimeSpan(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

        public static long? GetByteSize(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path value getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");
            return obj[path];
        }

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