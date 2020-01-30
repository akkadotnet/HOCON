// -----------------------------------------------------------------------
// <copyright file="InternalHoconEmptyValue.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon
{
    /// <summary>
    ///     This class represents an empty <see cref="InternalHoconValue" />,
    ///     it masquerades as all other types and are usually used to represent empty or unresolved substitution.
    /// </summary>
    public sealed class InternalHoconEmptyValue : InternalHoconValue
    {
        public InternalHoconEmptyValue() : base(null) { }
        
        public InternalHoconEmptyValue(IInternalHoconElement parent) : base(parent)
        {
        }

        public override HoconType Type => HoconType.Empty;

        public override string Raw => "";

        public override void Add(IInternalHoconElement value)
        {
            throw new HoconException($"Can not add new value to {nameof(InternalHoconEmptyValue)}");
        }

        public override void AddRange(IEnumerable<IInternalHoconElement> values)
        {
            throw new HoconException($"Can not add new values to {nameof(InternalHoconEmptyValue)}");
        }

        public override InternalHoconObject GetObject()
        {
            return new InternalHoconObject(Parent);
        }

        public override string GetString()
        {
            return "";
        }

        public override List<InternalHoconValue> GetArray()
        {
            return new List<InternalHoconValue>();
        }

        public override bool Equals(IInternalHoconElement other)
        {
            if (other is null) return false;
            return other.Type == HoconType.Empty;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override IInternalHoconElement Clone(IInternalHoconElement newParent)
        {
            return new InternalHoconEmptyValue(newParent);
        }
    }
}