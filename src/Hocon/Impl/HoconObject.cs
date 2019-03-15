//-----------------------------------------------------------------------
// <copyright file="HoconObject.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class HoconObject : Dictionary<string, HoconField>, IHoconElement
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
        public List<HoconValue> GetArray()
            => throw new HoconException("Can not convert Hocon object into an array.");

        /// <summary>
        /// Retrieves the <see cref="HoconField"/> field associated with the supplied <see cref="string"/> key.
        /// </summary>
        /// <param name="key">The <see cref="string"/> key associated with the field to retrieve.</param>
        /// <returns>
        /// The <see cref="HoconField"/> associated with the supplied key.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="HoconObject"/></exception>
        public HoconField GetField(string key)
        {
            if(key == null)
                throw new ArgumentNullException(nameof(key));

            if (!TryGetValue(key, out var item))
            {
                var p = Parent;
                while (p != null && !(p is HoconField))
                    p = p.Parent;

                throw new KeyNotFoundException($"Object does not contain a field with key `{key}`. " +
                                         $"Path: `{ (p != null ? $"{((HoconField)p).Path.Value}.{key}" : key) }`");
            }

            return item;
        }

        /// <summary>
        /// Retrieves the <see cref="HoconField"/> field associated with the supplied <see cref="string"/> key.
        /// </summary>
        /// <param name="key">The <see cref="string"/> key associated with the field to retrieve.</param>
        /// <param name="result">When this method returns, contains the <see cref="HoconField"/> 
        /// associated with the specified key, if the key is found; 
        /// otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="HoconObject"/> contains a field with the the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetField(string key, out HoconField result)
        {
            return TryGetValue(key, out result);
        }

        public HoconField GetField(HoconPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Count == 0)
                throw new ArgumentException("Path is empty.", nameof(path));

            var pathIndex = 0;
            var currentObject = this;
            while (true)
            {
                var key = path[pathIndex];

                if (!currentObject.TryGetValue(key, out var field))
                    throw new KeyNotFoundException($"Could not find field with key `{key}` at path `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                if (pathIndex >= path.Count - 1)
                    return field;

                if (field.Type != HoconType.Object)
                    throw new HoconException($"Invalid path, trying to access a key on a non object field. Path: `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                currentObject = field.GetObject();
                pathIndex = pathIndex + 1;
            }
        }

        public bool TryGetField(HoconPath path, out HoconField result)
        {
            result = null;
            if (path == null || path.Count == 0)
                return false;

            var pathIndex = 0;
            var currentObject = this;
            while (true)
            {
                var key = path[pathIndex];

                if (!currentObject.TryGetValue(key, out var field))
                    return false;

                if (pathIndex >= path.Count - 1)
                {
                    result = field;
                    return true;
                }

                if (field.Type != HoconType.Object)
                    return false;

                currentObject = field.GetObject();
                pathIndex = pathIndex + 1;
            }
        }

        /// <summary>
        /// Retrieves the merged <see cref="HoconObject"/> backing the <see cref="HoconField"/> field 
        /// associated with the supplied <see cref="string"/> key.
        /// </summary>
        /// <param name="key">The <see cref="string"/> key associated with the field to retrieve.</param>
        /// <returns>
        /// The <see cref="HoconObject"/> backing the <see cref="HoconField"/> field associated with the supplied key.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="HoconObject"/></exception>
        /// <exception cref="HoconException">The <see cref="HoconField.Type"/> is not of type <see cref="HoconType.Object"/></exception>
        public HoconObject GetObject(string key)
        {
            return GetField(key).GetObject();
        }

        /// <summary>
        /// Retrieves the merged <see cref="HoconObject"/> backing the <see cref="HoconField"/> field 
        /// associated with the supplied <see cref="string"/> key.
        /// </summary>
        /// <param name="key">The <see cref="string"/> key associated with the field to retrieve.</param>
        /// <param name="result">When this method returns, contains the backing <see cref="HoconObject"/>
        /// of the <see cref="HoconField"/> associated with the specified key, if the key is found; 
        /// otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="HoconObject"/> contains a <see cref="HoconField"/> field with the the specified key
        /// and the <see cref="HoconField.Type"/> is of type <see cref="HoconType.Object"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetObject(string key, out HoconObject result)
        {
            result = null;
            if (!TryGetField(key, out var field))
                return false;

            result = field.GetObject();
            return true;
        }

        /// <summary>
        /// Retrieves the backing <see cref="HoconValue"/> value of the <see cref="HoconField"/> associated with 
        /// the supplied <see cref="HoconPath"/> path, relative to this object.
        /// </summary>
        /// <param name="path">The relative <see cref="HoconPath"/> path associated with 
        /// the <see cref="HoconField"/> of the <see cref="HoconValue"/> value to retrieve.</param>
        /// <returns>
        /// The <see cref="HoconValue"/> value backing the <see cref="HoconField"/> field associated 
        /// with the supplied key.
        /// </returns>
        /// <exception cref="ArgumentNullException">path is null</exception>
        /// <exception cref="ArgumentException">path is empty</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="HoconObject"/></exception>
        public HoconValue GetValue(HoconPath path)
        {
            return GetField(path).Value;
        }

        /// <summary>
        /// Retrieves the backing <see cref="HoconValue"/> value of the <see cref="HoconField"/> associated with 
        /// the supplied <see cref="HoconPath"/> path, relative to this object.
        /// </summary>
        /// <param name="path">The relative <see cref="HoconPath"/> path associated with 
        /// the <see cref="HoconField"/> of the <see cref="HoconValue"/> value to retrieve.</param>
        /// <param name="result">When this method returns, contains the backing <see cref="HoconValue"/>
        /// of the <see cref="HoconField"/> associated with the specified <see cref="HoconPath"/> path, 
        /// if the path is resolveable; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="HoconObject"/> children contains a <see cref="HoconField"/> field resolveable
        /// with the the specified relative <see cref="HoconPath"/> path; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(HoconPath path, out HoconValue result)
        {
            result = null;
            if (!TryGetField(path, out var field))
                return false;
            result = field.Value;
            return true;
        }

        /// <summary>
        /// Retrieves the value associated with the supplied key.
        /// If the supplied key is not found, then one is created
        /// with a blank value.
        /// </summary>
        /// <param name="path">The path associated with the value to retrieve.</param>
        /// <returns>The value associated with the supplied key.</returns>
        private HoconField GetOrCreateKey(HoconPath path)
        {
            if (TryGetValue(path.Key, out var child))
                return child;

            child = new HoconField(path, this);
            Add(path.Key, child);
            return child;
        }

        internal List<HoconField> TraversePath(HoconPath path)
        {
            var result = new List<HoconField>();
            var pathLength = 1;
            var currentObject = this;
            while (true)
            {
                var child = currentObject.GetOrCreateKey(new HoconPath(path.GetRange(0, pathLength)));
                result.Add(child);

                pathLength++;
                if (pathLength > path.Count)
                    return result;

                child.EnsureFieldIsObject();

                // cannot use child.GetObject() because it would return a new merged object instance, which 
                // breaks autoref with the parent object in the previous loop
                currentObject = child.Value.GetObject();
            }
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
            var sb = new StringBuilder();
            foreach (var field in this)
            {
                sb.Append($"{i}{field.Key} : {field.Value.ToString(indent + 1, indentSize)},{Environment.NewLine}");
            }
            return sb.ToString(0, sb.Length - Environment.NewLine.Length - 1);
        }

        public virtual void Merge(HoconObject other)
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

        internal void ResolveValue(HoconField child)
        {
            if (child.Value.Count == 0)
            {
                if(child.HasOldValues)
                    child.RestoreOldValue();
                else
                    Remove(child.Key);
            }
        }

        /// <inheritdoc />
        public IHoconElement Clone(IHoconElement newParent)
        {
            var clone = new HoconObject(newParent);
            foreach (var kvp in this)
            {
                clone[kvp.Key] = (HoconField)kvp.Value.Clone(clone);
            }
            return clone;
        }

        public bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.Type != HoconType.Object) return false;
            return this.AsEnumerable().SequenceEqual(other.GetObject().AsEnumerable());
        }

        public override bool Equals(object obj)
        {
            return obj is IHoconElement element && Equals(element);
        }

        public override int GetHashCode()
        {
            const int modifier = 43;
            var result = 587;
            unchecked
            {
                foreach (var value in Values)
                {
                    result = result * modifier + value.GetHashCode();
                }
            }
            return result;
        }

        public static bool operator ==(HoconObject left, HoconObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconObject left, HoconObject right)
        {
            return !Equals(left, right);
        }
    }
}

