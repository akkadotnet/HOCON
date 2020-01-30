// -----------------------------------------------------------------------
// <copyright file="HoconArrayBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon.Builder
{
    public sealed class HoconArrayBuilder : List<HoconElement>
    {
        public new HoconArrayBuilder Add(HoconElement element)
        {
            base.Add(element);
            return this;
        }

        public new HoconArrayBuilder AddRange(IEnumerable<HoconElement> array)
        {
            if (array == null)
                return this;
            base.AddRange(array);
            return this;
        }

        public HoconArrayBuilder AddRange(InternalHoconArray array)
        {
            foreach (var element in array) Add(element.ToHoconImmutable());
            return this;
        }

        public HoconArrayBuilder AddRange(InternalHoconValue value)
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