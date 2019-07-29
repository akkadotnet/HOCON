﻿//-----------------------------------------------------------------------
// <copyright file="HoconField.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hocon
{
    /// <summary>
    /// This class represents a key and value tuple representing a Hocon field.
    /// <code>
    /// root {
    ///     items = [
    ///       "1",
    ///       "2"]
    /// }
    /// </code>
    /// </summary>

    public sealed class HoconField:IHoconElement
    {
        private readonly List<HoconValue> _internalValues;

        /// <inheritdoc/>
        public IHoconElement Parent { get; }

        /// <inheritdoc/>
        public HoconType Type => Value == null ? HoconType.Empty : Value.Type;

        /// <inheritdoc/>
        public string Raw => Value.Raw;

        public HoconPath Path { get; }
        public string Key => Path.Key;

        internal HoconField ParentField
        {
            get
            {
                var p = Parent;
                while (p != null && !(p is HoconField))
                    p = p.Parent;
                return p as HoconField;
            }
        }

        /// <summary>
        /// Returns true if there are old values stored.
        /// </summary>
        internal bool HasOldValues => _internalValues.Count > 1;

        public HoconValue Value
        {
            get
            {
                switch (_internalValues.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return _internalValues[0];
                    default:
                        var lastValue = _internalValues.Last();
                        if (lastValue.Type != HoconType.Object)
                            return lastValue;

                        var objects = new List<HoconObject>();
                        foreach (var val in _internalValues)
                        {
                            if (val.Type != HoconType.Object)
                                objects.Clear();
                            else
                                objects.Add(val.GetObject());
                        }

                        var value = new HoconValue(this);
                        if (objects.Count == 1)
                        {
                            value.Add(objects[0]);
                        }
                        else
                        {
                            var resultObject = new HoconMergedObject(value, objects);
                            value.Add(resultObject);
                        }
                        return value;
                }
            }
        }

        public HoconField(HoconPath path, HoconObject parent)
        {
            if(path == null)
                throw new ArgumentNullException(nameof(path));

            Path = new HoconPath(path);
            Parent = parent;
            _internalValues = new List<HoconValue>();
        }

        internal void EnsureFieldIsObject()
        {
            if (Type == HoconType.Object) return;

            var v = new HoconValue(this);
            var o = new HoconObject(v);
            v.Add(o);
            _internalValues.Add(v);
        }

        internal List<HoconSubstitution> SetValue(HoconValue value)
        {
            var removedSubs = new List<HoconSubstitution>();

            if (_internalValues.Count == 0)
            {
                _internalValues.Add(value);
                return removedSubs;
            }

            switch (value.Type)
            {
                case HoconType.Array:
                case HoconType.Literal:
                    var subs = value.GetSubstitutions();
                    if (subs.All(sub => sub.Path != Path))
                    {
                        foreach (var item in _internalValues)
                        {
                            removedSubs.AddRange(item.GetSubstitutions());
                        }
                        _internalValues.Clear();
                    }
                    _internalValues.Add(value);
                    break;
                case HoconType.Object:
                    var lastValue = _internalValues.Last();
                    if (lastValue.Type != HoconType.Object)
                        _internalValues.Add(value);
                    else
                        lastValue.GetObject().Merge(value.GetObject());
                    break;
                default:
                    _internalValues.Add(value);
                    break;
            }
            return removedSubs;
        }

        internal void RestoreOldValue()
        {
            if(HasOldValues)
                _internalValues.RemoveAt(_internalValues.Count - 1);
        }

        internal HoconValue OlderValueThan(IHoconElement marker)
        {
            var objectList = new List<HoconObject>();
            var index = 0;
            while (index < _internalValues.Count)
            {
                var value = _internalValues[index];
                if (value.Any(v => ReferenceEquals(v, marker)))
                {
                    break;
                }

                switch (value.Type)
                {
                    case HoconType.Object:
                        objectList.Add(value.GetObject());
                        break;
                    case HoconType.Literal:
                    case HoconType.Array:
                        objectList.Clear();
                        break;
                }

                index++;
            }

            if(objectList.Count == 0)
                return index == 0 ? null : _internalValues[index - 1];

            var result = new HoconValue(null);
            var o = new HoconObject(result);
            result.Add(o);

            foreach (var obj in objectList)
            {
                o.Merge(obj);
            }

            return result;
        }

        public HoconObject GetObject()
        {
            List<HoconObject> objectList = new List<HoconObject>();
            foreach (var value in _internalValues)
            {
                switch (value.Type)
                {
                    case HoconType.Object:
                        objectList.Add(value.GetObject());
                        break;
                    case HoconType.Literal:
                    case HoconType.Array:
                        objectList.Clear();
                        break;
                }
            }

            switch (objectList.Count)
            {
                case 0:
                    return null;
                case 1:
                    return objectList[0];
                default:
                    return new HoconMergedObject(this, objectList);
            }
        }

        public string GetString()
            => Value.GetString();

        public List<HoconValue> GetArray()
            => Value.GetArray();

        internal void ResolveValue(HoconValue value)
        {
            if (value.Type != HoconType.Empty)
                return;
            ((HoconObject)Parent).ResolveValue(this);
        }

        public IHoconElement Clone(IHoconElement newParent)
        {
            var newField = new HoconField(Path, (HoconObject)newParent);
            newField._internalValues.AddRange(_internalValues);
            return newField;
        }

        public override string ToString()
            => ToString(0, 2);

        public string ToString(int indent, int indentSize)
            => Value.ToString(indent, indentSize);

        public bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is HoconField field && Path.Equals(field.Path) && Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is IHoconElement element && Equals(element);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Path.GetHashCode() + Value.GetHashCode();
            }
        }

        public static bool operator ==(HoconField left, HoconField right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconField left, HoconField right)
        {
            return !Equals(left, right);
        }
    }
}
