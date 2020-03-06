// -----------------------------------------------------------------------
// <copyright file="HoconImmutableElement.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Globalization;
using System.Threading;

namespace Hocon
{
    public abstract class HoconElement :
        IEquatable<HoconElement>
    {
        [Obsolete("There is no need to use Value property anymore, please remove it.")]
        public HoconElement Value => this;

        public abstract HoconType Type { get; }

        public abstract string Raw { get; }

        public HoconElement this[int index]
        {
            get
            {
                switch (this)
                {
                    case HoconObject obj:
                        return obj.ToArray()[index];
                    case HoconArray arr:
                        return arr[index];
                    default:
                        throw new HoconException(
                            $"Integer indexers only works on positive integer keyed {nameof(HoconObject)} or {nameof(HoconArray)}");
                }
            }
        }

        public virtual HoconElement this[string path]
        {
            get
            {
                if (this is HoconObject obj)
                    return obj[path];

                throw new HoconException($"String path indexers only works on {nameof(HoconObject)}");
            }
        }

        public virtual HoconElement this[HoconPath path]
        {
            get
            {
                if (this is HoconObject obj)
                    return obj[path];

                throw new HoconException($"HoconPath path indexers only works on {nameof(HoconObject)}");
            }
        }

        public HoconElement AtKey(string key)
        {
            return new HoconObjectBuilder
            {
                { key, this }
            }.Build();
        }

        public virtual bool HasPath(string path)
        {
            return TryGetValue(path, out _);
        }

        public virtual bool HasPath(HoconPath path)
        {
            return TryGetValue(path, out _);
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!(other is HoconElement element)) return false;
            return Equals(element);
        }

        public abstract bool Equals(HoconElement other);

        public abstract string ToString(int indent, int indentSize);

        public static bool operator == (HoconElement left, HoconElement right)
        {
            if (ReferenceEquals(left, right)) return true;
            return left.Equals(right);
        }

        public static bool operator !=(HoconElement left, HoconElement right)
        {
            if (ReferenceEquals(left, right)) return false;
            return !left.Equals(right);
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

        public virtual HoconElement GetValue(string path)
        {
            if (!(this is HoconObject obj))
                throw new HoconValueException($"String path indexers only works on {nameof(HoconObject)}", path.ToString(), null);

            return obj.GetValue(path);
        }

        public virtual HoconElement GetValue(HoconPath path)
        {
            if (!(this is HoconObject obj))
                throw new HoconValueException($"HoconPath path indexers only works on {nameof(HoconObject)}", path.ToString(), null);

            return obj.GetValue(path);
        }

        public virtual bool TryGetValue(string path, out HoconElement result)
        {
            result = default;
            if (this is HoconObject obj)
                return obj.TryGetValue(path, out result);

            return false;
        }

        public virtual bool TryGetValue(HoconPath path, out HoconElement result)
        {
            result = default;
            if (this is HoconObject obj)
                return obj.TryGetValue(path, out result);

            return false;
        }

        #region Value getter
        /// <summary>
        ///     Retrieves a list of objects from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of objects represented by this <see cref="HoconElement" />.</returns>
        public HoconObject[] ToObjectArray()
        {
            if (this is HoconObject obj)
                return obj.ToArray().Cast<HoconObject>().ToArray();

            if (!(this is HoconArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconArray)} type. {GetType()} found instead.");

            return arr.Cast<HoconObject>().ToArray();
        }

        /*
        public T[] ToArray<T>()
        {
            if (this is HoconObject obj)
                return obj.ToArray().Cast<T>().ToArray();

            if (!(this is HoconArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconArray)} type. {GetType()} found instead.");

            return arr.Select(v => v.Get<T>()).ToArray();
        }
        */

        public IList<HoconElement> GetArray()
        {
            if (this is HoconObject obj)
                return obj.ToArray().ToList();

            if (!(this is HoconArray arr))
                throw new HoconException(
                    $"Array getter functions can only work on {nameof(HoconArray)} type. {GetType()} found instead.");

            return arr.ToList();
        }

        public HoconObject GetObject()
        {
            return this.ToObject();
        }

        public HoconObject GetObject(HoconObject @default)
        {
            if (TryGetObject(out var result))
                return result;
            return @default;
        }

        public bool TryGetObject(out HoconObject result)
        {
            result = default;
            if (this is HoconObject obj)
            {
                result = obj;
                return true;
            }
            return false;
        }

        #region Single value
        public virtual bool GetBoolean()
        {
            if (this is HoconLiteral lit)
                return lit.GetBoolean();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to bool, found {GetType()} instead.");
        }

        public virtual sbyte GetSByte()
        {
            if (this is HoconLiteral lit)
                return lit.GetSByte();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to sbyte, found {GetType()} instead.");
        }

        public virtual byte GetByte()
        {
            if (this is HoconLiteral lit)
                return lit.GetByte();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to byte, found {GetType()} instead.");
        }

        public virtual short GetShort()
        {
            if (this is HoconLiteral lit)
                return lit.GetShort();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to short, found {GetType()} instead.");
        }

        public virtual ushort GetUShort()
        {
            if (this is HoconLiteral lit)
                return lit.GetUShort();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to ushort, found {GetType()} instead.");
        }

        public virtual int GetInt()
        {
            if (this is HoconLiteral lit)
                return lit.GetInt();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to int, found {GetType()} instead.");
        }

        public virtual uint GetUInt()
        {
            if (this is HoconLiteral lit)
                return lit.GetUInt();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to uint, found {GetType()} instead.");
        }

        public virtual long GetLong()
        {
            if (this is HoconLiteral lit)
                return lit.GetLong();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to long, found {GetType()} instead.");
        }

        public virtual ulong GetULong()
        {
            if (this is HoconLiteral lit)
                return lit.GetULong();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to ulong, found {GetType()} instead.");
        }

        public virtual BigInteger GetBigInteger()
        {
            if (this is HoconLiteral lit)
                return lit.GetBigInteger();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to BigInteger, found {GetType()} instead.");
        }

        public virtual float GetFloat()
        {
            if (this is HoconLiteral lit)
                return lit.GetFloat();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to float, found {GetType()} instead.");
        }

        public virtual double GetDouble()
        {
            if (this is HoconLiteral lit)
                return lit.GetDouble();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to double, found {GetType()} instead.");
        }

        /// <summary>
        ///     Retrieves a list of decimal values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of decimal values represented by this <see cref="HoconElement" />.</returns>
        public virtual decimal GetDecimal()
        {
            if (this is HoconLiteral lit)
                return lit.GetDecimal();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to decimal, found {GetType()} instead.");
        }

        public virtual TimeSpan GetTimeSpan(bool allowInfinite = true)
        {
            if (this is HoconLiteral lit)
                return lit.GetTimeSpan(allowInfinite);

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to TimeSpan, found {GetType()} instead.");
        }

        public virtual string GetString()
        {
            if (this is HoconLiteral lit)
                return lit.GetString();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to string, found {GetType()} instead.");
        }

        public override string ToString()
        {
            if (this is HoconLiteral lit)
                return lit.ToString();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to string, found {GetType()} instead.");
        }

        //public static implicit operator char(Hoconthis this)
        public virtual char GetChar()
        {
            if (this is HoconLiteral lit)
                return lit.GetChar();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to string, found {GetType()} instead.");
        }

        public virtual long? GetByteSize()
        {
            if (this is HoconLiteral lit)
                return lit.GetByteSize();

            throw new HoconException(
                $"Can only convert {nameof(HoconLiteral)} type to string, found {GetType()} instead.");
        }

        #endregion

        #region TryGet value getter
        public bool TryGetBoolean(out bool result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetSByte(out sbyte result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetByte(out byte result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetShort(out short result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetUShort(out ushort result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetInt(out int result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetUInt(out uint result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetLong(out long result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetULong(out ulong result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetBigInteger(out BigInteger result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetFloat(out float result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetDouble(out double result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetDecimal(out decimal result)
        {
            result = default;
            if (!(this is HoconLiteral lit))
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

        public bool TryGetTimeSpan(out TimeSpan result, bool allowInfinite = true)
        {
            result = default;

            if (!(this is HoconLiteral lit))
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

        public bool TryGetString(out string result)
        {
            result = default;
            if (this is HoconLiteral lit)
            {
                result = lit.ToString();
                return true;
            }
            return false;
        }

        public bool TryGetChar(out char result)
        {
            result = default;
            if (this is HoconLiteral lit)
            {
                result = lit.GetChar();
                return true;
            }
            return false;
        }

        public bool TryGetByteSize(out long? result)
        {
            result = null;
            if (!TryGetString(out var res))
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
        #endregion

        #region List values
        /// <summary>
        ///     Retrieves a list of boolean values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<bool> GetBooleanList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetBooleanList();

                case HoconObject obj:
                    return obj.ToArray().GetBooleanList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into bool[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetBooleanList(out IList<bool> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetBooleanList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetBooleanList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of sbyte values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of sbyte values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<sbyte> GetSByteList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetSByteList();

                case HoconObject obj:
                    return obj.ToArray().GetSByteList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into sbyte[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetSByteList(out IList<sbyte> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetSByteList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetSByteList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of byte values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of byte values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<byte> GetByteList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetByteList();

                case HoconObject obj:
                    return obj.ToArray().GetByteList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into byte[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetByteList(out IList<byte> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetByteList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetByteList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of short values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of short values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<short> GetShortList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetShortList();

                case HoconObject obj:
                    return obj.ToArray().GetShortList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into short[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetShortList(out IList<short> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetShortList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetShortList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of ushort values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of ushort values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<ushort> GetUShortList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetUShortList();

                case HoconObject obj:
                    return obj.ToArray().GetUShortList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into ushort[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetUShortList(out IList<ushort> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetUShortList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetUShortList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of integer values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of integer values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<int> GetIntList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetIntList();

                case HoconObject obj:
                    return obj.ToArray().GetIntList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into int[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetIntList(out IList<int> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetIntList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetIntList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of unsigned integer values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of unsigned integer values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<uint> GetUIntList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetUIntList();

                case HoconObject obj:
                    return obj.ToArray().GetUIntList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into uint[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetUIntList(out IList<uint> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetUIntList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetUIntList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of long values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of long values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<long> GetLongList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetLongList();

                case HoconObject obj:
                    return obj.ToArray().GetLongList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into long[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetLongList(out IList<long> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetLongList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetLongList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of unsigned long values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of unsigned long values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<ulong> GetULongList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetULongList();

                case HoconObject obj:
                    return obj.ToArray().GetULongList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into ulong[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetULongList(out IList<ulong> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetULongList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetULongList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of BigInteger values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of BigInteger values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<BigInteger> GetBigIntegerList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetBigIntegerList();

                case HoconObject obj:
                    return obj.ToArray().GetBigIntegerList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into BigInteger[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetBigIntegerList(out IList<BigInteger> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetBigIntegerList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetBigIntegerList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of float values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of float values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<float> GetFloatList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetFloatList();

                case HoconObject obj:
                    return obj.ToArray().GetFloatList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into float[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetFloatList(out IList<float> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetFloatList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetFloatList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<double> GetDoubleList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetDoubleList();

                case HoconObject obj:
                    return obj.ToArray().GetDoubleList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into double[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetDoubleList(out IList<double> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetDoubleList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetDoubleList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        //public override implicit operator decimal[](HoconElement element)
        public virtual IList<decimal> GetDecimalList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetDecimalList();

                case HoconObject obj:
                    return obj.ToArray().GetDecimalList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into decimal[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetDecimalList(out IList<decimal> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetDecimalList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetDecimalList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of TimeSpan from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of TimeSpan represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<TimeSpan> GetTimeSpanList(bool allowInfinite = true)
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetTimeSpanList(allowInfinite);

                case HoconObject obj:
                    return obj.ToArray().GetTimeSpanList(allowInfinite);

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into TimeSpan[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetTimeSpanList(out IList<TimeSpan> result, bool allowInfinite = true)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetTimeSpanList(out result, allowInfinite);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetTimeSpanList(out result, allowInfinite))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of string values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of string values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<string> GetStringList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetStringList();

                case HoconObject obj:
                    return obj.ToArray().GetStringList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into string[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetStringList(out IList<string> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetStringList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetStringList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Retrieves a list of character values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of character values represented by this <see cref="HoconElement" />.</returns>
        public virtual IList<char> GetCharList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetCharList();

                case HoconObject obj:
                    return obj.ToArray().GetCharList();

                case HoconLiteral lit:
                    return lit.GetCharList();
            }

            // Should never reach this code
            throw new HoconException("Should never reach this code");
        }

        public virtual bool TryGetCharList(out IList<char> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetCharList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetCharList(out result))
                            return true;
                    return false;

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

        public virtual IList<HoconObject> GetObjectList()
        {
            switch (this)
            {
                case HoconArray arr:
                    return arr.GetObjectList();

                case HoconObject obj:
                    return obj.ToArray().GetObjectList();

                default:
                    throw new HoconException(
                        $"Can only convert positive integer keyed {nameof(HoconObject)} and {nameof(HoconArray)}" +
                        $" into string[], found {GetType()} instead.");
            }
        }

        public virtual bool TryGetObjectList(out IList<HoconObject> result)
        {
            result = default;
            switch (this)
            {
                case HoconArray arr:
                    return arr.TryGetObjectList(out result);

                case HoconObject obj:
                    if (obj.TryGetArray(out var oArr))
                        if (oArr.TryGetObjectList(out result))
                            return true;
                    return false;

                default:
                    return false;
            }
        }
        #endregion
        #endregion

        #region Object value getter

        #region Single values
        #region HoconObject
        public HoconObject GetObject(string path, HoconObject @default)
        {
            if (TryGetObject(path, out var result))
                return result;
            return @default;
        }

        public HoconObject GetObject(HoconPath path, HoconObject @default)
        {
            if (TryGetObject(path, out var result))
                return result;
            return @default;
        }

        public HoconObject GetObject(string path)
        {
            try
            {
                return this[path].ToObject();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public HoconObject GetObject(HoconPath path)
        {
            try
            {
                return this[path].ToObject();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetObject(string path, out HoconObject result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetObject(out result);

            result = default;
            return false;
        }

        public bool TryGetObject(HoconPath path, out HoconObject result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetObject(out result);

            result = default;
            return false;
        }
        #endregion

        #region String
        public string GetString(string path)
        {
            try
            {
                return this[path].GetString();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public string GetString(HoconPath path)
        {
            try
            {
                return this[path].GetString();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetString(string path, out string result)
        {
            if (TryGetValue(path, out var val))
                if(val is HoconLiteral value)
                    return value.TryGetString(out result);

            result = default;
            return false;
        }

        public bool TryGetString(HoconPath path, out string result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetString(out result);

            result = default;
            return false;
        }

        public string GetString(string path, string @default)
        {
            if (TryGetString(path, out var result))
                return result;
            return @default;
        }

        public string GetString(HoconPath path, string @default)
        {
            if (TryGetString(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Char
        public char GetChar(string path)
        {
            try
            {
                return this[path].GetChar();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public char GetChar(HoconPath path)
        {
            try
            {
                return this[path].GetChar();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetChar(string path, out char result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetChar(out result);

            result = default;
            return false;
        }

        public bool TryGetChar(HoconPath path, out char result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetChar(out result);

            result = default;
            return false;
        }

        public char GetChar(string path, char @default)
        {
            if (TryGetChar(path, out var result))
                return result;

            return @default;
        }

        public char GetChar(HoconPath path, char @default)
        {
            if (TryGetChar(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Boolean
        public bool GetBoolean(string path)
        {
            try
            {
                return this[path].GetBoolean();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool GetBoolean(HoconPath path)
        {
            try
            {
                return this[path].GetBoolean();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetBoolean(string path, out bool result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetBoolean(out result);

            result = default;
            return false;
        }

        public bool TryGetBoolean(HoconPath path, out bool result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetBoolean(out result);

            result = default;
            return false;
        }

        public bool GetBoolean(string path, bool @default)
        {
            if (TryGetBoolean(path, out var result))
                return result;

            return @default;
        }

        public bool GetBoolean(HoconPath path, bool @default)
        {
            if (TryGetBoolean(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Decimal
        public decimal GetDecimal(string path)
        {
            try
            {
                return this[path].GetDecimal();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public decimal GetDecimal(HoconPath path)
        {
            try
            {
                return this[path].GetDecimal();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetDecimal(string path, out decimal result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetDecimal(out result);

            result = default;
            return false;
        }

        public bool TryGetDecimal(HoconPath path, out decimal result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetDecimal(out result);

            result = default;
            return false;
        }

        public decimal GetDecimal(string path, decimal @default)
        {
            if (TryGetDecimal(path, out var result))
                return result;

            return @default;
        }

        public decimal GetDecimal(HoconPath path, decimal @default)
        {
            if (TryGetDecimal(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Float
        public float GetFloat(string path)
        {
            try
            {
                return this[path].GetFloat();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public float GetFloat(HoconPath path)
        {
            try
            {
                return this[path].GetFloat();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetFloat(string path, out float result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetFloat(out result);

            result = default;
            return false;
        }

        public bool TryGetFloat(HoconPath path, out float result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetFloat(out result);

            result = default;
            return false;
        }

        public float GetFloat(string path, float @default)
        {
            if (TryGetFloat(path, out var result))
                return result;

            return @default;
        }

        public float GetFloat(HoconPath path, float @default)
        {
            if (TryGetFloat(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Double
        public double GetDouble(string path)
        {
            try
            {
                return this[path].GetDouble();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public double GetDouble(HoconPath path)
        {
            try
            {
                return this[path].GetDouble();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetDouble(string path, out double result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetDouble(out result);

            result = default;
            return false;
        }

        public bool TryGetDouble(HoconPath path, out double result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetDouble(out result);

            result = default;
            return false;
        }

        public double GetDouble(string path, double @default)
        {
            if (TryGetDouble(path, out var result))
                return result;

            return @default;
        }

        public double GetDouble(HoconPath path, double @default)
        {
            if (TryGetDouble(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region BigInteger
        public BigInteger GetBigInteger(string path)
        {
            try
            {
                return this[path].GetBigInteger();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public BigInteger GetBigInteger(HoconPath path)
        {
            try
            {
                return this[path].GetBigInteger();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetBigInteger(string path, out BigInteger result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetBigInteger(out result);

            result = default;
            return false;
        }

        public bool TryGetBigInteger(HoconPath path, out BigInteger result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetBigInteger(out result);

            result = default;
            return false;
        }

        public BigInteger GetBigInteger(string path, BigInteger @default)
        {
            if (TryGetBigInteger(path, out var result))
                return result;

            return @default;
        }

        public BigInteger GetBigInteger(HoconPath path, BigInteger @default)
        {
            if (TryGetBigInteger(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region ULong
        public ulong GetULong(string path)
        {
            try
            {
                return this[path].GetULong();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public ulong GetULong(HoconPath path)
        {
            try
            {
                return this[path].GetULong();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetULong(string path, out ulong result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetULong(out result);

            result = default;
            return false;
        }

        public bool TryGetULong(HoconPath path, out ulong result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetULong(out result);

            result = default;
            return false;
        }

        public ulong GetULong(string path, ulong @default)
        {
            if (TryGetULong(path, out var result))
                return result;

            return @default;
        }

        public ulong GetULong(HoconPath path, ulong @default)
        {
            if (TryGetULong(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Long
        public long GetLong(string path)
        {
            try
            {
                return this[path].GetLong();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public long GetLong(HoconPath path)
        {
            try
            {
                return this[path].GetLong();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetLong(string path, out long result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetLong(out result);

            result = default;
            return false;
        }

        public bool TryGetLong(HoconPath path, out long result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetLong(out result);

            result = default;
            return false;
        }

        public long GetLong(string path, long @default)
        {
            if (TryGetLong(path, out var result))
                return result;

            return @default;
        }

        public long GetLong(HoconPath path, long @default)
        {
            if (TryGetLong(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region UInt
        public uint GetUInt(string path)
        {
            try
            {
                return this[path].GetUInt();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public uint GetUInt(HoconPath path)
        {
            try
            {
                return this[path].GetUInt();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetUInt(string path, out uint result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetUInt(out result);

            result = default;
            return false;
        }

        public bool TryGetUInt(HoconPath path, out uint result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetUInt(out result);

            result = default;
            return false;
        }

        public uint GetUInt(string path, uint @default)
        {
            if (TryGetUInt(path, out var result))
                return result;

            return @default;
        }

        public uint GetUInt(HoconPath path, uint @default)
        {
            if (TryGetUInt(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region GetInt
        public int GetInt(string path)
        {
            try
            {
                return this[path].GetInt();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public int GetInt(HoconPath path)
        {
            try
            {
                return this[path].GetInt();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetInt(string path, out int result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetInt(out result);

            result = default;
            return false;
        }

        public bool TryGetInt(HoconPath path, out int result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetInt(out result);

            result = default;
            return false;
        }

        public int GetInt(string path, int @default)
        {
            if (TryGetInt(path, out var result))
                return result;

            return @default;
        }

        public int GetInt(HoconPath path, int @default)
        {
            if (TryGetInt(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region UShort
        public ushort GetUShort(string path)
        {
            try
            {
                return this[path].GetUShort();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public ushort GetUShort(HoconPath path)
        {
            try
            {
                return this[path].GetUShort();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetUShort(string path, out ushort result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetUShort(out result);

            result = default;
            return false;
        }

        public bool TryGetUShort(HoconPath path, out ushort result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetUShort(out result);

            result = default;
            return false;
        }

        public ushort GetUShort(string path, ushort @default)
        {
            if (TryGetUShort(path, out var result))
                return result;

            return @default;
        }

        public ushort GetUShort(HoconPath path, ushort @default)
        {
            if (TryGetUShort(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Short
        public short GetShort(string path)
        {
            try
            {
                return this[path].GetShort();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public short GetShort(HoconPath path)
        {
            try
            {
                return this[path].GetShort();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetShort(string path, out short result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetShort(out result);

            result = default;
            return false;
        }

        public bool TryGetShort(HoconPath path, out short result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetShort(out result);

            result = default;
            return false;
        }

        public short GetShort(string path, short @default)
        {
            if (TryGetShort(path, out var result))
                return result;

            return @default;
        }

        public short GetShort(HoconPath path, short @default)
        {
            if (TryGetShort(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region SByte
        public sbyte GetSByte(string path)
        {
            try
            {
                return this[path].GetSByte();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public sbyte GetSByte(HoconPath path)
        {
            try
            {
                return this[path].GetSByte();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetSByte(string path, out sbyte result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetSByte(out result);

            result = default;
            return false;
        }

        public bool TryGetSByte(HoconPath path, out sbyte result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetSByte(out result);

            result = default;
            return false;
        }

        public sbyte GetSByte(string path, sbyte @default)
        {
            if (TryGetSByte(path, out var result))
                return result;

            return @default;
        }

        public sbyte GetSByte(HoconPath path, sbyte @default)
        {
            if (TryGetSByte(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Byte
        public byte GetByte(string path)
        {
            try
            {
                return this[path].GetByte();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public byte GetByte(HoconPath path)
        {
            try
            {
                return this[path].GetByte();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetByte(string path, out byte result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetByte(out result);

            result = default;
            return false;
        }

        public bool TryGetByte(HoconPath path, out byte result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetByte(out result);

            result = default;
            return false;
        }

        public byte GetByte(string path, byte @default)
        {
            if (TryGetByte(path, out var result))
                return result;

            return @default;
        }

        public byte GetByte(HoconPath path, byte @default)
        {
            if (TryGetByte(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region TimeSpan
        public TimeSpan GetTimeSpan(string path, bool allowInfinite = true)
        {
            try
            {
                return this[path].GetTimeSpan(allowInfinite);
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public TimeSpan GetTimeSpan(HoconPath path, bool allowInfinite = true)
        {
            try
            {
                return this[path].GetTimeSpan(allowInfinite);
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public TimeSpan GetTimeSpan(string path, TimeSpan? @default, bool allowInfinite = true)
        {
            if (TryGetTimeSpan(path, out var result, allowInfinite))
                return result;
            return @default.GetValueOrDefault();
        }

        public TimeSpan GetTimeSpan(HoconPath path, TimeSpan? @default, bool allowInfinite = true)
        {
            if (TryGetTimeSpan(path, out var result, allowInfinite))
                return result;
            return @default.GetValueOrDefault();
        }

        public bool TryGetTimeSpan(string path, out TimeSpan result, bool allowInfinite = true)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetTimeSpan(out result, allowInfinite);

            result = default;
            return false;
        }

        public bool TryGetTimeSpan(HoconPath path, out TimeSpan result, bool allowInfinite = true)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetTimeSpan(out result, allowInfinite);

            result = default;
            return false;
        }

        public TimeSpan GetByte(string path, TimeSpan @default, bool allowInfinite = true)
        {
            if (TryGetTimeSpan(path, out var result, allowInfinite))
                return result;

            return @default;
        }

        public TimeSpan GetByte(HoconPath path, TimeSpan @default, bool allowInfinite = true)
        {
            if (TryGetTimeSpan(path, out var result, allowInfinite))
                return result;

            return @default;
        }
        #endregion

        #region ByteSize
        public long? GetByteSize(string path)
        {
            try
            {
                return this[path].GetByteSize();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public long? GetByteSize(HoconPath path)
        {
            try
            {
                return this[path].GetByteSize();
            }
            catch (Exception ex)
            {
                throw new HoconValueException(ex.Message, path, ex);
            }
        }

        public bool TryGetByteSize(string path, out long? result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetByteSize(out result);

            result = default;
            return false;
        }

        public bool TryGetByteSize(HoconPath path, out long? result)
        {
            if (TryGetValue(path, out var val))
                if (val is HoconLiteral value)
                    return value.TryGetByteSize(out result);

            result = default;
            return false;
        }

        public long? GetByteSize(string path, long? @default)
        {
            if (TryGetByteSize(path, out var result))
                return result;

            return @default;
        }

        public long? GetByteSize(HoconPath path, long? @default)
        {
            if (TryGetByteSize(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #endregion

        #region Array values
        #region Boolean
        /// <summary>
        ///     Retrieves a list of boolean values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of boolean values represented by this <see cref="HoconElement" />.</returns>
        public IList<bool> GetBooleanList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetBooleanList());
        }

        public  IList<bool> GetBooleanList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetBooleanList());
        }

        public  bool TryGetBooleanList(string path, out IList<bool> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetBooleanList(out result);

            result = default;
            return false;
        }

        public  bool TryGetBooleanList(HoconPath path, out IList<bool> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetBooleanList(out result);

            result = default;
            return false;
        }

        public  IList<bool> GetBooleanList(string path, IList<bool> @default)
        {
            if (TryGetBooleanList(path, out var result))
                return result;
            return @default;
        }

        public  IList<bool> GetBooleanList(HoconPath path, IList<bool> @default)
        {
            if (TryGetBooleanList(path, out var result))
                return result;
            return @default;
        }
        #endregion

        #region Decimal
        public  IList<decimal> GetDecimalList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetDecimalList());
        }

        public  IList<decimal> GetDecimalList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetDecimalList());
        }

        public  bool TryGetDecimalList(string path, out IList<decimal> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetDecimalList(out result);

            result = default;
            return false;
        }

        public  bool TryGetDecimalList(HoconPath path, out IList<decimal> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetDecimalList(out result);

            result = default;
            return false;
        }

        public  IList<decimal> GetDecimalList(string path, IList<decimal> @default)
        {
            if (TryGetDecimalList(path, out var result))
                return result;

            return @default;
        }

        public  IList<decimal> GetDecimalList(HoconPath path, IList<decimal> @default)
        {
            if (TryGetDecimalList(path, out var result))
                return result;

            return @default;
        }
        #endregion

        #region Float
        public  IList<float> GetFloatList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetFloatList());
        }

        public  IList<float> GetFloatList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetFloatList());
        }

        public  IList<float> GetFloatList(string path, IList<float> @default)
        {
            if (TryGetFloatList(path, out var result))
                return result;

            return @default;
        }

        public  IList<float> GetFloatList(HoconPath path, IList<float> @default)
        {
            if (TryGetFloatList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetFloatList(string path, out IList<float> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetFloatList(out result);

            result = default;
            return false;
        }

        public  bool TryGetFloatList(HoconPath path, out IList<float> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetFloatList(out result);

            result = default;
            return false;
        }
        #endregion

        #region Double
        /// <summary>
        ///     Retrieves a list of double values from this <see cref="HoconElement" />.
        /// </summary>
        /// <returns>A list of double values represented by this <see cref="HoconElement" />.</returns>
        public  IList<double> GetDoubleList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetDoubleList());
        }

        public  IList<double> GetDoubleList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetDoubleList());
        }

        public  IList<double> GetDoubleList(string path, IList<double> @default)
        {
            if (TryGetDoubleList(path, out var result))
                return result;

            return @default;
        }

        public  IList<double> GetDoubleList(HoconPath path, IList<double> @default)
        {
            if (TryGetDoubleList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetDoubleList(string path, out IList<double> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetDoubleList(out result);

            result = default;
            return false;
        }

        public  bool TryGetDoubleList(HoconPath path, out IList<double> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetDoubleList(out result);

            result = default;
            return false;
        }
        #endregion

        #region SByte
        public  IList<sbyte> GetSByteList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetSByteList());
        }

        public  IList<sbyte> GetSByteList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetSByteList());
        }

        public  IList<sbyte> GetSByteList(string path, IList<sbyte> @default)
        {
            if (TryGetSByteList(path, out var result))
                return result;

            return @default;
        }

        public  IList<sbyte> GetSByteList(HoconPath path, IList<sbyte> @default)
        {
            if (TryGetSByteList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetSByteList(string path, out IList<sbyte> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetSByteList(out result);

            result = default;
            return false;
        }

        public  bool TryGetSByteList(HoconPath path, out IList<sbyte> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetSByteList(out result);

            result = default;
            return false;
        }
        #endregion

        #region Byte
        public  IList<byte> GetByteList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetByteList());
        }

        public  IList<byte> GetByteList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetByteList());
        }

        public  IList<byte> GetByteList(string path, IList<byte> @default)
        {
            if (TryGetByteList(path, out var result))
                return result;

            return @default;
        }

        public  IList<byte> GetByteList(HoconPath path, IList<byte> @default)
        {
            if (TryGetByteList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetByteList(string path, out IList<byte> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetByteList(out result);

            result = default;
            return false;
        }

        public  bool TryGetByteList(HoconPath path, out IList<byte> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetByteList(out result);

            result = default;
            return false;
        }
        #endregion

        #region Short
        public  IList<short> GetShortList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetShortList());
        }

        public  IList<short> GetShortList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetShortList());
        }

        public  IList<short> GetShortList(string path, IList<short> @default)
        {
            if (TryGetShortList(path, out var result))
                return result;

            return @default;
        }

        public  IList<short> GetShortList(HoconPath path, IList<short> @default)
        {
            if (TryGetShortList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetShortList(string path, out IList<short> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetShortList(out result);

            result = default;
            return false;
        }

        public  bool TryGetShortList(HoconPath path, out IList<short> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetShortList(out result);

            result = default;
            return false;
        }
        #endregion

        #region UShort
        public  IList<ushort> GetUShortList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetUShortList());
        }
        public  IList<ushort> GetUShortList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetUShortList());
        }

        public  IList<ushort> GetUShortList(string path, IList<ushort> @default)
        {
            if (TryGetUShortList(path, out var result))
                return result;

            return @default;
        }

        public  IList<ushort> GetUShortList(HoconPath path, IList<ushort> @default)
        {
            if (TryGetUShortList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetUShortList(string path, out IList<ushort> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetUShortList(out result);

            result = default;
            return false;
        }

        public  bool TryGetUShortList(HoconPath path, out IList<ushort> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetUShortList(out result);

            result = default;
            return false;
        }
        #endregion

        #region Int
        public  IList<int> GetIntList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetIntList());
        }

        public  IList<int> GetIntList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetIntList());
        }

        public  IList<int> GetIntList(string path, IList<int> @default)
        {
            if (TryGetIntList(path, out var result))
                return result;

            return @default;
        }

        public  IList<int> GetIntList(HoconPath path, IList<int> @default)
        {
            if (TryGetIntList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetIntList(string path, out IList<int> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetIntList(out result);

            result = default;
            return false;
        }

        public  bool TryGetIntList(HoconPath path, out IList<int> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetIntList(out result);

            result = default;
            return false;
        }
        #endregion

        #region UInt
        public  IList<uint> GetUIntList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetUIntList());
        }

        public  IList<uint> GetUIntList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetUIntList());
        }

        public  IList<uint> GetUIntList(string path, IList<uint> @default)
        {
            if (TryGetUIntList(path, out var result))
                return result;

            return @default;
        }

        public  IList<uint> GetUIntList(HoconPath path, IList<uint> @default)
        {
            if (TryGetUIntList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetUIntList(string path, out IList<uint> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetUIntList(out result);

            result = default;
            return false;
        }

        public  bool TryGetUIntList(HoconPath path, out IList<uint> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetUIntList(out result);

            result = default;
            return false;
        }
        #endregion

        #region Long
        public  IList<long> GetLongList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetLongList());
        }

        public  IList<long> GetLongList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetLongList());
        }

        public  IList<long> GetLongList(string path, IList<long> @default)
        {
            if (TryGetLongList(path, out var result))
                return result;

            return @default;
        }

        public  IList<long> GetLongList(HoconPath path, IList<long> @default)
        {
            if (TryGetLongList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetLongList(string path, out IList<long> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetLongList(out result);

            result = default;
            return false;
        }

        public  bool TryGetLongList(HoconPath path, out IList<long> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetLongList(out result);

            result = default;
            return false;
        }
        #endregion

        #region ULong
        public  IList<ulong> GetULongList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetULongList());
        }

        public  IList<ulong> GetULongList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetULongList());
        }

        public  IList<ulong> GetULongList(string path, IList<ulong> @default)
        {
            if (TryGetULongList(path, out var result))
                return result;

            return @default;
        }

        public  IList<ulong> GetULongList(HoconPath path, IList<ulong> @default)
        {
            if (TryGetULongList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetULongList(string path, out IList<ulong> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetULongList(out result);

            result = default;
            return false;
        }

        public  bool TryGetULongList(HoconPath path, out IList<ulong> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetULongList(out result);

            result = default;
            return false;
        }
        #endregion

        #region BigInteger
        public  IList<BigInteger> GetBigIntegerList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetBigIntegerList());
        }

        public  IList<BigInteger> GetBigIntegerList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetBigIntegerList());
        }

        public  IList<BigInteger> GetBigIntegerList(string path, IList<BigInteger> @default)
        {
            if (TryGetBigIntegerList(path, out var result))
                return result;

            return @default;
        }

        public  IList<BigInteger> GetBigIntegerList(HoconPath path, IList<BigInteger> @default)
        {
            if (TryGetBigIntegerList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetBigIntegerList(string path, out IList<BigInteger> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetBigIntegerList(out result);

            result = default;
            return false;
        }

        public  bool TryGetBigIntegerList(HoconPath path, out IList<BigInteger> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetBigIntegerList(out result);

            result = default;
            return false;
        }
        #endregion

        #region String
        public  IList<string> GetStringList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetStringList());
        }

        public  IList<string> GetStringList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetStringList());
        }

        public  IList<string> GetStringList(string path, IList<string> @default)
        {
            if (TryGetStringList(path, out var result))
                return result;

            return @default;
        }

        public  IList<string> GetStringList(HoconPath path, IList<string> @default)
        {
            if (TryGetStringList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetStringList(string path, out IList<string> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetStringList(out result);

            result = default;
            return false;
        }

        public  bool TryGetStringList(HoconPath path, out IList<string> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetStringList(out result);

            result = default;
            return false;
        }
        #endregion

        #region Char
        public  IList<char> GetCharList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetCharList());
        }

        public  IList<char> GetCharList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetCharList());
        }

        public  IList<char> GetCharList(string path, IList<char> @default = null)
        {
            if (TryGetCharList(path, out var result))
                return result;

            return @default;
        }

        public  IList<char> GetCharList(HoconPath path, IList<char> @default = null)
        {
            if (TryGetCharList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetCharList(string path, out IList<char> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetCharList(out result);

            result = default;
            return false;
        }

        public  bool TryGetCharList(HoconPath path, out IList<char> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetCharList(out result);

            result = default;
            return false;

        }
        #endregion

        #region HoconObject
        public  IList<HoconObject> GetObjectList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetObjectList());
        }

        public  IList<HoconObject> GetObjectList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetObjectList());
        }

        public  IList<HoconObject> GetObjectList(string path, IList<HoconObject> @default)
        {
            if (TryGetObjectList(path, out var result))
                return result;

            return @default;
        }

        public  IList<HoconObject> GetObjectList(HoconPath path, IList<HoconObject> @default = null)
        {
            if (TryGetObjectList(path, out var result))
                return result;

            return @default;
        }

        public  bool TryGetObjectList(string path, out IList<HoconObject> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetObjectList(out result);

            result = default;
            return false;

        }

        public  bool TryGetObjectList(HoconPath path, out IList<HoconObject> result)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetObjectList(out result);

            result = default;
            return false;

        }
        #endregion

        #region TimeSpan
        public  IList<TimeSpan> GetTimeSpanList(string path)
        {
            return WrapWithValueException(path, () => this[path].GetTimeSpanList());
        }

        public  IList<TimeSpan> GetTimeSpanList(HoconPath path)
        {
            return WrapWithValueException(path, () => this[path].GetTimeSpanList());
        }

        public  IList<TimeSpan> GetTimeSpanList(string path, bool allowInfinite = true, IList<TimeSpan> @default = null)
        {
            if (TryGetTimeSpanList(path, out var result, allowInfinite))
                return result;

            return @default;
        }

        public  IList<TimeSpan> GetTimeSpanList(HoconPath path, bool allowInfinite = true, IList<TimeSpan> @default = null)
        {
            if (TryGetTimeSpanList(path, out var result, allowInfinite))
                return result;

            return @default;
        }

        public  bool TryGetTimeSpanList(string path, out IList<TimeSpan> result, bool allowInfinite = true)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetTimeSpanList(out result, allowInfinite);

            result = default;
            return false;
        }

        public  bool TryGetTimeSpanList(HoconPath path, out IList<TimeSpan> result, bool allowInfinite = true)
        {
            if (TryGetValue(path, out var value))
                return value.TryGetTimeSpanList(out result, allowInfinite);

            result = default;
            return false;
        }
        #endregion
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