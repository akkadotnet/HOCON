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
    internal static class HoconElementExtensions
    {
        internal static bool IsLiteral(this HoconType hoconType)
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

        internal static bool IsMergeable(this HoconType t1, HoconType t2)
        {
            return t1 == t2 || t1.IsLiteral() && t2.IsLiteral();
        }

        internal static bool IsMergeable(this IHoconElement e1, IHoconElement e2)
        {
            return e1.Type.IsMergeable(e2.Type);
        }
    }
}