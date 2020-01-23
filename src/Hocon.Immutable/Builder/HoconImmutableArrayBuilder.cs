// -----------------------------------------------------------------------
// <copyright file="HoconImmutableArrayBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Hocon.Immutable.Extensions;

namespace Hocon.Immutable.Builder
{
    public sealed class HoconImmutableArrayBuilder : List<HoconImmutableElement>
    {
        public HoconImmutableArrayBuilder AddRange(HoconArray array)
        {
            foreach (var element in array) Add(element.ToHoconImmutable());
            return this;
        }

        public HoconImmutableArrayBuilder AddRange(HoconValue value)
        {
            foreach (var element in value.GetArray()) Add(element.ToHoconImmutable());
            return this;
        }

        public HoconImmutableArray Build()
        {
            return HoconImmutableArray.Create(this);
        }
    }
}