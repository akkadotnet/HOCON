using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon.Impl
{
    public class HoconMergedObject : HoconObject
    {
        public HoconMergedObject(IHoconElement owner, List<HoconObject> objects):base(owner)
        {
            foreach (var obj in objects)
            {
                Merge(obj);
            }
        }
    }
}
