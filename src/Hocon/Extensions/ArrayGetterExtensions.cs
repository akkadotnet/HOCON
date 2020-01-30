// -----------------------------------------------------------------------
// <copyright file="ArrayGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    public static class ArrayGetterExtensions
    {
        public static T[] ToArray<T>(this HoconElement element)
        {
            if (element is HoconObject obj)
                return obj.ToArray().ToArray<T>();

            if (!(element is HoconArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconArray)} type. {element.GetType()} found instead.");

            return arr.Select(v => v.Get<T>()).ToArray();
        }

        public static IList<HoconElement> GetArray(this HoconElement element)
        {
            if (element is HoconObject obj)
                return obj.ToArray().ToList();

            if (!(element is HoconArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconArray)} type. {element.GetType()} found instead.");

            return arr.ToList();
        }

        /// <summary>
        ///     Retrieves a list of byte values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of byte values represented by this <see cref="HoconElement" />.</returns>
        public static byte[] ToByteArray(this HoconElement element)
        {
            return element;
        }

        public static IList<byte> GetByteList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToByteArray();
        }

        public static IList<byte> GetByteList(this HoconElement element, string path, IList<byte> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToByteArray();
        }

        public static sbyte[] ToSByteArray(this HoconElement element)
        {
            return element;
        }

        public static IList<sbyte> GetSByteList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToSByteArray();
        }

        public static IList<sbyte> GetSByteList(this HoconElement element, string path, IList<sbyte> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToSByteArray();
        }

        /// <summary>
        ///     Retrieves a list of integer values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of integer values represented by this <see cref="HoconElement" />.</returns>
        public static int[] ToIntArray(this HoconElement element)
        {
            return element;
        }

        public static IList<int> GetIntList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToIntArray();
        }

        public static IList<int> GetIntList(this HoconElement element, string path, IList<int> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToIntArray();
        }

        public static uint[] ToUIntArray(this HoconElement element)
        {
            return element;
        }

        public static IList<uint> GetUIntList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToUIntArray();
        }

        public static IList<uint> GetUIntList(this HoconElement element, string path, IList<uint> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToUIntArray();
        }

        public static short[] ToShortArray(this HoconElement element)
        {
            return element;
        }

        public static IList<short> GetShortList(this HoconElement element, string path, IList<short> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToShortArray();
        }

        public static ushort[] ToUShortArray(this HoconElement element)
        {
            return element;
        }

        public static IList<ushort> GetUShortList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToUShortArray();
        }

        public static IList<ushort> GetUShortList(this HoconElement element, string path, IList<ushort> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToUShortArray();
        }

        /// <summary>
        ///     Retrieves a list of long values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of long values represented by this <see cref="HoconElement" />.</returns>
        public static long[] ToLongArray(this HoconElement element)
        {
            return element;
        }

        public static IList<long> GetLongList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToLongArray();
        }

        public static IList<long> GetLongList(this HoconElement element, string path, IList<long> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToLongArray();
        }

        public static ulong[] ToULongArray(this HoconElement element)
        {
            return element;
        }

        public static IList<ulong> GetULongList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToULongArray();
        }

        public static IList<ulong> GetULongList(this HoconElement element, string path, IList<ulong> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToULongArray();
        }

        /// <summary>
        ///     Retrieves a list of boolean values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconElement" />.</returns>
        public static bool[] ToBooleanArray(this HoconElement element)
        {
            return element;
        }

        public static IList<bool> GetBooleanList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToBooleanArray();
        }

        public static IList<bool> GetBooleanList(this HoconElement element, string path, IList<bool> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToBooleanArray();
        }

        /// <summary>
        ///     Retrieves a list of float values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of float values represented by this <see cref="HoconElement" />.</returns>
        public static float[] ToFloatArray(this HoconElement element)
        {
            return element;
        }

        public static IList<float> GetFloatList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToFloatArray();
        }

        public static IList<float> GetFloatList(this HoconElement element, string path, IList<float> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToFloatArray();
        }

        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconElement" />.</returns>
        public static double[] ToDoubleArray(this HoconElement element)
        {
            return element;
        }

        public static IList<double> GetDoubleList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToDoubleArray();
        }

        public static IList<double> GetDoubleList(this HoconElement element, string path, IList<double> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToDoubleArray();
        }

        /// <summary>
        ///     Retrieves a list of decimal values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of decimal values represented by this <see cref="HoconElement" />.</returns>
        public static decimal[] ToDecimalArray(this HoconElement element)
        {
            return element;
        }

        public static IList<decimal> GetDecimalList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToDecimalArray();
        }

        public static IList<decimal> GetDecimalList(this HoconElement element, string path, IList<decimal> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToDecimalArray();
        }

        /// <summary>
        ///     Retrieves a list of string values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static string[] ToStringArray(this HoconElement element)
        {
            return element;
        }

        public static IList<string> GetStringList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToStringArray();
        }

        public static IList<string> GetStringList(this HoconElement element, string path, IList<string> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToStringArray();
        }

        public static char[] ToCharArray(this HoconElement element)
        {
            return element;
        }

        public static IList<char> GetCharList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToCharArray();
        }

        public static IList<char> GetCharList(this HoconElement element, string path, IList<char> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToCharArray();
        }

        /// <summary>
        ///     Retrieves a list of objects from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconImmutableElement" />.</returns>
        public static HoconObject[] ToObjectArray(this HoconElement element)
        {
            if (element is HoconObject obj)
                return obj.ToArray().ToObjectArray();

            if (!(element is HoconArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconArray)} type. {element.GetType()} found instead.");

            return arr.Cast<HoconObject>().ToArray();
        }

        public static IList<HoconObject> GetObjectList(this HoconElement element, string path)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            return obj[path].ToObjectArray();
        }

        public static IList<HoconObject> GetObjectList(this HoconElement element, string path, IList<HoconObject> @default)
        {
            if (!(element is HoconObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconObject)} type. {element.GetType()} found instead.");

            if (!obj.HasPath(path))
                return @default;

            return obj[path].ToObjectArray();
        }
    }
}