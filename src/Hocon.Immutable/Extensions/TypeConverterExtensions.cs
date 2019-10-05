using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon.Immutable.Extensions
{
    public static class TypeConverterExtensions
    {
        public static HoconImmutableObject ToObject(this HoconImmutableElement element)
        {
            if (element is HoconImmutableObject obj)
                return obj;
            return null;
        }

        public static HoconImmutableArray ToArray(this HoconImmutableElement element)
        {
            switch (element)
            {
                case HoconImmutableObject obj:
                    return obj.ToArray();
                case HoconImmutableArray arr:
                    return arr;
                default:
                    return null;
            }
        }

        public static HoconImmutableLiteral ToLiteral(this HoconImmutableElement element)
        {
            if (element is HoconImmutableLiteral lit)
                return lit;
            return null;
        }
    }
}
