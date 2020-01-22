// -----------------------------------------------------------------------
// <copyright file="ArrayGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Hocon.Immutable.Extensions
{
    public static class ArrayGetterExtensions
    {
        public static T[] ToArray<T>(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj.ToArray().ToArray<T>();

            if (!(element is HoconImmutableArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconImmutableArray)} type. {element.GetType()} found instead.");

            return arr.Select(v => v.Get<T>()).ToArray();
        }

        public static IList<HoconImmutableElement> GetArray(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj.ToArray().ToList();

            if (!(element is HoconImmutableArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconImmutableArray)} type. {element.GetType()} found instead.");

            return arr.ToList();
        }

        /// <summary>
        ///     Retrieves a list of byte values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of byte values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static byte[] ToByteArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<byte> GetByteList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToByteArray();
        }

        public static sbyte[] ToSByteArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<sbyte> GetSByteList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToSByteArray();
        }

        /// <summary>
        ///     Retrieves a list of integer values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of integer values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static int[] ToIntArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<int> GetIntList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToIntArray();
        }

        public static uint[] ToUIntArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<uint> GetUIntList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToUIntArray();
        }

        public static short[] ToShortArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<short> GetShortList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToShortArray();
        }

        public static ushort[] ToUShortArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<ushort> GetUShortList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToUShortArray();
        }

        /// <summary>
        ///     Retrieves a list of long values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of long values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static long[] ToLongArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<long> GetLongList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToLongArray();
        }

        public static ulong[] ToULongArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<ulong> GetULongList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToULongArray();
        }

        /// <summary>
        ///     Retrieves a list of boolean values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static bool[] ToBooleanArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<bool> GetBooleanList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToBooleanArray();
        }

        /// <summary>
        ///     Retrieves a list of float values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of float values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static float[] ToFloatArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<float> GetFloatList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToFloatArray();
        }

        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static double[] ToDoubleArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<double> GetDoubleList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToDoubleArray();
        }

        /// <summary>
        ///     Retrieves a list of decimal values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of decimal values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static decimal[] ToDecimalArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<decimal> GetDecimalList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToDecimalArray();
        }

        /// <summary>
        ///     Retrieves a list of string values from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconImmutableElement" />.</returns>
        public static string[] ToStringArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<string> GetStringList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToStringArray();
        }

        public static char[] ToCharArray(this HoconImmutableElement element)
        {
            return element;
        }

        public static IList<char> GetCharList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToCharArray();
        }

        /// <summary>
        ///     Retrieves a list of objects from this <see cref="HoconImmutableElement" />.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconImmutableElement" />.</returns>
        public static HoconImmutableObject[] ToObjectArray(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj.ToArray().ToObjectArray();

            if (!(element is HoconImmutableArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconImmutableArray)} type. {element.GetType()} found instead.");

            return arr.Cast<HoconImmutableObject>().ToArray();
        }

        public static IList<HoconImmutableObject> GetObjectList(this HoconImmutableElement element, string path)
        {
            if (!(element is HoconImmutableObject obj))
                throw new HoconException(
                    $"Path list getter can only work on {nameof(HoconImmutableObject)} type. {element.GetType()} found instead.");

            return obj[path].ToObjectArray();
        }
    }
}