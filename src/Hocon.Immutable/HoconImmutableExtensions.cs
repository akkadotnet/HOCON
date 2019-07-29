//-----------------------------------------------------------------------
// <copyright file="HoconImmutableExtensions.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon.Immutable
{
    public static class HoconImmutableExtensions
    {
        #region HoconImmutable typecast converter 

        public static HoconImmutableObject ToObject(this HoconImmutableElement element)
        {
            return (HoconImmutableObject) element;
        }

        public static HoconImmutableArray ToArray(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj.ToArray();

            return (HoconImmutableArray) element;
        }

        public static HoconImmutableLiteral ToLiteral(this HoconImmutableElement element)
        {
            return (HoconImmutableLiteral) element;
        }

        #endregion

        #region Hocon class to HoconImmutable class converter

        public static HoconImmutableObject ToHoconImmutable(this HoconRoot root)
        {
            return root.Value.GetObject().ToHoconImmutable();
        }

        public static HoconImmutableElement ToHoconImmutable(this IHoconElement element)
        {
            switch (element)
            {
                case HoconObject o:
                    return o.ToHoconImmutable();
                case HoconArray a:
                    return a.ToHoconImmutable();
                case HoconLiteral l:
                    return l.ToHoconImmutable();
                case HoconValue v:
                    return v.ToHoconImmutable();
                case HoconField f:
                    return f.ToHoconImmutable();
                default:
                    throw new HoconException($"Unknown Hocon element type:{element.GetType().Name}");
            }
        }

        public static HoconImmutableElement ToHoconImmutable(this HoconValue value)
        {
            switch (value.Type)
            {
                case HoconType.Object:
                    var obj = value.GetObject();
                    return obj.ToHoconImmutable();
                case HoconType.Array:
                    var list = new List<HoconImmutableElement>();
                    foreach (var element in value.GetArray())
                    {
                        list.Add(element.ToHoconImmutable());
                    }
                    return new HoconImmutableArray(list);
                case HoconType.Literal:
                    return new HoconImmutableLiteral(value.GetString());
                case HoconType.Empty:
                    return new HoconImmutableLiteral(null);
                default:
                    // Should never reach this line.
                    throw new HoconException($"Unknown Hocon field type:{value.Type}");
            }
        }

        public static HoconImmutableObject ToHoconImmutable(this HoconObject @object)
        {
            var dict = new SortedDictionary<string, HoconImmutableElement>();
            foreach (var kvp in @object)
            {
                dict[kvp.Key] = kvp.Value.ToHoconImmutable();
            }
            return new HoconImmutableObject(dict);
        }

        public static HoconImmutableElement ToHoconImmutable(this HoconField field)
        {
            return field.Value.ToHoconImmutable();
        }

        public static HoconImmutableArray ToHoconImmutable(this HoconArray array)
        {
            var list = new List<HoconImmutableElement>();
            foreach (var element in array)
            {
                list.Add(element.ToHoconImmutable());
            }
            return new HoconImmutableArray(list);
        }

        public static HoconImmutableLiteral ToHoconImmutable(this HoconLiteral literal)
        {
            return literal.LiteralType == HoconLiteralType.Null ? null : new HoconImmutableLiteral(literal.Value);
        }

        #endregion

        #region Value Getter methods

        public static T Get<T>(this HoconImmutableElement element)
        {
            return (T) element.Get(typeof(T));
        }

        public static object Get(this HoconImmutableElement element, Type type)
        {
            if (PrimitiveConverters.ContainsKey(type))
                return PrimitiveConverters[type].Invoke(element);

            throw new NotImplementedException();
        }

        private static readonly Dictionary<Type, Func<HoconImmutableElement, object>> PrimitiveConverters = 
            new Dictionary<Type, Func<HoconImmutableElement, object>>
            {
                { typeof(bool), e => e.GetBoolean() },
                { typeof(bool[]), e => e.ToArray<bool>() },

                { typeof(char), e => e.GetChar() },
                { typeof(char[]), e => e.ToCharArray() },

                { typeof(float), e => e.GetFloat() },
                { typeof(float[]), e => e.ToArray<float>() },

                { typeof(double), e => e.GetDouble() },
                { typeof(double[]), e => e.ToArray<double>() },

                { typeof(decimal), e => e.GetDecimal() },
                { typeof(decimal[]), e => e.ToArray<decimal>() },

                { typeof(byte), e => e.GetByte() },
                { typeof(byte[]), e => e.ToArray<byte>() },

                { typeof(sbyte), e => e.GetSByte() },
                { typeof(sbyte[]), e => e.ToArray<sbyte>() },

                { typeof(ushort), e => e.GetUShort() },
                { typeof(ushort[]), e => e.ToArray<ushort>() },

                { typeof(short), e => e.GetShort() },
                { typeof(short[]), e => e.ToArray<short>() },

                { typeof(uint), e => e.GetUInt() },
                { typeof(uint[]), e => e.ToArray<uint>() },

                { typeof(int), e => e.GetInt() },
                { typeof(int[]), e => e.ToArray<int>() },

                { typeof(ulong), e => e.GetULong() },
                { typeof(ulong[]), e => e.ToArray<ulong>() },

                { typeof(long), e => e.GetLong() },
                { typeof(long[]), e => e.ToArray<long>() },

                { typeof(string), e => e.GetString() },
                { typeof(string[]), e => e.ToArray<string>() },

                { typeof(TimeSpan), e => e.GetTimeSpan() },
                { typeof(TimeSpan[]), e => e.ToArray<TimeSpan>() },
            };

        #region Primitive data type getters

        public static string GetString(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException($"Value getter functions can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");
            return lit.Value;
        }

        public static char GetChar(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException($"Value getter functions can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");
            return lit.Value[0];
        }

        /// <summary>
        /// Retrieves the boolean value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The boolean value represented by this <see cref="HoconValue"/>.</returns>
        /// <exception cref="System.NotSupportedException">
        /// This exception occurs when the <see cref="HoconValue"/> doesn't
        /// conform to the standard boolean values: "on", "off", "true", or "false"
        /// </exception>
        public static bool GetBoolean(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the decimal value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The decimal value represented by this <see cref="HoconValue"/>.</returns>
        public static decimal GetDecimal(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the float value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The float value represented by this <see cref="HoconValue"/>.</returns>
        public static float GetFloat(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the double value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The double value represented by this <see cref="HoconValue"/>.</returns>
        public static double GetDouble(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the unsigned long value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The unsigned long value represented by this <see cref="HoconValue"/>.</returns>
        public static ulong GetULong(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the long value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue"/>.</returns>
        public static long GetLong(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the unsigned integer value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The unsigned integer value represented by this <see cref="HoconValue"/>.</returns>
        public static uint GetUInt(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the integer value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The integer value represented by this <see cref="HoconValue"/>.</returns>
        public static int GetInt(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the unsigned short value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The unsigned short value represented by this <see cref="HoconValue"/>.</returns>
        public static ushort GetUShort(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the short value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The short value represented by this <see cref="HoconValue"/>.</returns>
        public static short GetShort(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the signed byte value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The signed byte value represented by this <see cref="HoconValue"/>.</returns>
        public static sbyte GetSByte(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the byte value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The byte value represented by this <see cref="HoconValue"/>.</returns>
        public static byte GetByte(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the time span value from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The time span value represented by this <see cref="HoconValue"/>.</returns>
        public static TimeSpan GetTimeSpan(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves the long value, optionally suffixed with a 'b', from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>The long value represented by this <see cref="HoconValue"/>.</returns>
        public static long? GetByteSize(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException($"Value getter functions can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");

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
            {
                if (byteSize.Suffixes.Any(suffix => string.Equals(unit, suffix, StringComparison.Ordinal)))
                {
                    return (long)(byteSize.Factor * double.Parse(value));
                }
            }

            throw new FormatException($"{unit} is not a valid byte size suffix");
        }

        #region Helpers

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
        #endregion

        #endregion

        #region List array getters

        public static T[] ToArray<T>(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj.ToArray().ToArray<T>();

            if (!(element is HoconImmutableArray arr))
                throw new HoconException($"Array getter functions can only work on {nameof(HoconImmutableArray)} type. {element.GetType()} found instead.");

            return arr.Select(v => v.Get<T>()).ToArray();
        }

        /// <summary>
        /// Retrieves a list of byte values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of byte values represented by this <see cref="HoconValue"/>.</returns>
        public static byte[] ToByteArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of integer values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of integer values represented by this <see cref="HoconValue"/>.</returns>
        public static int[] ToIntArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static short[] ToShortArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of long values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of long values represented by this <see cref="HoconValue"/>.</returns>
        public static long[] ToLongArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of boolean values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconValue"/>.</returns>
        public static bool[] ToBooleanArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of float values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of float values represented by this <see cref="HoconValue"/>.</returns>
        public static float[] ToFloatArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of double values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconValue"/>.</returns>
        public static double[] ToDoubleArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of decimal values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of decimal values represented by this <see cref="HoconValue"/>.</returns>
        public static decimal[] ToDecimalArray(this HoconImmutableElement element)
        {
            return element;
        }

        /// <summary>
        /// Retrieves a list of string values from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconValue"/>.</returns>
        public static string[] ToStringArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static char[] ToCharArray(this HoconImmutableElement element)
        {
            if (!(element is HoconImmutableLiteral lit))
                throw new HoconException($"Char list getter functions can only work on {nameof(HoconImmutableLiteral)} type. {element.GetType()} found instead.");

            return lit.Value.ToCharArray();
        }

        /// <summary>
        /// Retrieves a list of objects from this <see cref="HoconValue"/>.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconValue"/>.</returns>
        public static HoconImmutableObject[] ToObjectArray(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj.ToArray().ToObjectArray();

            if (!(element is HoconImmutableArray arr))
                throw new HoconException($"Array getter functions can only work on {nameof(HoconImmutableArray)} type. {element.GetType()} found instead.");

            return arr.Cast<HoconImmutableObject>().ToArray();
        }

        #endregion

        #endregion
    }
}
