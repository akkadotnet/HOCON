// -----------------------------------------------------------------------
// <copyright file="HoconEmptyValue.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon
{
    /// <summary>
    ///     This class represents an empty <see cref="HoconValue" />,
    ///     it masquerades as all other types and are usually used to represent empty or unresolved substitution.
    /// </summary>
    public sealed class HoconEmptyValue : HoconValue
    {
        public HoconEmptyValue() : base(null) { }
        
        public HoconEmptyValue(IHoconElement parent) : base(parent)
        {
        }

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

        public override HoconObject GetObject()
        {
            return new HoconObject(Parent);
        }

        public override string GetString()
        {
            return "";
        }

        public override IList<HoconValue> GetArray()
        {
            return new List<HoconValue>();
        }

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