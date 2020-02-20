// -----------------------------------------------------------------------
// <copyright file="HoconObject.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hocon.Extensions;

namespace Hocon
{
    /// <summary>
    ///     This class represents an object element in a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    ///     <code>
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
    internal class InternalHoconObject : Dictionary<string, HoconField>, IHoconElement
    {
        private static readonly InternalHoconObject _empty;
        public static InternalHoconObject Empty => _empty;

        static InternalHoconObject()
        {
            var value = new HoconValue(null);
            _empty = new InternalHoconObject(value);
        }

        [Obsolete("Only used for serialization", true)]
        private InternalHoconObject()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InternalHoconObject" /> class.
        /// </summary>
        public InternalHoconObject(IHoconElement parent)
        {
            if (!(parent is HoconValue))
                throw new HoconException("HoconObject parent can only be a HoconValue.");

            Parent = parent;
        }

        public new HoconField this[string key]
            => GetField(key);

        public HoconField this[HoconPath path]
            => GetField(path);

        public HoconPath Path
        {
            get
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent is HoconField field) return field.Path;

                    parent = parent.Parent;
                }

                return HoconPath.Empty;
            }
        }

        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Retrieves the underlying map that contains the barebones
        ///     object values.
        /// </summary>
        public IDictionary<string, object> Unwrapped
            => this.ToDictionary(k => k.Key, v
                => v.Value.Type == HoconType.Object ? (object) v.Value.GetObject().Unwrapped : v.Value);

        public IHoconElement Parent { get; }

        /// <inheritdoc />
        /// <returns><see cref="HoconType.Object" />.</returns>
        public HoconType Type => HoconType.Object;

        /// <inheritdoc />
        /// <summary>
        ///     Retrieves a list of elements associated with this element.
        /// </summary>
        public InternalHoconObject GetObject()
        {
            return this;
        }

        /// <inheritdoc />
        /// <exception cref="HoconException">
        ///     This element is an object, it is not a string, therefore this method will throw an exception.
        /// </exception>
        public string GetString()
        {
            throw new HoconException("Can not convert Hocon object into a string.");
        }

        public string Raw
            => throw new HoconException("Can not convert Hocon object into a string.");

        /// <inheritdoc />
        /// <summary>
        ///     Converts a numerically indexed object into an array where its elements are sorted
        ///     based on the numerically sorted order of the key.
        /// </summary>
        public IList<HoconValue> GetArray()
        {
            var sortedDict = new SortedDictionary<int, HoconValue>();
            var type = HoconType.Empty;
            foreach (var field in Values)
            {
                if (field.Value == null || !int.TryParse(field.Key, out var index) || index < 0)
                    continue;
                if (type == HoconType.Empty)
                    type = field.Type;
                else if (type != field.Type)
                    throw new HoconException(
                        $"Array element mismatch. Expected: {type} Found: {field.Type} Path:{Path}");

                sortedDict[index] = field.Value;
            }

            if (sortedDict.Count == 0)
                throw new HoconException(
                    "Object is empty, does not contain any numerically indexed fields, or contains only non-positive integer indices");

            return sortedDict.Values.ToList();
        }

        public bool TryGetArray(out List<HoconValue> result)
        {
            result = new List<HoconValue>();

            var sortedDict = new SortedDictionary<int, HoconValue>();
            var type = HoconType.Empty;
            foreach (var field in Values)
            {
                if (field.Value == null || !int.TryParse(field.Key, out var index) || index < 0)
                    continue;
                if (type == HoconType.Empty)
                    type = field.Type;
                else if (type != field.Type)
                    return false;

                sortedDict[index] = field.Value;
            }

            if (sortedDict.Count == 0)
                return false;

            result = sortedDict.Values.ToList();
            return true;
        }

        /// <inheritdoc />
        public string ToString(int indent, int indentSize)
        {
            var i = new string(' ', indent * indentSize);
            var sb = new StringBuilder();
            foreach (var field in this)
                sb.Append($"{i}{field.Key} : {field.Value.ToString(indent + 1, indentSize)},{Environment.NewLine}");
            return sb.Length > 2 ? sb.ToString(0, sb.Length - Environment.NewLine.Length - 1) : sb.ToString();
        }

        /// <inheritdoc />
        public IHoconElement Clone(IHoconElement newParent)
        {
            var clone = new InternalHoconObject(newParent);
            foreach (var kvp in this) clone.SetField(kvp.Key, kvp.Value.Clone(clone) as HoconField);
            return clone;
        }

        /// <summary>
        ///     Retrieves the <see cref="HoconField" /> field associated with the supplied <see cref="string" /> key.
        /// </summary>
        /// <param name="key">The <see cref="string" /> key associated with the field to retrieve.</param>
        /// <returns>
        ///     The <see cref="HoconField" /> associated with the supplied key.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="InternalHoconObject" /></exception>
        public HoconField GetField(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            var path = HoconPath.Parse(key);
            return GetField(path);
        }

        /// <summary>
        ///     Retrieves the <see cref="HoconField" /> field associated with the supplied <see cref="string" /> key.
        /// </summary>
        /// <param name="key">The <see cref="string" /> key associated with the field to retrieve.</param>
        /// <param name="result">
        ///     When this method returns, contains the <see cref="HoconField" />
        ///     associated with the specified key, if the key is found;
        ///     otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="InternalHoconObject" /> contains a field with the the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetField(string key, out HoconField result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            var path = HoconPath.Parse(key);
            return TryGetField(path, out result);
        }

        public HoconField GetField(HoconPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Count == 0)
                throw new ArgumentException("Path is empty.", nameof(path));

            var currentObject = this;

            var pathIndex = 0;
            while (true)
            {
                var key = path[pathIndex];

                if (!currentObject.TryGetValue(key, out var field))
                    throw new KeyNotFoundException(
                        $"Could not find field with key `{key}` at path `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                if (pathIndex >= path.Count - 1)
                    return field;

                if (field.Type != HoconType.Object)
                    throw new HoconException(
                        $"Invalid path, trying to access a key on a non object field. Path: `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                currentObject = field.GetObject();
                pathIndex++;
            }
        }

        internal virtual void SetField(string key, HoconField value)
        {
            base[key] = value;
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
                pathIndex++;
            }
        }

        /// <summary>
        ///     Retrieves the merged <see cref="InternalHoconObject" /> backing the <see cref="HoconField" /> field
        ///     associated with the supplied <see cref="string" /> key.
        /// </summary>
        /// <param name="key">The <see cref="string" /> key associated with the field to retrieve.</param>
        /// <returns>
        ///     The <see cref="InternalHoconObject" /> backing the <see cref="HoconField" /> field associated with the supplied key.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="InternalHoconObject" /></exception>
        /// <exception cref="HoconException">
        ///     The <see cref="HoconField.Type" /> is not of type <see cref="HoconType.Object" />
        /// </exception>
        public InternalHoconObject GetObject(string key)
        {
            return GetField(key).GetObject();
        }

        /// <summary>
        ///     Retrieves the merged <see cref="InternalHoconObject" /> backing the <see cref="HoconField" /> field
        ///     associated with the supplied <see cref="string" /> key.
        /// </summary>
        /// <param name="key">The <see cref="string" /> key associated with the field to retrieve.</param>
        /// <param name="result">
        ///     When this method returns, contains the backing <see cref="InternalHoconObject" />
        ///     of the <see cref="HoconField" /> associated with the specified key, if the key is found;
        ///     otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="InternalHoconObject" /> contains a <see cref="HoconField" /> field with the the specified key
        ///     and the <see cref="HoconField.Type" /> is of type <see cref="HoconType.Object" />; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetObject(string key, out InternalHoconObject result)
        {
            result = null;
            if (!TryGetField(key, out var field))
                return false;

            result = field.GetObject();
            return true;
        }

        /// <summary>
        ///     Retrieves the backing <see cref="HoconValue" /> value of the <see cref="HoconField" /> associated with
        ///     the supplied <see cref="HoconPath" /> path, relative to this object.
        /// </summary>
        /// <param name="path">
        ///     The relative <see cref="HoconPath" /> path associated with
        ///     the <see cref="HoconField" /> of the <see cref="HoconValue" /> value to retrieve.
        /// </param>
        /// <returns>
        ///     The <see cref="HoconValue" /> value backing the <see cref="HoconField" /> field associated
        ///     with the supplied key.
        /// </returns>
        /// <exception cref="ArgumentNullException">path is null</exception>
        /// <exception cref="ArgumentException">path is empty</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="InternalHoconObject" /></exception>
        public HoconValue GetValue(HoconPath path)
        {
            return GetField(path).Value;
        }

        /// <summary>
        ///     Retrieves the backing <see cref="HoconValue" /> value of the <see cref="HoconField" /> associated with
        ///     the supplied <see cref="HoconPath" /> path, relative to this object.
        /// </summary>
        /// <param name="path">
        ///     The relative <see cref="HoconPath" /> path associated with
        ///     the <see cref="HoconField" /> of the <see cref="HoconValue" /> value to retrieve.
        /// </param>
        /// <param name="result">
        ///     When this method returns, contains the backing <see cref="HoconValue" />
        ///     of the <see cref="HoconField" /> associated with the specified <see cref="HoconPath" /> path,
        ///     if the path is resolveable; otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="InternalHoconObject" /> children contains a <see cref="HoconField" /> field resolveable
        ///     with the the specified relative <see cref="HoconPath" /> path; otherwise, <c>false</c>.
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
        ///     Retrieves the value associated with the supplied key.
        ///     If the supplied key is not found, then one is created
        ///     with a blank value.
        /// </summary>
        /// <param name="key">The path associated with the value to retrieve.</param>
        /// <returns>The value associated with the supplied key.</returns>
        internal virtual HoconField GetOrCreateKey(string key)
        {
            if (TryGetValue(key, out var child))
                return child;

            child = new HoconField(key, this);
            Add(key, child);
            return child;
        }

        internal virtual HoconField TraversePath(HoconPath relativePath)
        {
            var currentObject = this;
            var index = 0;
            while (true)
            {
                var child = currentObject.GetOrCreateKey(relativePath[index]);
                index++;
                if (index > relativePath.Count - 1)
                    return child;
                child.EnsureFieldIsObject();
                currentObject = child.Value.GetObject();
            }
        }

        /// <summary>
        ///     Returns a HOCON string representation of this element.
        /// </summary>
        /// <returns>A HOCON string representation of this element.</returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        public virtual void Merge(InternalHoconObject other)
        {
            var keys = other.Keys.ToArray();
            foreach (var key in keys)
            {
                var quotedKey = $"\"{key}\"";
                if (!ContainsKey(key))
                {
                    base[key] = other[quotedKey];
                    continue;
                }

                var thisItem = this[quotedKey];
                var otherItem = other[quotedKey].Value;
                if (thisItem.Type == HoconType.Object && otherItem.Type == HoconType.Object)
                    thisItem.GetObject().Merge(otherItem.GetObject());
                else
                    thisItem.SetValue(otherItem);
            }
        }

        public void FallbackMerge(InternalHoconObject other)
        {
            foreach(var kvp in other)
            {
                var path = kvp.Value.Path;
                if (!TryGetValue(path, out _))
                {
                    var currentObject = this;
                    HoconField newField = null;
                    foreach (var key in path)
                    {
                        newField = currentObject.GetOrCreateKey(key);
                        if (newField.Type == HoconType.Empty)
                        {
                            var emptyValue = new HoconValue(newField);
                            emptyValue.Add(new InternalHoconObject(emptyValue));
                            newField.SetValue(emptyValue);
                        }
                        currentObject = newField.GetObject();
                    }
                    newField.SetValue(kvp.Value.Value);
                } else
                {
                    if(kvp.Value.Type == HoconType.Object)
                        FallbackMerge(kvp.Value.GetObject());
                }
            }
        }

        internal void ResolveValue(HoconField child)
        {
            if (child.Type == HoconType.Empty)
            {
                if (child.HasOldValues)
                    child.RestoreOldValue();
                else
                    Remove(child.Key);
            }
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
                foreach (var value in Values) result = result * modifier + value.GetHashCode();
            }

            return result;
        }

        public static bool operator ==(InternalHoconObject left, InternalHoconObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InternalHoconObject left, InternalHoconObject right)
        {
            return !Equals(left, right);
        }
    }
}