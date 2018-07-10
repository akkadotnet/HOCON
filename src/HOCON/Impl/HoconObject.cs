//-----------------------------------------------------------------------
// <copyright file="HoconObject.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Hocon
{
    /// <summary>
    /// This class represents an object element in a HOCON (Human-Optimized Config Object Notation)
    /// configuration string.
    /// <code>
    /// akka {  
    ///   actor {
    ///     debug {  
    ///       receive = on 
    ///       autoreceive = on
    ///       lifecycle = on
    ///       event-stream = on
    ///       unhandled = on
    ///     }
    ///   }
    /// }
    /// </code>
    /// </summary>
    public class HoconObject : Dictionary<string, HoconValue>, IHoconElement
    {
        public IHoconElement Parent { get; }

        /// <inheritdoc />
        /// <returns><see cref="HoconType.Object"/>.</returns>
        public HoconType Type => HoconType.Object;

        /// <summary>
        /// Retrieves the underlying map that contains the barebones
        /// object values.
        /// </summary>
        public IDictionary<string, object> Unwrapped 
            => this.ToDictionary(k => k.Key, v 
                => v.Value.Type == HoconType.Object ? (object)v.Value.GetObject().Unwrapped : v.Value);

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconObject"/> class.
        /// </summary>
        public HoconObject(IHoconElement parent)
        {
            Parent = parent;
        }

        /// <inheritdoc />
        public HoconObject GetObject() => this;

        /// <inheritdoc />
        /// <exception cref="HoconException">
        /// This element is an object, it is not a string, therefore this method will throw an exception.
        /// </exception>
        public string GetString()
            => throw new HoconException("Can not convert Hocon object into a string.");

        public string Raw
            => throw new HoconException("Can not convert Hocon object into a string.");

        /// <inheritdoc />
        /// <exception cref="HoconException">
        /// This element is an object, it is not an array, Therefore this method will throw an exception.
        /// </exception>
        public IList<HoconValue> GetArray()
            => throw new HoconException("Can not convert Hocon object into an array.");

        /// <summary>
        /// Retrieves the value associated with the supplied key.
        /// </summary>
        /// <param name="key">The key associated with the value to retrieve.</param>
        /// <returns>
        /// The value associated with the supplied key or null
        /// if they key does not exist.
        /// </returns>
        public HoconValue GetKey(string key)
        {
            if(TryGetValue(key, out var item))
                return item;
#if AKKA
            return null; // this is intentional, decided by AKKA devs
#else
            throw new HoconException($"Object does not contain a field with key `{key}`");
#endif
        }

        /// <summary>
        /// Retrieves the value associated with the supplied key.
        /// If the supplied key is not found, then one is created
        /// with a blank value.
        /// </summary>
        /// <param name="key">The key associated with the value to retrieve.</param>
        /// <returns>The value associated with the supplied key.</returns>
        internal HoconValue GetOrCreateKey(string key)
        {
            if (TryGetValue(key, out var child))
                return child;

            child = new HoconValue(this);
            Add(key, child);
            return child;
        }

        internal void TraversePath(HoconPath path, List<HoconValue> result)
        {
            var key = path[0];
            path.RemoveAt(0);

            if (!TryGetValue(key, out var child))
            {
                child = new HoconValue(this);
                this[key] = child;
            }

            result.Add(child);

            if (path.Count > 0)
            {
                HoconObject childObject;
                if (child.Type != HoconType.Object)
                {
                    childObject = new HoconObject(this);
                    if (child.IsSubstitution())
                        child.AppendValue(childObject);
                    else if (child.Type != HoconType.Object)
                        child.NewValue(childObject);
                }
                else
                {
                    childObject = child.GetObject();
                }

                childObject.TraversePath(path, result);
            }

            path.Insert(0, key);
        }

        /// <summary>
        /// Returns a HOCON string representation of this element.
        /// </summary>
        /// <returns>A HOCON string representation of this element.</returns>
        public override string ToString()
            => ToString(0, 2);

        /// <inheritdoc />
        public string ToString(int indent, int indentSize)
        {
            var i = new string(' ', indent * indentSize);
            var list = new List<string>();
            foreach (var field in this)
            {
                list.Add($"{i}{field.Key} : {field.Value.ToString(indent + 1, indentSize)}");
            }
            return string.Join($",{Environment.NewLine}", list);
        }

        public void Merge(HoconObject other)
        {
            var keys = other.Keys.ToArray();
            foreach (var key in keys)
            {
                if (ContainsKey(key))
                {
                    var thisItem = this[key];
                    var otherItem = other[key];
                    if (thisItem.Type == HoconType.Object && otherItem.Type == HoconType.Object)
                    {
                        thisItem.GetObject().Merge(otherItem.GetObject());
                        continue;
                    }
                }
                this[key] = other[key];
            }
        }

        internal void ResolveValue(HoconValue child)
        {
            if(child.Type != HoconType.Empty)
                return;

            foreach (var kvp in this.ToArray())
            {
                if (kvp.Value == child)
                {
                    Remove(kvp.Key);
                    return;
                }
            }
        }

        /// <inheritdoc />
        public IHoconElement Clone(IHoconElement newParent)
        {
            var clone = new HoconObject(newParent);
            foreach (var kvp in this)
            {
                clone[kvp.Key] = (HoconValue)kvp.Value.Clone(this);
            }
            return clone;
        }
    }
}

