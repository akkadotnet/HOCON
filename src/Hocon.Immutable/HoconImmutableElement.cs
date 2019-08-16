//-----------------------------------------------------------------------
// <copyright file="HoconImmutableElement.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Numerics;

namespace Hocon.Immutable
{
    public abstract class HoconImmutableElement
    {
        [Obsolete("There is no need to use Value property with Hocon.Immutable, please remove it.")]
        public HoconImmutableElement Value => this;

        public HoconImmutableElement this[int index]
        {
            get
            {
                switch (this)
                {
                    case HoconImmutableObject obj:
                        return obj.ToArray()[index];
                    case HoconImmutableArray arr:
                        return arr[index];
                    default:
                        throw new HoconException($"Integer indexers only works on positive integer keyed {nameof(HoconImmutableObject)} or {nameof(HoconImmutableArray)}");
                }
            }
        }

        public HoconImmutableElement this[string path]
        {
            get
            {
                if (this is HoconImmutableObject obj)
                    return obj[path];

                throw new HoconException($"String path indexers only works on {nameof(HoconImmutableObject)}");
            }
        }

        #region Casting operators
        public static implicit operator bool(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to bool, found {element.GetType()} instead.");
        }

        public static implicit operator sbyte(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to sbyte, found {element.GetType()} instead.");
        }

        public static implicit operator byte(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to byte, found {element.GetType()} instead.");
        }

        public static implicit operator short(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to short, found {element.GetType()} instead.");
        }

        public static implicit operator ushort(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to ushort, found {element.GetType()} instead.");
        }

        public static implicit operator int(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to int, found {element.GetType()} instead.");
        }

        public static implicit operator uint(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to uint, found {element.GetType()} instead.");
        }

        public static implicit operator long(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to long, found {element.GetType()} instead.");
        }

        public static implicit operator ulong(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to ulong, found {element.GetType()} instead.");
        }

        public static implicit operator BigInteger(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to BigInteger, found {element.GetType()} instead.");
        }

        public static implicit operator float(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to float, found {element.GetType()} instead.");
        }

        public static implicit operator double(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to double, found {element.GetType()} instead.");
        }

        public static implicit operator decimal(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to decimal, found {element.GetType()} instead.");
        }

        public static implicit operator TimeSpan(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to TimeSpan, found {element.GetType()} instead.");
        }

        public static implicit operator string(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit.Value;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to string, found {element.GetType()} instead.");
        }

        public static implicit operator char(HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;

            throw new HoconException($"Can only convert {nameof(HoconImmutableLiteral)} type to string, found {element.GetType()} instead.");
        }

        public static implicit operator bool[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into bool[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator sbyte[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into sbyte[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator byte[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into byte[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator short[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into short[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator ushort[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into ushort[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator int[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into int[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator uint[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into uint[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator long[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into long[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator ulong[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into ulong[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator BigInteger[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into BigInteger[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator float[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into float[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator double[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into double[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator decimal[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into decimal[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator TimeSpan[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into TimeSpan[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator string[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                default:
                    throw new HoconException($"Can only convert positive integer keyed {nameof(HoconImmutableObject)} and {nameof(HoconImmutableArray)}" +
                                             $" into string[], found {element.GetType()} instead.");
            }
        }

        public static implicit operator char[] (HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableArray arr:
                    return arr;
                case HoconImmutableObject obj:
                    return obj;
                case HoconImmutableLiteral lit:
                    return lit;
            }
            // Should never reach this code
            throw new HoconException("Should never reach this code");
        }
        #endregion
    }
}
