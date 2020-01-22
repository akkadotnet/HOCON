// -----------------------------------------------------------------------
// <copyright file="TypeConverterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

namespace Hocon.Immutable.Extensions
{
    public static class TypeConverterExtensions
    {
        public static HoconImmutableObject ToObject(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj;
            return null;
        }

        public static HoconImmutableArray ToArray(this HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableObject obj:
                    return obj.ToArray();
                case HoconImmutableArray arr:
                    return arr;
                default:
                    return null;
            }
        }

        public static HoconImmutableLiteral ToLiteral(this HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;
            return null;
        }
    }
}