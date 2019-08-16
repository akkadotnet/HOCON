using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hocon.Immutable.Extensions;

namespace Hocon.Immutable.Builder
{
    public sealed class HoconImmutableObjectBuilder:SortedDictionary<string, HoconImmutableElement>
    {
        public HoconImmutableObjectBuilder Merge(IDictionary<string, HoconImmutableElement> fields)
        {
            foreach (var kvp in fields)
            {
                this[kvp.Key] = kvp.Value;
            }
            return this;
        }

        public HoconImmutableObjectBuilder Merge(HoconObject @object)
        {
            foreach (var kvp in @object)
            {
                this[kvp.Key] = kvp.Value.ToHoconImmutable();
            }
            return this;
        }

        public HoconImmutableObject Build()
        {
            return HoconImmutableObject.Create(this);
        }
    }
}
