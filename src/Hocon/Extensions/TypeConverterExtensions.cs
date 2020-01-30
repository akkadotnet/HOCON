// -----------------------------------------------------------------------
// <copyright file="TypeConverterExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

namespace Hocon
{
    public static class TypeConverterExtensions
    {
        public static HoconObject ToObject(this HoconElement element)
        {
            if (element is HoconObject obj)
                return obj;
            return null;
        }

        public static HoconArray ToArray(this HoconElement element)
        {
            switch (element)
            {
                case HoconObject obj:
                    return obj.ToArray();
                case HoconArray arr:
                    return arr;
                default:
                    return null;
            }
        }

        public static HoconLiteral ToLiteral(this HoconElement element)
        {
            if (element is HoconLiteral lit)
                return lit;
            return null;
        }
    }
}