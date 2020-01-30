// -----------------------------------------------------------------------
// <copyright file="InternalHoconField.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    /// <summary>
    ///     This class represents a key and value tuple representing a Hocon field.
    ///     <code>
    /// root {
    ///     items = [
    ///       "1",
    ///       "2"]
    /// }
    /// </code>
    /// </summary>
    public sealed class InternalHoconField : IInternalHoconElement
    {
        private readonly List<InternalHoconValue> _internalValues = new List<InternalHoconValue>();

        public InternalHoconField(string key, InternalHoconObject parent)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            Path = new HoconPath(parent.Path) {key};
            Parent = parent;
        }

        public HoconPath Path { get; }
        public string Key => Path.Key;

        /// <summary>
        ///     Returns true if there are old values stored.
        /// </summary>
        internal bool HasOldValues => _internalValues.Count > 1;

        public InternalHoconValue Value
        {
            get
            {
                var lastValue = _internalValues.LastOrDefault();

                if (lastValue == null)
                    return null;

                if (lastValue.Type != HoconType.Object)
                    return lastValue;

                var filteredValues = new List<InternalHoconValue>();
                foreach (var value in _internalValues)
                    if (value.Type != HoconType.Object && value.Type != HoconType.Empty)
                        filteredValues.Clear();
                    else
                        filteredValues.Add(value);

                var returnValue = new InternalHoconValue(this);
                foreach (var value in filteredValues)
                    returnValue.AddRange(value);
                return returnValue;
            }
        }

        /// <inheritdoc />
        public IInternalHoconElement Parent { get; }

        /// <inheritdoc />
        public HoconType Type => Value == null ? HoconType.Empty : Value.Type;

        /// <inheritdoc />
        public string Raw => Value.Raw;

        /// <inheritdoc />
        public InternalHoconObject GetObject()
        {
            return Value.GetObject();
        }

        /// <inheritdoc />
        public string GetString()
        {
            return Value.GetString();
        }

        public List<InternalHoconValue> GetArray()
        {
            return Value.GetArray();
        }

        public IInternalHoconElement Clone(IInternalHoconElement newParent)
        {
            var newField = new InternalHoconField(Key, (InternalHoconObject) newParent);
            foreach (var internalValue in _internalValues) newField._internalValues.Add(internalValue.Clone(newField) as InternalHoconValue);
            return newField;
        }

        public string ToString(int indent, int indentSize)
        {
            return Value.ToString(indent, indentSize);
        }

        public bool Equals(IInternalHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is InternalHoconField field && Path.Equals(field.Path) && Value.Equals(other);
        }

        internal void EnsureFieldIsObject()
        {
            if (Type == HoconType.Object) return;

            var v = new InternalHoconValue(this);
            var o = new InternalHoconObject(v);
            v.Add(o);
            _internalValues.Add(v);
        }

        internal void SetValue(InternalHoconValue value)
        {
            if (value == null)
                return;

            if (value.Type != HoconType.Object)
                foreach (var item in _internalValues)
                {
                    var subs = item.GetSubstitutions();
                    var preservedSub = value.GetSubstitutions();
                    foreach (var sub in subs.Except(preservedSub)) sub.Removed = true;
                }

            _internalValues.Add(value);
        }

        internal void RestoreOldValue()
        {
            if (HasOldValues)
                _internalValues.RemoveAt(_internalValues.Count - 1);
        }

        internal InternalHoconValue OlderValueThan(IInternalHoconElement marker)
        {
            var filteredObjectValue = new List<InternalHoconValue>();
            var index = 0;
            while (index < _internalValues.Count)
            {
                var value = _internalValues[index];
                if (value.Any(v => ReferenceEquals(v, marker))) break;

                switch (value.Type)
                {
                    case HoconType.Object:
                        filteredObjectValue.Add(value);
                        break;
                    case HoconType.Boolean:
                    case HoconType.Number:
                    case HoconType.String:
                    case HoconType.Array:
                        filteredObjectValue.Clear();
                        break;
                }

                index++;
            }

            if (filteredObjectValue.Count == 0)
                return index == 0 ? null : _internalValues[index - 1];

            var result = new InternalHoconValue(this);
            foreach (var value in filteredObjectValue) result.AddRange(value);

            return result;
        }

        internal void ResolveValue(InternalHoconValue value)
        {
            if (value.Type != HoconType.Empty)
                return;
            ((InternalHoconObject) Parent).ResolveValue(this);
        }

        public override string ToString()
        {
            return ToString(0, 2);
        }

        public override bool Equals(object obj)
        {
            return obj is IInternalHoconElement element && Equals(element);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Path.GetHashCode() + Value.GetHashCode();
            }
        }

        public static bool operator ==(InternalHoconField left, InternalHoconField right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InternalHoconField left, InternalHoconField right)
        {
            return !Equals(left, right);
        }
    }
}