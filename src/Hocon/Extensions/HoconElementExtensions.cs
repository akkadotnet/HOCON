// -----------------------------------------------------------------------
// <copyright file="HoconElementExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

namespace Hocon
{
    /// <summary>
    ///     HoconElementExtensions
    /// </summary>
    public static class HoconElementExtensions
    {
        public static bool IsLiteral(this HoconType hoconType)
        {
            switch (hoconType)
            {
                case HoconType.Boolean:
                case HoconType.Number:
                case HoconType.String:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMergeable(this HoconType t1, HoconType t2)
        {
            return t1 == t2 || t1.IsLiteral() && t2.IsLiteral();
        }

        public static bool IsMergeable(this IInternalHoconElement e1, IInternalHoconElement e2)
        {
            return e1.Type.IsMergeable(e2.Type);
        }
    }
}