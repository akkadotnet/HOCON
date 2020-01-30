// -----------------------------------------------------------------------
// <copyright file="HoconImmutableExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using Hocon.Builder;

namespace Hocon.Extensions
{
    public static class HoconImmutableExtensions
    {
        public static HoconObject ToHoconImmutable(this HoconRoot root)
        {
            return new HoconObjectBuilder()
                .Merge(root.Value.GetObject())
                .Build();
        }

        public static HoconElement ToHoconImmutable(this IInternalHoconElement element)
        {
            switch (element)
            {
                case InternalHoconObject o:
                    return o.ToHoconImmutable();
                case InternalHoconArray a:
                    return a.ToHoconImmutable();
                case InternalHoconLiteral l:
                    return l.ToHoconImmutable();
                case InternalHoconValue v:
                    return v.ToHoconImmutable();
                case InternalHoconField f:
                    return f.ToHoconImmutable();
                default:
                    throw new HoconException($"Unknown Hocon element type:{element.GetType().Name}");
            }
        }

        public static HoconElement ToHoconImmutable(this InternalHoconValue value)
        {
            switch (value.Type)
            {
                case HoconType.Object:
                    return new HoconObjectBuilder()
                        .Merge(value.GetObject())
                        .Build();
                case HoconType.Array:
                    return new HoconArrayBuilder()
                        .AddRange(value)
                        .Build();
                case HoconType.Boolean:
                case HoconType.Number:
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

        public static HoconObject ToHoconImmutable(this InternalHoconObject @object)
        {
            return new HoconObjectBuilder()
                .Merge(@object)
                .Build();
        }

        public static HoconElement ToHoconImmutable(this InternalHoconField field)
        {
            return field.Value.ToHoconImmutable();
        }

        public static HoconArray ToHoconImmutable(this InternalHoconArray array)
        {
            return new HoconArrayBuilder()
                .AddRange(array)
                .Build();
        }

        public static HoconLiteral ToHoconImmutable(this InternalHoconLiteral literal)
        {
            return literal.LiteralType == HoconLiteralType.Null
                ? null
                : new HoconLiteralBuilder()
                    .Append(literal)
                    .Build();
        }
    }
}