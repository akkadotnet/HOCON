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

        public override IList<IHoconElement> Values => new List<IHoconElement>().AsReadOnly();

        public override HoconObject GetObject() => new HoconObject(Parent);

        public override string GetString() => "";

        public override string Raw => "";

        public override IList<HoconValue> GetArray() => new List<HoconValue>();

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconEmptyValue(newParent);
        }
    }
}
