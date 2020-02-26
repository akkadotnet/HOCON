// -----------------------------------------------------------------------
// <copyright file="HoconImmutableArrayBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon
{
    public sealed class HoconArrayBuilder : List<HoconElement>
    {
        public HoconArrayBuilder():base()
        { }

        public HoconArrayBuilder(IEnumerable<HoconElement> collection):base(collection)
        { }

        public HoconArrayBuilder(int capacity):base(capacity)
        { }

        internal HoconArrayBuilder AddRange(InternalHoconArray array)
        {
            foreach (var element in array) Add(element.ToHoconImmutable());
            return this;
        }

        internal HoconArrayBuilder AddRange(HoconValue value)
        {
            foreach (var element in value.GetArray()) Add(element.ToHoconImmutable());
            return this;
        }

        public HoconArray Build()
        {
            return HoconArray.Create(this);
        }
    }
}