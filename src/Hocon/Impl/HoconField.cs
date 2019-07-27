//-----------------------------------------------------------------------
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
        private readonly List<HoconValue> _internalValues = new List<HoconValue>();

        /// <inheritdoc/>
        public IHoconElement Parent { get; }

        /// <inheritdoc/>
        public HoconType Type => Value == null ? HoconType.Empty : Value.Type;

        /// <inheritdoc/>
        public string Raw => Value.Raw;

        public HoconPath Path { get; }
        public string Key => Path.Key;

        /// <summary>
        /// Returns true if there are old values stored.
        /// </summary>
        internal bool HasOldValues => _internalValues.Count > 1;

        public HoconValue Value
        {
            get
            {
                var lastValue = _internalValues.LastOrDefault();

                if (lastValue == null)
                        return null;

                if (lastValue.Type != HoconType.Object)
                    return lastValue;

                var filteredValues = new List<HoconValue>();
                foreach (var value in _internalValues)
                {
                    if (value.Type != HoconType.Object && value.Type != HoconType.Empty)
                        filteredValues.Clear();
                    else
                        filteredValues.Add(value);
                }

                var returnValue = new HoconValue(this);
                foreach (var value in filteredValues)
                    returnValue.AddRange(value);
                return returnValue;
            }
        }

        public HoconField(string key, HoconObject parent)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            Path = new HoconPath(parent.Path) {key};
            Parent = parent;
        }

        internal void EnsureFieldIsObject()
        {
            if (Type == HoconType.Object) return;

            var v = new HoconValue(this);
            var o = new HoconObject(v);
            v.Add(o);
            _internalValues.Add(v);
        }

        internal void SetValue(HoconValue value)
        {
            if (value == null)
                return;

            if (value.Type == HoconType.Literal)
            {
                foreach (var item in _internalValues)
                {
                    var subs = item.GetSubstitutions();
                    foreach (var sub in subs)
                    {
                        sub.Removed = true;
                    }
                }
            }

            _internalValues.Add(value);
        }

        internal void RestoreOldValue()
        {
            if(HasOldValues)
                _internalValues.RemoveAt(_internalValues.Count - 1);
        }

        internal HoconValue OlderValueThan(IHoconElement marker)
        {
            var filteredObjectValue = new List<HoconValue>();
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
                        filteredObjectValue.Add(value);
                        break;
                    case HoconType.Literal:
                    case HoconType.Array:
                        filteredObjectValue.Clear();
                        break;
                }

                index++;
            }

            if(filteredObjectValue.Count == 0)
                return index == 0 ? null : _internalValues[index - 1];

            var result = new HoconValue(this);
            foreach (var value in filteredObjectValue)
            {
                result.AddRange(value);
            }

            return result;
        }

        public HoconObject GetObject()
        {
            return Value.GetObject();
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
            var newField = new HoconField(Key, (HoconObject)newParent);
            foreach (var internalValue in _internalValues)
            {
                newField._internalValues.Add(internalValue);
            }
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
