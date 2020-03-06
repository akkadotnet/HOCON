// -----------------------------------------------------------------------
// <copyright file="HoconImmutableObjectBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
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
        { }

        internal HoconObjectBuilder(InternalHoconObject @object)
        {
            foreach (var kvp in @object) 
                this[kvp.Key] = kvp.Value.ToHoconImmutable();
        }

        public HoconObjectBuilder Merge(HoconObject other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return InternalMerge(other.GetEnumerator());
        }

        public HoconObjectBuilder Merge(Dictionary<string, HoconElement> fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            return InternalMerge(fields.GetEnumerator());
        }

        public HoconObjectBuilder Merge(IReadOnlyDictionary<string, HoconElement> fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            return InternalMerge(fields.GetEnumerator());
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

        public new HoconObjectBuilder Add(string key, HoconElement value)
        {
            base.Add(key, value);
            return this;
        }

        public new HoconObjectBuilder Clear()
        {
            base.Clear();
            return this;
        }

        public new HoconObjectBuilder Remove(string key)
        {
            base.Remove(key);
            return this;
        }
    }
}