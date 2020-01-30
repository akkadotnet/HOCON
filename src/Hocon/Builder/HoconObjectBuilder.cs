// -----------------------------------------------------------------------
// <copyright file="HoconObjectBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon.Builder
{
    public sealed class HoconObjectBuilder : Dictionary<string, HoconElement>
    {
        public HoconObjectBuilder()
        { }

        public HoconObjectBuilder(IDictionary<string, HoconElement> fields):base(fields)
        { }

        public HoconObjectBuilder(IReadOnlyDictionary<string, HoconElement> fields)
        {
            Merge(fields);
        }

        public HoconObjectBuilder Merge(IDictionary<string, HoconElement> fields)
        {
            foreach (var kvp in fields) this[kvp.Key] = kvp.Value;
            return this;
        }

        public HoconObjectBuilder Merge(IReadOnlyDictionary<string, HoconElement> fields)
        {
            foreach (var kvp in fields) this[kvp.Key] = kvp.Value;
            return this;
        }

        internal HoconObjectBuilder Merge(InternalHoconObject @object)
        {
            foreach (var kvp in @object) this[kvp.Key] = kvp.Value.ToHoconImmutable();
            return this;
        }

        public HoconObject Build()
        {
            return HoconObject.Create(this);
        }
    }
}