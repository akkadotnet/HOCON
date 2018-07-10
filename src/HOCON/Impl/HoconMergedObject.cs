using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon
{
    public class HoconMergedObject:HoconObject
    {
        public IList<HoconObject> Objects { get; }

        public HoconMergedObject(IHoconElement parent, IList<HoconObject> objects) : base(parent)
        {
            Objects = objects;
            foreach (var obj in objects)
            {
                Merge(obj);
            }
        }
    }
}
