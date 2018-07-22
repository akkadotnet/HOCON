//-----------------------------------------------------------------------
// <copyright file="HoconValue.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon
{
    /// <summary>
    /// This class represents an empty <see cref="HoconValue"/>, 
    /// it masquerades as all other types and are usually used to represent empty or unresolved substitution.
    /// </summary>
    public sealed class HoconEmptyValue:HoconValue
    {
        public HoconEmptyValue(IHoconElement parent):base(parent) { }

        public override HoconType Type => HoconType.Empty;

        public override string Raw => "";

        public override void Add(IHoconElement value)
        {
            throw new HoconException($"Can not add new value to {nameof(HoconEmptyValue)}");
        }

        public override void AddRange(IEnumerable<IHoconElement> values)
        {
            throw new HoconException($"Can not add new values to {nameof(HoconEmptyValue)}");
        }

        public override HoconObject GetObject() => new HoconObject(Parent);

        public override string GetString() => "";

        public override List<HoconValue> GetArray() => new List<HoconValue>();

        public override bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            return other.Type == HoconType.Empty;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconEmptyValue(newParent);
        }
    }
}
