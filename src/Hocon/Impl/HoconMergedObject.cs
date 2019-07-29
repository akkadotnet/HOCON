//-----------------------------------------------------------------------
// <copyright file="HoconMergedObject.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon
{
    public sealed class HoconMergedObject:HoconObject
    {
        public List<HoconObject> Objects { get; }

        public HoconMergedObject(IHoconElement parent, List<HoconObject> objects) : base(parent)
        {
            Objects = objects;
            foreach (var obj in Objects)
            {
                base.Merge(obj);
            }
        }

        internal override HoconField TraversePath(HoconPath relativePath)
        {
            var result = Objects.Last().TraversePath(relativePath);
            Clear();
            foreach (var obj in Objects)
            {
                base.Merge(obj);
            }

            return result;
        }

        internal override HoconField GetOrCreateKey(string key)
        {
            var result = Objects.Last().GetOrCreateKey(key);
            Clear();
            foreach (var obj in Objects)
            {
                base.Merge(obj);
            }

            return result;
        }

        internal override void SetField(string key, HoconField value)
        {
            Objects.Last().SetField(key, value);
            base.SetField(key, value);
        }

        public override void Merge(HoconObject other)
        {
            var parent = (HoconValue) Parent;
            parent.Add(other);

            Objects.Add(other);
            base.Merge(other);
        }
        
    }
}
