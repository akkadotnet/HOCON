// -----------------------------------------------------------------------
// <copyright file="HoconArray.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Hocon
{
    public sealed class HoconArray : HoconElement, IImmutableList<HoconElement>
    {
        private readonly ImmutableArray<HoconElement> _elements;

        private HoconArray(IEnumerable<HoconElement> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            _elements = elements.ToImmutableArray();
        }

        public new HoconElement this[int index] => _elements[index];

        internal static HoconArray Create(IEnumerable<HoconElement> elements)
        {
            return new HoconArray(elements);
        }

        public override string ToString(int indent, int indentSize)
        {
            var i = new string(' ', indent * indentSize);
            var sb = new StringBuilder($"{i}[{Environment.NewLine}");
            foreach (var element in this)
            {
                sb.AppendLine(element.ToString(indent, indentSize + 1));
            }
            sb.Append($"{i}]");
            return sb.ToString();
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

        public int Count => _elements.Length;

        #endregion

        #region Casting operators

        public static implicit operator bool[](HoconArray arr)
        {
            return arr.Select(v => (bool) v).ToArray();
        }

        public static implicit operator sbyte[](HoconArray arr)
        {
            return arr.Select(v => (sbyte) v).ToArray();
        }

        public static implicit operator byte[](HoconArray arr)
        {
            return arr.Select(v => (byte) v).ToArray();
        }

        public static implicit operator short[](HoconArray arr)
        {
            return arr.Select(v => (short) v).ToArray();
        }

        public static implicit operator ushort[](HoconArray arr)
        {
            return arr.Select(v => (ushort) v).ToArray();
        }

        public static implicit operator int[](HoconArray arr)
        {
            return arr.Select(v => (int) v).ToArray();
        }

        public static implicit operator uint[](HoconArray arr)
        {
            return arr.Select(v => (uint) v).ToArray();
        }

        public static implicit operator long[](HoconArray arr)
        {
            return arr.Select(v => (long) v).ToArray();
        }

        public static implicit operator ulong[](HoconArray arr)
        {
            return arr.Select(v => (ulong) v).ToArray();
        }

        public static implicit operator BigInteger[](HoconArray arr)
        {
            return arr.Select(v => (BigInteger) v).ToArray();
        }

        public static implicit operator float[](HoconArray arr)
        {
            return arr.Select(v => (float) v).ToArray();
        }

        public static implicit operator double[](HoconArray arr)
        {
            return arr.Select(v => (double) v).ToArray();
        }

        public static implicit operator decimal[](HoconArray arr)
        {
            return arr.Select(v => (decimal) v).ToArray();
        }

        public static implicit operator TimeSpan[](HoconArray arr)
        {
            return arr.Select(v => (TimeSpan) v).ToArray();
        }

        public static implicit operator string[](HoconArray arr)
        {
            return arr.Select(v => (string) v).ToArray();
        }

        public static implicit operator char[](HoconArray arr)
        {
            return arr.SelectMany(v => ((string) v).ToCharArray()).ToArray();
        }

        #endregion
    }
}