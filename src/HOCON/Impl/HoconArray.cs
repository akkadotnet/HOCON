//-----------------------------------------------------------------------
// <copyright file="HoconArray.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
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
        public IList<HoconValue> GetArray()
        {
            var result = new List<HoconValue>();
            foreach (var item in this)
            {
                switch (item) {
                    case HoconValue value:
                        result.Add(value);
                        break;
                    case HoconSubstitution sub:
                        result.AddRange(sub.ResolvedValue.Values.Cast<HoconValue>());
                        break;
                    default:
                        throw new HoconException("Unknown ");
                }
            }
            return result;
        }

        internal void ResolveValue(HoconSubstitution sub)
        {
            if (sub.Type == HoconType.Empty)
            {
                Remove(sub);
                return;
            }

            if (sub.Type != HoconType.Array)
            {
                throw HoconParserException.Create(sub, sub.Path,
                    $"Substitution value must match the rest of the field type or empty. Parent value type: {Type}, substitution type: {sub.Type}");
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
    }
}

