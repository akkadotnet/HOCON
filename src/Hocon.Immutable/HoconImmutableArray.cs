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

namespace Hocon.Immutable
{
    public sealed class HoconImmutableArray : HoconImmutableElement, IImmutableList<HoconImmutableElement>
    {
        private readonly ImmutableArray<HoconImmutableElement> _elements;

        private HoconImmutableArray(IEnumerable<HoconImmutableElement> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            _elements = elements.ToImmutableArray();
        }

        public new HoconImmutableElement this[int index] => _elements[index];

        internal static HoconImmutableArray Create(IEnumerable<HoconImmutableElement> elements)
        {
            return new HoconImmutableArray(elements);
        }

        #region Interface implementation

        public IEnumerator<HoconImmutableElement> GetEnumerator()
        {
            foreach (var element in _elements)
                yield return element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IImmutableList<HoconImmutableElement> Clear()
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public int IndexOf(HoconImmutableElement item, int startIndex, int count,
            IEqualityComparer<HoconImmutableElement> equalityComparer)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _elements.IndexOf(item, startIndex, count, equalityComparer);
        }

        public int LastIndexOf(HoconImmutableElement item, int startIndex, int count,
            IEqualityComparer<HoconImmutableElement> equalityComparer)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _elements.LastIndexOf(item, startIndex, count, equalityComparer);
        }

        public IImmutableList<HoconImmutableElement> Add(HoconImmutableElement element)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> AddRange(IEnumerable<HoconImmutableElement> elements)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> Insert(int index, HoconImmutableElement element)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> InsertRange(int index, IEnumerable<HoconImmutableElement> elements)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> Remove(HoconImmutableElement element,
            IEqualityComparer<HoconImmutableElement> equalityComparer)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> RemoveAll(Predicate<HoconImmutableElement> match)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> RemoveRange(IEnumerable<HoconImmutableElement> elements,
            IEqualityComparer<HoconImmutableElement> equalityComparer)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> RemoveRange(int index, int count)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> RemoveAt(int index)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> SetItem(int index, HoconImmutableElement element)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public IImmutableList<HoconImmutableElement> Replace(
            HoconImmutableElement oldValue,
            HoconImmutableElement newValue,
            IEqualityComparer<HoconImmutableElement> equalityComparer)
        {
            throw new InvalidOperationException("Can not change array state after it is built.");
        }

        public int Count => _elements.Length;

        #endregion

        #region Casting operators

        public static implicit operator bool[](HoconImmutableArray arr)
        {
            return arr.Select(v => (bool) v).ToArray();
        }

        public static implicit operator sbyte[](HoconImmutableArray arr)
        {
            return arr.Select(v => (sbyte) v).ToArray();
        }

        public static implicit operator byte[](HoconImmutableArray arr)
        {
            return arr.Select(v => (byte) v).ToArray();
        }

        public static implicit operator short[](HoconImmutableArray arr)
        {
            return arr.Select(v => (short) v).ToArray();
        }

        public static implicit operator ushort[](HoconImmutableArray arr)
        {
            return arr.Select(v => (ushort) v).ToArray();
        }

        public static implicit operator int[](HoconImmutableArray arr)
        {
            return arr.Select(v => (int) v).ToArray();
        }

        public static implicit operator uint[](HoconImmutableArray arr)
        {
            return arr.Select(v => (uint) v).ToArray();
        }

        public static implicit operator long[](HoconImmutableArray arr)
        {
            return arr.Select(v => (long) v).ToArray();
        }

        public static implicit operator ulong[](HoconImmutableArray arr)
        {
            return arr.Select(v => (ulong) v).ToArray();
        }

        public static implicit operator BigInteger[](HoconImmutableArray arr)
        {
            return arr.Select(v => (BigInteger) v).ToArray();
        }

        public static implicit operator float[](HoconImmutableArray arr)
        {
            return arr.Select(v => (float) v).ToArray();
        }

        public static implicit operator double[](HoconImmutableArray arr)
        {
            return arr.Select(v => (double) v).ToArray();
        }

        public static implicit operator decimal[](HoconImmutableArray arr)
        {
            return arr.Select(v => (decimal) v).ToArray();
        }

        public static implicit operator TimeSpan[](HoconImmutableArray arr)
        {
            return arr.Select(v => (TimeSpan) v).ToArray();
        }

        public static implicit operator string[](HoconImmutableArray arr)
        {
            return arr.Select(v => (string) v).ToArray();
        }

        public static implicit operator char[](HoconImmutableArray arr)
        {
            return arr.SelectMany(v => ((string) v).ToCharArray()).ToArray();
        }

        #endregion
    }
}