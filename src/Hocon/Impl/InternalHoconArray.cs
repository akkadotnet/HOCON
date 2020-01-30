// -----------------------------------------------------------------------
// <copyright file="InternalHoconArray.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    /// <summary>
    ///     This class represents an array element in a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    ///     <code>
    /// root {
    ///     items = [
    ///       "1",
    ///       "2"]
    /// }
    /// </code>
    /// </summary>
    public sealed class InternalHoconArray : List<InternalHoconValue>, IInternalHoconElement
    {
        private HoconType _arrayType = HoconType.Empty;

        public InternalHoconArray(IInternalHoconElement parent)
        {
            Parent = parent;
        }

        public IInternalHoconElement Parent { get; }
        public HoconType Type => HoconType.Array;

        public InternalHoconObject GetObject()
        {
            throw new HoconException("Can not convert Hocon array into an object.");
        }

        /// <inheritdoc />
        /// <exception cref="HoconException">
        ///     This element is an array. It is not a string.
        ///     Therefore this method will throw an exception.
        /// </exception>
        public string GetString()
        {
            throw new HoconException("Can not convert Hocon array into a string.");
        }

        public string Raw
            => throw new HoconException("Can not convert Hocon array into a string.");

        /// <inheritdoc />
        public List<InternalHoconValue> GetArray()
        {
            return this;
        }

        public string ToString(int indent, int indentSize)
        {
            return $"[{string.Join(", ", this)}]";
        }

        public IInternalHoconElement Clone(IInternalHoconElement newParent)
        {
            var newArray = new InternalHoconArray(newParent);
            foreach (var value in this) newArray.Add(value.Clone(newArray) as InternalHoconValue);
            return newArray;
        }

        public bool Equals(IInternalHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is InternalHoconArray array && Equals(array);
        }

        public new void Add(InternalHoconValue value)
        {
            if (value.Type != HoconType.Empty)
            {
                if (_arrayType == HoconType.Empty)
                    _arrayType = value.Type;
                else if (!value.Type.IsMergeable(_arrayType))
                    throw new HoconException(
                        $"Array value must match the rest of the array type or empty. Array value type: {_arrayType}, inserted type: {value.Type}");
            }

            base.Add(value);
        }

        internal void ResolveValue(InternalHoconSubstitution sub)
        {
            var subValue = (InternalHoconValue) sub.Parent;
            if (sub.Type == HoconType.Empty)
            {
                subValue.Remove(sub);
                if (subValue.Count == 0)
                    Remove(subValue);
                return;
            }

            if (_arrayType != HoconType.Empty && sub.Type != _arrayType)
                throw HoconParserException.Create(sub, sub.Path,
                    $"Substitution value must match the rest of the field type or empty. Array value type: {_arrayType}, substitution type: {sub.Type}");
        }

        /// <summary>
        ///     Returns a HOCON string representation of this element.
        /// </summary>
        /// <returns>A HOCON string representation of this element.</returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        private bool Equals(InternalHoconArray other)
        {
            if (Count != other.Count) return false;
            for (var i = 0; i < Count; ++i)
                if (!Equals(this[i], other[i]))
                    return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IInternalHoconElement element && Equals(element);
        }

        public override int GetHashCode()
        {
            const int seed = 599;
            const int modifier = 37;

            unchecked
            {
                return this.Aggregate(seed, (current, item) => current * modifier + item.GetHashCode());
            }
        }

        public static bool operator ==(InternalHoconArray left, InternalHoconArray right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InternalHoconArray left, InternalHoconArray right)
        {
            return !Equals(left, right);
        }
    }
}