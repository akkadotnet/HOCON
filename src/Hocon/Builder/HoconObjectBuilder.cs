// -----------------------------------------------------------------------
// <copyright file="HoconImmutableObjectBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon
{
    public sealed class HoconObjectBuilder : SortedDictionary<string, HoconElement>
    {
        public HoconObjectBuilder():base()
        { }

        public HoconObjectBuilder(HoconElement source)
        {
            if(!(source is HoconObject other))
                throw new HoconException($"Could not create HoconObject from {source.GetType()}");
            Merge(other);
        }

        public HoconObjectBuilder(Dictionary<string, HoconElement> source):base(source)
        { 

        }

        /*
        public HoconObjectBuilder(IReadOnlyDictionary<string, HoconElement> source)
        {
            Merge(source);
        }
        */

        internal HoconObjectBuilder(InternalHoconObject @object)
        {
            foreach (var kvp in @object) 
                this[kvp.Key] = kvp.Value.ToHoconImmutable();
        }

        public HoconObjectBuilder Merge(Dictionary<string, HoconElement> fields)
        {
            return InternalMerge(fields.GetEnumerator());
        }

        public HoconObjectBuilder Merge(IReadOnlyDictionary<string, HoconElement> fields)
        {
            return InternalMerge(fields.GetEnumerator());
        }

        public HoconObjectBuilder FallbackMerge(HoconObject @object)
        {
            return new HoconObjectBuilder(@object).InternalMerge(GetEnumerator());
        }

        private HoconObjectBuilder InternalMerge(IEnumerator<KeyValuePair<string, HoconElement>> kvps)
        {
            while(kvps.MoveNext())
            {
                var kvp = kvps.Current;
                var otherValue = kvp.Value;
                var key = kvp.Key;

                if (
                    !(otherValue is HoconObject otherObject) ||
                    !TryGetValue(kvp.Key, out var thisValue) ||
                    !(thisValue is HoconObject thisObject))
                {
                    this[key] = otherValue;
                    continue;
                }

                if (thisObject == otherObject)
                    continue;

                this[key] = new HoconObjectBuilder(thisObject).Merge(otherObject).Build();
            }

            return this;
        }

        public HoconObject Build()
        {
            return HoconObject.Create(this);
        }
    }
}