// -----------------------------------------------------------------------
// <copyright file="GenericGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hocon
{
    public static class HoconGenericGetterExtensions
    {
        private static readonly Dictionary<Type, Func<HoconElement, object>> PrimitiveConverters =
            new Dictionary<Type, Func<HoconElement, object>>
            {
                {typeof(bool), e => e.GetBoolean()},
                {typeof(bool[]), e => e.GetBooleanList()},

                {typeof(char), e => e.GetChar()},
                {typeof(char[]), e => e.GetCharList()},

                {typeof(float), e => e.GetFloat()},
                {typeof(float[]), e => e.GetFloatList()},

                {typeof(double), e => e.GetDouble()},
                {typeof(double[]), e => e.GetDoubleList()},

                {typeof(decimal), e => e.GetDecimal()},
                {typeof(decimal[]), e => e.GetDecimalList()},

                {typeof(byte), e => e.GetByte()},
                {typeof(byte[]), e => e.GetByteList()},

                {typeof(sbyte), e => e.GetSByte()},
                {typeof(sbyte[]), e => e.GetSByteList()},

                {typeof(ushort), e => e.GetUShort()},
                {typeof(ushort[]), e => e.GetUShortList()},

                {typeof(short), e => e.GetShort()},
                {typeof(short[]), e => e.GetShortList()},

                {typeof(uint), e => e.GetUInt()},
                {typeof(uint[]), e => e.GetUIntList()},

                {typeof(int), e => e.GetInt()},
                {typeof(int[]), e => e.GetIntList()},

                {typeof(ulong), e => e.GetULong()},
                {typeof(ulong[]), e => e.GetULongList()},

                {typeof(long), e => e.GetLong()},
                {typeof(long[]), e => e.GetLongList()},

                {typeof(BigInteger), e => e.GetBigInteger()},
                {typeof(BigInteger[]), e => e.GetBigIntegerList()},

                {typeof(string), e => e.ToString()},
                {typeof(string[]), e => e.GetStringList()},

                {typeof(TimeSpan), e => e.GetTimeSpan()},
                {typeof(TimeSpan[]), e => e.GetTimeSpanList()}
            };

        public static T Get<T>(this HoconElement element)
        {
            return (T) element.Get(typeof(T));
        }

        public static object Get(this HoconElement element, Type type)
        {
            if (PrimitiveConverters.ContainsKey(type))
                return PrimitiveConverters[type].Invoke(element);

            throw new NotImplementedException();
        }
    }
}