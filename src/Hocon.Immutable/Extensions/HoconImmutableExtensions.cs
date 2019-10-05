//-----------------------------------------------------------------------
// <copyright file="HoconImmutableExtensions.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Hocon.Immutable.Builder;

namespace Hocon.Immutable.Extensions
{
    public static class HoconImmutableExtensions
    {
        public static HoconImmutableObject ToHoconImmutable(this HoconRoot root)
        {
            return new HoconImmutableObjectBuilder()
                .Merge(root.Value.GetObject())
                .Build();
        }

        public static HoconImmutableElement ToHoconImmutable(this IHoconElement element)
        {
            switch (element)
            {
                case HoconObject o:
                    return o.ToHoconImmutable();
                case HoconArray a:
                    return a.ToHoconImmutable();
                case HoconLiteral l:
                    return l.ToHoconImmutable();
                case HoconValue v:
                    return v.ToHoconImmutable();
                case HoconField f:
                    return f.ToHoconImmutable();
                default:
                    throw new HoconException($"Unknown Hocon element type:{element.GetType().Name}");
            }
        }

        public static HoconImmutableElement ToHoconImmutable(this HoconValue value)
        {
            switch (value.Type)
            {
                case HoconType.Object:
                    return new HoconImmutableObjectBuilder()
                        .Merge(value.GetObject())
                        .Build();
                case HoconType.Array:
                    return new HoconImmutableArrayBuilder()
                        .AddRange(value)
                        .Build();
                case HoconType.Literal:
                    return new HoconImmutableLiteralBuilder()
                        .Append(value)
                        .Build();
                case HoconType.Empty:
                    return HoconImmutableLiteral.Null;
                default:
                    // Should never reach this line.
                    throw new HoconException($"Unknown Hocon field type:{value.Type}");
            }
        }

        public static HoconImmutableObject ToHoconImmutable(this HoconObject @object)
        {
            return new HoconImmutableObjectBuilder()
                .Merge(@object)
                .Build();
        }

        public static HoconImmutableElement ToHoconImmutable(this HoconField field)
        {
            return field.Value.ToHoconImmutable();
        }

        public static HoconImmutableArray ToHoconImmutable(this HoconArray array)
        {
            return new HoconImmutableArrayBuilder()
                .AddRange(array)
                .Build();
        }

        public static HoconImmutableLiteral ToHoconImmutable(this HoconLiteral literal)
        {
            return literal.LiteralType == HoconLiteralType.Null
                ? null
                : new HoconImmutableLiteralBuilder()
                    .Append(literal)
                    .Build();
        }
    }
}
