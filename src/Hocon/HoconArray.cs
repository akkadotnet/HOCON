// -----------------------------------------------------------------------
// <copyright file="HoconImmutableArray.cs" company="Akka.NET Project">
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
    public sealed class HoconArray : 
        HoconElement, 
        IImmutableList<HoconElement>
    {
        private readonly ImmutableArray<HoconElement> _elements;

        private HoconArray(IEnumerable<HoconElement> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            _elements = elements.ToImmutableArray();
        }

        public override HoconType Type => HoconType.Array;

        public new HoconElement this[int index] => _elements[index];

        public override string Raw => $"[{string.Join(", ", _elements)}]";

        public override string ToString(int indent, int indentSize)
        {
            var i = new string(' ', indent * indentSize);
            var j = new string(' ', (indent - 1) * indentSize);
            var sb = new StringBuilder($"[{Environment.NewLine}");
            foreach(var element in _elements)
            {
                sb.Append($"{i}{element.ToString(indent + 1, indentSize)},{Environment.NewLine}");
            }
            if (sb.Length > Environment.NewLine.Length + 1)
                sb.Remove(sb.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length + 1);
            sb.Append($"{Environment.NewLine}{j}]");
            return sb.ToString();
        }

        internal static HoconArray Create(IEnumerable<HoconElement> elements)
        {
            return new HoconArray(elements);
        }

        #region Interface implementation

        public IEnumerator<HoconElement> GetEnumerator()
        {
            foreach (var element in _elements)
                yield return element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IImmutableList<HoconElement> Clear()
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public int IndexOf(HoconElement item, int startIndex, int count,
            IEqualityComparer<HoconElement> equalityComparer)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _elements.IndexOf(item, startIndex, count, equalityComparer);
        }

        public int LastIndexOf(HoconElement item, int startIndex, int count,
            IEqualityComparer<HoconElement> equalityComparer)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _elements.LastIndexOf(item, startIndex, count, equalityComparer);
        }

        public IImmutableList<HoconElement> Add(HoconElement element)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> AddRange(IEnumerable<HoconElement> elements)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> Insert(int index, HoconElement element)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> InsertRange(int index, IEnumerable<HoconElement> elements)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> Remove(HoconElement element,
            IEqualityComparer<HoconElement> equalityComparer)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> RemoveAll(Predicate<HoconElement> match)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> RemoveRange(IEnumerable<HoconElement> elements,
            IEqualityComparer<HoconElement> equalityComparer)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> RemoveRange(int index, int count)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> RemoveAt(int index)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> SetItem(int index, HoconElement element)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconElement> Replace(
            HoconElement oldValue,
            HoconElement newValue,
            IEqualityComparer<HoconElement> equalityComparer)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        /*
        public bool Equals(HoconArray other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Count != other.Count) return false;

            for(var i = 0; i < Count; i++)
            {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }
        */

        public override int GetHashCode()
        {
            return 722328647 + _elements.GetHashCode();
        }

        public override bool Equals(HoconElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is HoconArray otherArray)) return false;

            if (otherArray.Count != Count) return false;
            for(var i=0; i<Count; ++i)
            {
                var thisElem = this[i];
                var otherElem = otherArray[i];
                if (!thisElem.Equals(otherElem))
                    return false;
            }
            return true;
        }

        public int Count => _elements.Length;

        #endregion

        #region Casting operators

        public override IList<bool> GetBooleanList()
        {
            return this.Select(v => v.GetBoolean()).ToArray();
        }

        public override bool TryGetBooleanList(out IList<bool> result)
        {
            result = default;
            var list = new List<bool>();
            foreach (var val in this)
            {
                if (!val.TryGetBoolean(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<sbyte> GetSByteList()
        {
            return this.Select(v => v.GetSByte()).ToArray();
        }

        public override bool TryGetSByteList(out IList<sbyte> result)
        {
            result = default;
            var list = new List<sbyte>();
            foreach (var val in this)
            {
                if (!val.TryGetSByte(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<byte> GetByteList()
        {
            return this.Select(v => v.GetByte()).ToArray();
        }

        public override bool TryGetByteList(out IList<byte> result)
        {
            result = default;

            var list = new List<byte>();
            foreach (var val in this)
            {
                if (!val.TryGetByte(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<short> GetShortList()
        {
            return this.Select(v => v.GetShort()).ToArray();
        }

        public override bool TryGetShortList(out IList<short> result)
        {
            result = default;

            var list = new List<short>();
            foreach (var val in this)
            {
                if (!val.TryGetShort(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<ushort> GetUShortList()
        {
            return this.Select(v => v.GetUShort()).ToArray();
        }

        public override bool TryGetUShortList(out IList<ushort> result)
        {
            result = default;

            var list = new List<ushort>();
            foreach (var val in this)
            {
                if (!val.TryGetUShort(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;

        }

        public override IList<int> GetIntList()
        {
            return this.Select(v => v.GetInt()).ToArray();
        }

        public override bool TryGetIntList(out IList<int> result)
        {
            result = default;

            var list = new List<int>();
            foreach (var val in this)
            {
                if (!val.TryGetInt(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<uint> GetUIntList()
        {
            return this.Select(v => v.GetUInt()).ToArray();
        }

        public override bool TryGetUIntList(out IList<uint> result)
        {
            result = default;

            var list = new List<uint>();
            foreach (var val in this)
            {
                if (!val.TryGetUInt(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<long> GetLongList()
        {
            return this.Select(v => v.GetLong()).ToArray();
        }

        public override bool TryGetLongList(out IList<long> result)
        {
            result = default;

            var list = new List<long>();
            foreach (var val in this)
            {
                if (!val.TryGetLong(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<ulong> GetULongList()
        {
            return this.Select(v => v.GetULong()).ToArray();
        }

        public override bool TryGetULongList(out IList<ulong> result)
        {
            result = default;

            var list = new List<ulong>();
            foreach (var val in this)
            {
                if (!val.TryGetULong(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<BigInteger> GetBigIntegerList()
        {
            return this.Select(v => v.GetBigInteger()).ToArray();
        }

        public override bool TryGetBigIntegerList(out IList<BigInteger> result)
        {
            result = default;

            var list = new List<BigInteger>();
            foreach (var val in this)
            {
                if (!val.TryGetBigInteger(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<float> GetFloatList()
        {
            return this.Select(v => v.GetFloat()).ToArray();
        }

        public override bool TryGetFloatList(out IList<float> result)
        {
            result = default;

            var list = new List<float>();
            foreach (var val in this)
            {
                if (!val.TryGetFloat(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;

        }

        public override IList<double> GetDoubleList()
        {
            return this.Select(v => v.GetDouble()).ToArray();
        }

        public override bool TryGetDoubleList(out IList<double> result)
        {
            result = default;

            var list = new List<double>();
            foreach (var val in this)
            {
                if (!val.TryGetDouble(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;

        }

        public override IList<decimal> GetDecimalList()
        {
            return this.Select(v => v.GetDecimal()).ToArray();
        }

        public override bool TryGetDecimalList(out IList<decimal> result)
        {
            result = default;

            var list = new List<decimal>();
            foreach (var val in this)
            {
                if (!val.TryGetDecimal(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;

        }

        public override IList<TimeSpan> GetTimeSpanList(bool allowInfinite = true)
        {
            return this.Select(v => v.GetTimeSpan(allowInfinite)).ToArray();
        }

        public override bool TryGetTimeSpanList(out IList<TimeSpan> result, bool allowInfinite = true)
        {
            result = default;

            var list = new List<TimeSpan>();
            foreach (var val in this)
            {
                if (!val.TryGetTimeSpan(out var res, allowInfinite))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<string> GetStringList()
        {
            return this.Select(v => v.GetString()).ToArray();
        }

        public override bool TryGetStringList(out IList<string> result)
        {
            result = default;

            var list = new List<string>();
            foreach (var val in this)
            {
                if (!val.TryGetString(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<char> GetCharList()
        {
            return this.SelectMany(v => v.ToString().ToCharArray()).ToArray();
        }

        public override bool TryGetCharList(out IList<char> result)
        {
            result = default;

            var list = new List<char>();
            foreach (var val in this)
            {
                if (!val.TryGetChar(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;
        }

        public override IList<HoconObject> GetObjectList()
        {
            return this.Select(v => v.ToObject()).ToArray();
        }

        public override bool TryGetObjectList(out IList<HoconObject> result)
        {
            result = default;

            var list = new List<HoconObject>();
            foreach (var val in this)
            {
                if (!val.TryGetObject(out var res))
                    return false;
                list.Add(res);
            }
            result = list;
            return true;

        }
        #endregion
    }
}