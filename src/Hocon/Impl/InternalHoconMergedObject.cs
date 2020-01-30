// -----------------------------------------------------------------------
// <copyright file="InternalHoconMergedObject.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    public sealed class InternalHoconMergedObject : InternalHoconObject
    {
        public InternalHoconMergedObject(IInternalHoconElement parent, List<InternalHoconObject> objects) : base(parent)
        {
            Objects = objects;
            foreach (var obj in Objects) base.Merge(obj);
        }

        public List<InternalHoconObject> Objects { get; }

        internal override InternalHoconField TraversePath(HoconPath relativePath)
        {
            var result = Objects.Last().TraversePath(relativePath);
            Clear();
            foreach (var obj in Objects) base.Merge(obj);

            return result;
        }

        internal override InternalHoconField GetOrCreateKey(string key)
        {
            var result = Objects.Last().GetOrCreateKey(key);
            Clear();
            foreach (var obj in Objects) base.Merge(obj);

            return result;
        }

        internal override void SetField(string key, InternalHoconField value)
        {
            Objects.Last().SetField(key, value);
            base.SetField(key, value);
        }

        public override void Merge(InternalHoconObject other)
        {
            var parent = (InternalHoconValue) Parent;
            parent.Add(other);

            Objects.Add(other);
            base.Merge(other);
        }
    }
}