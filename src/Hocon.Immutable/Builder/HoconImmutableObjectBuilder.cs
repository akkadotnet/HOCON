// -----------------------------------------------------------------------
// <copyright file="HoconImmutableObjectBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Hocon.Immutable.Extensions;

namespace Hocon.Immutable.Builder
{
    public sealed class HoconImmutableObjectBuilder : SortedDictionary<string, HoconImmutableElement>
    {
        public HoconImmutableObjectBuilder Merge(IDictionary<string, HoconImmutableElement> fields)
        {
            foreach (var kvp in fields) this[kvp.Key] = kvp.Value;
            return this;
        }

        public HoconImmutableObjectBuilder Merge(HoconObject @object)
        {
            foreach (var kvp in @object) this[kvp.Key] = kvp.Value.ToHoconImmutable();
            return this;
        }

        public HoconImmutableObject Build()
        {
            return HoconImmutableObject.Create(this);
        }
    }
}