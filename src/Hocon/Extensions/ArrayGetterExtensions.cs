// -----------------------------------------------------------------------
// <copyright file="ArrayGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System;
using System.Numerics;

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

        #region Boolean
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
            return WrapWithValueException(path, () => element[path].ToBooleanArray());
        }

        public static IList<bool> GetBooleanList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToBooleanArray());
        }

        public static IList<bool> GetBooleanList(this HoconElement element, string path, IList<bool> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetBooleanList(out var result))
                        return result;

            return @default;
        }

        public static IList<bool> GetBooleanList(this HoconElement element, HoconPath path, IList<bool> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetBooleanList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetBooleanList(this HoconElement element, out IList<bool> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<bool>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetBoolean(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<bool>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetBoolean(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Decimal
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
            return WrapWithValueException(path, () => element[path].ToDecimalArray());
        }

        public static IList<decimal> GetDecimalList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToDecimalArray());
        }

        public static IList<decimal> GetDecimalList(this HoconElement element, string path, IList<decimal> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetDecimalList(out var result))
                        return result;

            return @default;
        }

        public static IList<decimal> GetDecimalList(this HoconElement element, HoconPath path, IList<decimal> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetDecimalList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetDecimalList(this HoconElement element, out IList<decimal> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<decimal>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetDecimal(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<decimal>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetDecimal(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Float
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
            return WrapWithValueException(path, () => element[path].ToFloatArray());
        }

        public static IList<float> GetFloatList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToFloatArray());
        }

        public static IList<float> GetFloatList(this HoconElement element, string path, IList<float> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetFloatList(out var result))
                        return result;

            return @default;
        }

        public static IList<float> GetFloatList(this HoconElement element, HoconPath path, IList<float> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetFloatList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetFloatList(this HoconElement element, out IList<float> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<float>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetFloat(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<float>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetFloat(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Double
        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconElement" />.</returns>
        public static double[] ToDoubleArray(this HoconElement element)
        {
            return element;
        }

        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconElement" />.</returns>
        public static IList<double> GetDoubleList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToDoubleArray());
        }

        public static IList<double> GetDoubleList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToDoubleArray());
        }

        public static IList<double> GetDoubleList(this HoconElement element, string path, IList<double> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetDoubleList(out var result))
                        return result;

            return @default;
        }

        /// <inheritdoc cref="GetDoubleList(HoconElement, string)" />
        public static IList<double> GetDoubleList(this HoconElement element, HoconPath path, IList<double> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetDoubleList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetDoubleList(this HoconElement element, out IList<double> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<double>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetDouble(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<double>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetDouble(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region SByte
        public static sbyte[] ToSByteArray(this HoconElement element)
        {
            return element;
        }

        public static IList<sbyte> GetSByteList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToSByteArray());
        }

        public static IList<sbyte> GetSByteList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToSByteArray());
        }

        public static IList<sbyte> GetSByteList(this HoconElement element, string path, IList<sbyte> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetSByteList(out var result))
                        return result;

            return @default;
        }

        public static IList<sbyte> GetSByteList(this HoconElement element, HoconPath path, IList<sbyte> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetSByteList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetSByteList(this HoconElement element, out IList<sbyte> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<sbyte>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetSByte(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<sbyte>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetSByte(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Byte
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
            return WrapWithValueException(path, () => element[path].ToByteArray());
        }

        public static IList<byte> GetByteList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToByteArray());
        }

        public static IList<byte> GetByteList(this HoconElement element, string path, IList<byte> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetByteList(out var result))
                        return result;

            return @default;
        }

        public static IList<byte> GetByteList(this HoconElement element, HoconPath path, IList<byte> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetByteList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetByteList(this HoconElement element, out IList<byte> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<byte>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetByte(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<byte>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetByte(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Short
        public static short[] ToShortArray(this HoconElement element)
        {
            return element;
        }

        public static IList<short> GetShortList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToShortArray());
        }

        public static IList<short> GetShortList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToShortArray());
        }

        public static IList<short> GetShortList(this HoconElement element, string path, IList<short> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetShortList(out var result))
                        return result;

            return @default;
        }

        public static IList<short> GetShortList(this HoconElement element, HoconPath path, IList<short> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetShortList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetShortList(this HoconElement element, out IList<short> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<short>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetShort(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<short>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetShort(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region UShort
        public static ushort[] ToUShortArray(this HoconElement element)
        {
            return element;
        }

        public static IList<ushort> GetUShortList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToUShortArray());
        }
        public static IList<ushort> GetUShortList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToUShortArray());
        }

        public static IList<ushort> GetUShortList(this HoconElement element, string path, IList<ushort> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetUShortList(out var result))
                        return result;

            return @default;
        }

        public static IList<ushort> GetUShortList(this HoconElement element, HoconPath path, IList<ushort> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetUShortList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetUShortList(this HoconElement element, out IList<ushort> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<ushort>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetUShort(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<ushort>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetUShort(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Int
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
            return WrapWithValueException(path, () => element[path].ToIntArray());
        }

        public static IList<int> GetIntList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToIntArray());
        }

        public static IList<int> GetIntList(this HoconElement element, string path, IList<int> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetIntList(out var result))
                        return result;

            return @default;
        }

        public static IList<int> GetIntList(this HoconElement element, HoconPath path, IList<int> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetIntList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetIntList(this HoconElement element, out IList<int> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<int>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetInt(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<int>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetInt(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region UInt
        public static uint[] ToUIntArray(this HoconElement element)
        {
            return element;
        }

        public static IList<uint> GetUIntList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToUIntArray());
        }

        public static IList<uint> GetUIntList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToUIntArray());
        }

        public static IList<uint> GetUIntList(this HoconElement element, string path, IList<uint> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetUIntList(out var result))
                        return result;

            return @default;
        }

        public static IList<uint> GetUIntList(this HoconElement element, HoconPath path, IList<uint> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetUIntList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetUIntList(this HoconElement element, out IList<uint> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<uint>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetUInt(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<uint>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetUInt(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Long
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
            return WrapWithValueException(path, () => element[path].ToLongArray());
        }

        public static IList<long> GetLongList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToLongArray());
        }

        public static IList<long> GetLongList(this HoconElement element, string path, IList<long> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetLongList(out var result))
                        return result;

            return @default;
        }

        public static IList<long> GetLongList(this HoconElement element, HoconPath path, IList<long> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetLongList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetLongList(this HoconElement element, out IList<long> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<long>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetLong(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<long>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetLong(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region ULong
        public static ulong[] ToULongArray(this HoconElement element)
        {
            return element;
        }

        public static IList<ulong> GetULongList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToULongArray());
        }

        public static IList<ulong> GetULongList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToULongArray());
        }

        public static IList<ulong> GetULongList(this HoconElement element, string path, IList<ulong> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetULongList(out var result))
                        return result;

            return @default;
        }

        public static IList<ulong> GetULongList(this HoconElement element, HoconPath path, IList<ulong> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetULongList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetULongList(this HoconElement element, out IList<ulong> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<ulong>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetULong(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<ulong>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetULong(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region BigInteger
        public static BigInteger[] ToBigIntegerArray(this HoconElement element)
        {
            return element;
        }

        public static IList<BigInteger> GetBigIntegerList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToBigIntegerArray());
        }

        public static IList<BigInteger> GetBigIntegerList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToBigIntegerArray());
        }

        public static IList<BigInteger> GetBigIntegerList(this HoconElement element, string path, IList<BigInteger> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetBigIntegerList(out var result))
                        return result;

            return @default;
        }

        public static IList<BigInteger> GetBigIntegerList(this HoconElement element, HoconPath path, IList<BigInteger> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetBigIntegerList(out var result))
                        return result;

            return @default;
        }
        public static bool TryGetBigIntegerList(this HoconElement element, out IList<BigInteger> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<BigInteger>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetBigInteger(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<BigInteger>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetBigInteger(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region String
        /// <summary>
        ///     Retrieves a list of string values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconElement" />.</returns>
        public static string[] ToStringArray(this HoconElement element)
        {
            return element;
        }

        public static IList<string> GetStringList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToStringArray());
        }

        public static IList<string> GetStringList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToStringArray());
        }

        public static IList<string> GetStringList(this HoconElement element, string path, IList<string> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetStringList(out var result))
                        return result;

            return @default;
        }

        public static IList<string> GetStringList(this HoconElement element, HoconPath path, IList<string> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetStringList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetStringList(this HoconElement element, out IList<string> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<string>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetString(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<string>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetString(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Char
        public static char[] ToCharArray(this HoconElement element)
        {
            return element;
        }

        public static IList<char> GetCharList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToCharArray());
        }

        public static IList<char> GetCharList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToCharArray());
        }

        public static IList<char> GetCharList(this HoconElement element, string path, IList<char> @default = null)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetCharList(out var result))
                        return result;

            return @default;
        }

        public static IList<char> GetCharList(this HoconElement element, HoconPath path, IList<char> @default = null)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetCharList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetCharList(this HoconElement element, out IList<char> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<char>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetChar(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<char>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetChar(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                case HoconLiteral lit:
                    if (lit.Value != null)
                    {
                        result = lit.Value.ToCharArray();
                        return true;
                    }
                    return false;
            }

            // Should never reach this code
            return false;
        }
        #endregion

        #region HoconObject
        /// <summary>
        ///     Retrieves a list of objects from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconElement" />.</returns>
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
            return WrapWithValueException(path, () => element[path].ToObjectArray());
        }

        public static IList<HoconObject> GetObjectList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToObjectArray());
        }

        public static IList<HoconObject> GetObjectList(this HoconElement element, string path, IList<HoconObject> @default)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetObjectList(out var result))
                        return result;

            return @default;
        }

        /// <inheritdoc cref="GetObjectList(HoconElement, string)" />
        public static IList<HoconObject> GetObjectList(this HoconElement element, HoconPath path, IList<HoconObject> @default = null)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetObjectList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetObjectList(this HoconElement element, out IList<HoconObject> result)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<HoconObject>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetObject(out var res))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<HoconObject>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetObject(out var res))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region TimeSpan
        public static TimeSpan[] ToTimeSpanArray(this HoconElement element)
        {
            return element;
        }

        public static IList<TimeSpan> GetTimeSpanList(this HoconElement element, string path)
        {
            return WrapWithValueException(path, () => element[path].ToTimeSpanArray());
        }

        public static IList<TimeSpan> GetTimeSpanList(this HoconElement element, HoconPath path)
        {
            return WrapWithValueException(path, () => element[path].ToTimeSpanArray());
        }

        public static IList<TimeSpan> GetTimeSpanList(this HoconElement element, string path, IList<TimeSpan> @default = null)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetTimeSpanList(out var result))
                        return result;

            return @default;
        }

        public static IList<TimeSpan> GetTimeSpanList(this HoconElement element, HoconPath path, IList<TimeSpan> @default = null)
        {
            if (element is HoconObject obj)
                if (obj.TryGetValue(path, out var value))
                    if (value.TryGetTimeSpanList(out var result))
                        return result;

            return @default;
        }

        public static bool TryGetTimeSpanList(this HoconElement element, out IList<TimeSpan> result, bool allowInfinite = true)
        {
            result = default;
            switch (element)
            {
                case HoconArray arr:
                    var list = new List<TimeSpan>();
                    foreach (var val in arr)
                    {
                        if (!val.TryGetTimeSpan(out var res, allowInfinite))
                            return false;
                        list.Add(res);
                    }
                    result = list;
                    return true;
                case HoconObject obj:
                    if (!obj.TryGetArray(out var oArr))
                        return false;
                    var oList = new List<TimeSpan>();
                    foreach (var val in oArr)
                    {
                        if (!val.TryGetTimeSpan(out var res, allowInfinite))
                            return false;
                        oList.Add(res);
                    }
                    result = oList;
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}