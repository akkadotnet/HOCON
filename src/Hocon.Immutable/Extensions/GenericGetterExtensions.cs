// -----------------------------------------------------------------------
// <copyright file="GenericGetterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hocon.Immutable.Extensions
{
    public static class GenericGetterExtensions
    {
        private static readonly Dictionary<Type, Func<HoconImmutableElement, object>> PrimitiveConverters =
            new Dictionary<Type, Func<HoconImmutableElement, object>>
            {
                {typeof(bool), e => e.GetBoolean()},
                {typeof(bool[]), e => e.ToArray<bool>()},

                {typeof(char), e => e.GetChar()},
                {typeof(char[]), e => e.ToCharArray()},

                {typeof(float), e => e.GetFloat()},
                {typeof(float[]), e => e.ToArray<float>()},

                {typeof(double), e => e.GetDouble()},
                {typeof(double[]), e => e.ToArray<double>()},

                {typeof(decimal), e => e.GetDecimal()},
                {typeof(decimal[]), e => e.ToArray<decimal>()},

                {typeof(byte), e => e.GetByte()},
                {typeof(byte[]), e => e.ToArray<byte>()},

                {typeof(sbyte), e => e.GetSByte()},
                {typeof(sbyte[]), e => e.ToArray<sbyte>()},

                {typeof(ushort), e => e.GetUShort()},
                {typeof(ushort[]), e => e.ToArray<ushort>()},

                {typeof(short), e => e.GetShort()},
                {typeof(short[]), e => e.ToArray<short>()},

                {typeof(uint), e => e.GetUInt()},
                {typeof(uint[]), e => e.ToArray<uint>()},

                {typeof(int), e => e.GetInt()},
                {typeof(int[]), e => e.ToArray<int>()},

                {typeof(ulong), e => e.GetULong()},
                {typeof(ulong[]), e => e.ToArray<ulong>()},

                {typeof(long), e => e.GetLong()},
                {typeof(long[]), e => e.ToArray<long>()},

                {typeof(BigInteger), e => e.GetBigInteger()},
                {typeof(BigInteger[]), e => e.ToArray<BigInteger>()},

                {typeof(string), e => e.GetString()},
                {typeof(string[]), e => e.ToArray<string>()},

                {typeof(TimeSpan), e => e.GetTimeSpan()},
                {typeof(TimeSpan[]), e => e.ToArray<TimeSpan>()}
            };

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
    }
}