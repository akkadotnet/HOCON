//-----------------------------------------------------------------------
// <copyright file="HoconArray.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    /// <summary>
    /// This class represents an array element in a HOCON (Human-Optimized Config Object Notation)
    /// configuration string.
    /// <code>
    /// root {
    ///     items = [
    ///       "1",
    ///       "2"]
    /// }
    /// </code>
    /// </summary>
    public sealed class HoconArray : List<IHoconElement>, IHoconElement
    {
        private HoconType _arrayType = HoconType.Empty;

        public IHoconElement Parent { get; }
        public HoconType Type => HoconType.Array;

        public HoconArray(IHoconElement parent)
        {
            Parent = parent;
        }

        public HoconObject GetObject()
        {
            throw new HoconException("Can not convert Hocon array into an object.");
        }

        /// <inheritdoc />
        /// <exception cref="HoconException">
        /// This element is an array. It is not a string.
        /// Therefore this method will throw an exception.
        /// </exception>
        public string GetString()
        {
            throw new HoconException("Can not convert Hocon array into a string.");
        }

        public string Raw
            => throw new HoconException("Can not convert Hocon array into a string.");

        /// <inheritdoc />
        public List<HoconValue> GetArray()
        {
            var result = new List<HoconValue>();
            foreach (var item in this)
            {
                switch (item) {
                    case HoconValue value:
                        result.Add(value);
                        break;
                    case HoconObject obj:
                        var val = new HoconValue(this);
                        val.Add(obj);
                        result.Add(val);
                        break;
                    case HoconSubstitution sub:
                        if (sub.ResolvedValue != null)
                        {
                            if(sub.ResolvedValue.Type == HoconType.Array)
                                result.AddRange(sub.ResolvedValue.GetArray());
                            else
                                result.Add(sub.ResolvedValue);
                        }
                        break;
                    default:
                        throw new HoconException($"Unknown type: {item.Type}");
                }
            }
            return result;
        }

        public new void Add(IHoconElement element)
        {
            if (element.Type != HoconType.Empty)
            {
                if(_arrayType == HoconType.Empty)
                    _arrayType = element.Type;
                else if (element.Type != _arrayType)
                    throw new HoconException(
                        $"Array value must match the rest of the array type or empty. Array value type: {_arrayType}, inserted type: {element.Type}");
            }

            base.Add(element);
        }

        internal void ResolveValue(HoconSubstitution sub)
        {
            if (sub.Type == HoconType.Empty)
            {
                Remove(sub);
                return;
            }

            if (_arrayType != HoconType.Empty && sub.Type != _arrayType)
            {
                throw HoconParserException.Create(sub, sub.Path,
                    $"Substitution value must match the rest of the field type or empty. Array value type: {_arrayType}, substitution type: {sub.Type}");
            }
        }

        /// <summary>
        /// Returns a HOCON string representation of this element.
        /// </summary>
        /// <returns>A HOCON string representation of this element.</returns>
        public override string ToString()
            => ToString(0, 2);

        public string ToString(int indent, int indentSize)
        {
            return $"[{string.Join(", ", this)}]";
        }

        public IHoconElement Clone(IHoconElement newParent)
        {
            var clone = new HoconArray(newParent);
            clone.AddRange(this);
            return clone;
        }

        private bool Equals(HoconArray other)
        {
            if (Count != other.Count) return false;
            for(var i = 0; i < Count; ++i)
            {
                if (!Equals(this[i], other[i]))
                    return false;
            }
            return true;
        }

        public bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is HoconArray array && Equals(array);
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
                return this.Aggregate(seed, (current, item) => (current * modifier) + item.GetHashCode());
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

