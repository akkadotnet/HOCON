// -----------------------------------------------------------------------
// <copyright file="HoconImmutableExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

namespace Hocon
{
    public static class HoconExtensions
    {
        internal static HoconObject ToHoconImmutable(this HoconRoot root)
        {
            return new HoconObjectBuilder(root.Value.GetObject()).Build();
        }

        internal static HoconElement ToHoconImmutable(this IHoconElement element)
        {
            switch (element)
            {
                case InternalHoconObject o:
                    return o.ToHoconImmutable();
                case InternalHoconArray a:
                    return a.ToHoconImmutable();
                case InternalHoconLiteral l:
                    return l.ToHoconImmutable();
                case HoconValue v:
                    return v.ToHoconImmutable();
                case HoconField f:
                    return f.ToHoconImmutable();
                default:
                    throw new HoconException($"Unknown Hocon element type:{element.GetType().Name}");
            }
        }

        internal static HoconElement ToHoconImmutable(this HoconValue value)
        {
            switch (value.Type)
            {
                case HoconType.Object:
                    return new HoconObjectBuilder(value.GetObject()).Build();
                case HoconType.Array:
                    return new HoconArrayBuilder()
                        .AddRange(value)
                        .Build();
                case HoconType.String:
                    return new HoconLiteralBuilder()
                        .Append(value)
                        .Build();
                case HoconType.Empty:
                    return HoconLiteral.Null;
                default:
                    // Should never reach this line.
                    throw new HoconException($"Unknown Hocon field type:{value.Type}");
            }
        }

        internal static HoconObject ToHoconImmutable(this InternalHoconObject @object)
        {
            return new HoconObjectBuilder(@object).Build();
        }

        internal static HoconElement ToHoconImmutable(this HoconField field)
        {
            return field.Value.ToHoconImmutable();
        }

        internal static HoconArray ToHoconImmutable(this InternalHoconArray array)
        {
            return new HoconArrayBuilder()
                .AddRange(array)
                .Build();
        }

        internal static HoconLiteral ToHoconImmutable(this InternalHoconLiteral literal)
        {
            return literal.LiteralType == HoconLiteralType.Null
                ? null
                : new HoconLiteralBuilder()
                    .Append(literal)
                    .Build();
        }
    }
}