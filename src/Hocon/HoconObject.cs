// -----------------------------------------------------------------------
// <copyright file="HoconImmutableObject.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Hocon
{
    public class HoconObject :
        HoconElement,
        IReadOnlyDictionary<string, HoconElement>,
        IDictionary<string, HoconElement>
    {
        public static readonly HoconObject Empty = new HoconObject();

        private ImmutableSortedDictionary<string, HoconElement> _fields;

        protected HoconObject()
        {
            _fields = ImmutableSortedDictionary<string, HoconElement>.Empty;
        }

        protected HoconObject(HoconElement source)
        {
            if (!(source is HoconObject other))
                throw new HoconException($"Could not create HoconObject from {source.GetType()}");

            _fields = other.ToImmutableSortedDictionary();
        }

        protected HoconObject(IDictionary<string, HoconElement> fields)
        {
            _fields = fields.ToImmutableSortedDictionary();
        }

        protected HoconObject(IReadOnlyDictionary<string, HoconElement> fields)
        {
            _fields = fields.ToImmutableSortedDictionary();
        }

        public override HoconType Type => HoconType.Object;

        public override HoconElement this[HoconPath path] => GetValue(path);

        public new HoconElement this[string path] {
            get => GetValue(path);
            set => throw new InvalidOperationException("HoconObject is a read only Dictionary.");
        }

        public IDictionary<string, object> Unwrapped =>
            this.ToDictionary(k => k.Key, v =>
                    v.Value is HoconObject obj ? (object)obj.Unwrapped : v.Value);

        public IEnumerable<string> Keys => _fields.Keys;
        public IEnumerable<HoconElement> Values => _fields.Values;
        public int Count => _fields.Count;

        ICollection<string> IDictionary<string, HoconElement>.Keys => _fields.Keys.ToArray();

        ICollection<HoconElement> IDictionary<string, HoconElement>.Values => _fields.Values.ToArray();

        public bool IsReadOnly => true;

        public override bool HasPath(string path)
        {
            return TryGetValue(path, out _);
        }

        public override bool HasPath(HoconPath path)
        {
            return TryGetValue(path, out _);
        }

        public HoconArray ToArray()
        {
            var sortedDict = new SortedDictionary<int, HoconElement>();
            Type type = null;
            foreach (var kvp in _fields)
            {
                if (!int.TryParse(kvp.Key, out var index) || index < 0)
                    continue;
                if (type == null)
                    type = kvp.Value.GetType();
                else if (type != kvp.Value.GetType())
                    throw new HoconException($"Array element mismatch. Expected: {type} Found: {kvp.Value.GetType()}");
                sortedDict[index] = kvp.Value;
            }

            if (sortedDict.Count == 0)
                throw new HoconException(
                    "Object is empty, does not contain any numerically indexed fields, or contains only non-positive integer indices");

            return HoconArray.Create(sortedDict.Values);
        }

        public bool TryGetArray(out HoconArray result)
        {
            result = default;
            var sortedDict = new SortedDictionary<int, HoconElement>();
            Type type = null;
            foreach (var kvp in _fields)
            {
                if (!int.TryParse(kvp.Key, out var index) || index < 0)
                    continue;
                if (type == null)
                    type = kvp.Value.GetType();
                else if (type != kvp.Value.GetType())
                    return false;
                sortedDict[index] = kvp.Value;
            }

            if (sortedDict.Count == 0)
                return false;

            result = HoconArray.Create(sortedDict.Values);
            return true;
        }

        public HoconObject Merge(HoconObject other)
        {
            return new HoconObjectBuilder(this).Merge(other).Build();
        }

        /// <summary>
        ///     Converts the current HoconObject to a pretty printed string.
        /// </summary>
        /// <param name="indentSize">The number of spaces used for each indent.</param>
        /// <returns>A string containing the current configuration.</returns>
        public string PrettyPrint(int indentSize)
        {
            return ToString(1, indentSize);
        }

        public override string ToString()
        {
            return ToString(1, 2);
        }

        public override string ToString(int indent, int indentSize)
        {
            var i = new string(' ', indent * indentSize);
            var j = new string(' ', (indent - 1) * indentSize);
            var sb = new StringBuilder($"{{{Environment.NewLine}");
            foreach (var field in this)
            {
                sb.Append($"{i}{(field.Key.NeedQuotes() ? field.Key.AddQuotes() : field.Key)} : {field.Value.ToString(indent + 1, indentSize)},{Environment.NewLine}");
            }
            if (sb.Length > Environment.NewLine.Length + 1) 
                sb.Remove(sb.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length + 1);
            sb.Append($"{Environment.NewLine}{j}}}");
            return sb.ToString();
            //return sb.Length > 2 ? sb.ToString(0, sb.Length - Environment.NewLine.Length - 1) : sb.ToString();
        }

        internal static HoconObject Create(IDictionary<string, HoconElement> fields)
        {
            return new HoconObject(fields);
        }

        #region Interface implementation

        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        [Obsolete("Use GetValue() instead")]
        public HoconElement GetField(string path)
        {
            return GetValue(path);
        }

        [Obsolete("Use GetValue() instead")]
        public HoconElement GetField(HoconPath path)
        {
            return GetValue(path);
        }

        public virtual HoconElement GetValue(string path)
        {
            return GetValue(HoconPath.Parse(path));
        }

        public virtual HoconElement GetValue(HoconPath path)
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

                if (!currentObject._fields.TryGetValue(key, out var field))
                    throw new KeyNotFoundException(
                        $"Could not find field with key `{key}` at path `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                if (pathIndex >= path.Count - 1)
                    return field;

                if (!(field is HoconObject obj))
                    throw new HoconException(
                        $"Invalid path, trying to access a key on a non object field. Path: `{new HoconPath(path.GetRange(0, pathIndex + 1)).Value}`");

                currentObject = obj;
                pathIndex++;
            }
        }

        public virtual bool TryGetValue(string path, out HoconElement result)
        {
            result = null;
            return !string.IsNullOrWhiteSpace(path) && TryGetValue(HoconPath.Parse(path), out result);
        }

        public virtual bool TryGetValue(HoconPath path, out HoconElement result)
        {
            result = null;
            if (path == null || path.Count == 0)
                return false;

            var pathIndex = 0;
            var currentObject = this;
            while (true)
            {
                var key = path[pathIndex];

                if (!currentObject._fields.TryGetValue(key, out var field))
                    return false;

                if (pathIndex >= path.Count - 1)
                {
                    result = field;
                    return true;
                }

                if (!(field is HoconObject obj))
                    return false;

                currentObject = obj;
                pathIndex++;
            }
        }

        public IEnumerator<KeyValuePair<string, HoconElement>> GetEnumerator()
        {
            return !_fields.IsEmpty
                ? _fields.GetEnumerator()
                : Enumerable.Empty<KeyValuePair<string, HoconElement>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(HoconElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is HoconObject otherObject))
                return false;

            foreach (var kvp in otherObject)
            {
                if (!_fields.TryGetValue(kvp.Key, out var thisValue))
                    return false;
                if (!thisValue.Equals(kvp.Value))
                    return false;
            }
            return true;
        }

        [Obsolete("Used only for serialization", true)]
        public void Add(string key, HoconElement value)
        {
            var fields = _fields.ToList();
            fields.Add(new KeyValuePair<string, HoconElement>(key, value));
            _fields = fields.ToImmutableSortedDictionary();
        }

        public bool Remove(string key)
        {
            throw new InvalidOperationException("HoconObject is a read only Dictionary.");
        }

        public void Add(KeyValuePair<string, HoconElement> item)
        {
            throw new InvalidOperationException("HoconObject is a read only Dictionary.");
        }

        public void Clear()
        {
            throw new InvalidOperationException("HoconObject is a read only Dictionary.");
        }

        public bool Contains(KeyValuePair<string, HoconElement> item)
        {
            return _fields.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, HoconElement>[] array, int arrayIndex)
        {
            foreach(var kvp in _fields)
            {
                array[arrayIndex] = kvp;
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<string, HoconElement> item)
        {
            throw new InvalidOperationException("HoconObject is a read only Dictionary.");
        }

        #endregion

        #region Casting operators

        public static implicit operator bool[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator sbyte[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator byte[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator short[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator ushort[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator int[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator uint[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator long[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator ulong[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator BigInteger[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator float[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator double[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator decimal[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator TimeSpan[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator string[](HoconObject obj)
        {
            return obj.ToArray();
        }

        public static implicit operator char[](HoconObject obj)
        {
            return obj.ToArray();
        }

        #endregion
    }
}