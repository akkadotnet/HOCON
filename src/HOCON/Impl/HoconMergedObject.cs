using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon.Impl
{
    public class HoconMergedObject : HoconObject
    {
        public HoconMergedObject(List<HoconObject> objects):base()
        {
            HoconObject.FloatingObjects.Remove(this);
            Unscope();

            foreach (var obj in objects)
            {
                Merge(obj);
            }
        }
    }
}
