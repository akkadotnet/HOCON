// -----------------------------------------------------------------------
// <copyright file="HoconArray.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Hocon.Extensions;

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
    public sealed class HoconArray : List<HoconValue>, IHoconElement
    {
        private HoconType _arrayType = HoconType.Empty;

        public HoconArray(IHoconElement parent)
        {
            Parent = parent;
        }

        public IHoconElement Parent { get; }
        public HoconType Type => HoconType.Array;

        public HoconObject GetObject()
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
        public IList<HoconValue> GetArray()
        {
            return this;
        }

        public string ToString(int indent, int indentSize)
        {
            return $"[{string.Join(", ", this)}]";
        }

        public IHoconElement Clone(IHoconElement newParent)
        {
            var newArray = new HoconArray(newParent);
            foreach (var value in this) newArray.Add(value.Clone(newArray) as HoconValue);
            return newArray;
        }

        public bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.Type != HoconType.Array) return false;
            if (other is HoconArray array) return Equals(array);
            return this.SequenceEqual(other.GetArray());
        }

        public new void Add(HoconValue value)
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

        internal void ResolveValue(HoconSubstitution sub)
        {
            var subValue = (HoconValue) sub.Parent;
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

        private bool Equals(HoconArray other)
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
            return obj is IHoconElement element && Equals(element);
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

        public static bool operator ==(HoconArray left, HoconArray right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconArray left, HoconArray right)
        {
            return !Equals(left, right);
        }
    }
}