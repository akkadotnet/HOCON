// -----------------------------------------------------------------------
// <copyright file="HoconImmutableArrayBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
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
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            foreach (var element in array) Add(element.ToHoconImmutable());
            return this;
        }

        internal HoconArrayBuilder AddRange(HoconValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            foreach (var element in value.GetArray()) Add(element.ToHoconImmutable());
            return this;
        }

        public HoconArray Build()
        {
            return HoconArray.Create(this);
        }
    }
}