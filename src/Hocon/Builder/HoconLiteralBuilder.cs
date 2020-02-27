// -----------------------------------------------------------------------
// <copyright file="HoconImmutableLiteralBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Hocon
{
    public sealed class HoconLiteralBuilder
    {
        private readonly Regex _dotRegex = new Regex(@"\.", RegexOptions.Compiled);

        private readonly StringBuilder _builder;
        private HoconLiteralType _literalType = HoconLiteralType.UnquotedString;
        private HoconLiteralType LiteralType
        {
            get => _literalType;
            set
            {
                _literalType = value;

                if (_builder.ToString().NeedTripleQuotes())
                    _literalType = HoconLiteralType.TripleQuotedString;
                if (_builder.ToString().NeedQuotes())
                    _literalType = HoconLiteralType.QuotedString;

            }
        }

        #region Properties

        public int Capacity
        {
            get => _builder.Capacity;
            set => _builder.Capacity = value;
        }

        public char this[int index]
        {
            get => _builder[index];
            set => _builder[index] = value;
        }

        public int Length
        {
            get => _builder.Length;
            set => _builder.Length = value;
        }

        public int MaxCapacity => _builder.MaxCapacity;

        #endregion

        #region Constructors

        public HoconLiteralBuilder()
        {
            _builder = new StringBuilder();
        }

        public HoconLiteralBuilder(int capacity)
        {
            _builder = new StringBuilder(capacity);
        }

        public HoconLiteralBuilder(string value)
        {
            _builder = new StringBuilder(value);
        }

        public HoconLiteralBuilder(int capacity, int maxCapacity)
        {
            _builder = new StringBuilder(capacity, maxCapacity);
        }

        public HoconLiteralBuilder(string value, int capacity)
        {
            _builder = new StringBuilder(value, capacity);
        }

        public HoconLiteralBuilder(string value, int startIndex, int length, int capacity)
        {
            _builder = new StringBuilder(value, startIndex, length, capacity);
        }

        #endregion

        internal HoconLiteralBuilder Append(HoconValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            foreach (var element in value)
            {
                if(element is HoconSubstitution sub)
                {
                    Append(sub.ResolvedValue);
                    continue;
                }

                if (!(element is InternalHoconLiteral lit))
                    throw new HoconException(
                        $"Can only add Hocon class of type {nameof(InternalHoconLiteral)} and its derived classes into a literal builder.");

                Append(lit);
            }

            return this;
        }

        internal HoconLiteralBuilder Append(InternalHoconLiteral lit)
        {
            if (lit == null)
                throw new ArgumentNullException(nameof(lit));

            _builder.Append(lit.Value);
            LiteralType = lit.LiteralType;
            return this;
        }

        public bool Equals(HoconLiteralBuilder otherBuilder)
        {
            return _builder.Equals(otherBuilder._builder) && LiteralType == otherBuilder.LiteralType;
        }

        public HoconLiteral Build()
        {
            return HoconLiteral.Create(_builder.ToString(), _literalType);
        }

        #region Facade wrapper functions

        #region Append facade

        public HoconLiteralBuilder Append(ushort value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(uint value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(ulong value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(char[] value, int startIndex, int charCount)
        {
            _builder.Append(value, startIndex, charCount);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Append(string value, int startIndex, int count)
        {
            _builder.Append(value, startIndex, count);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Append(char value, int repeatCount)
        {
            _builder.Append(value, repeatCount);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Append(sbyte value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(float value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(bool value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Bool;
            return this;
        }

        public HoconLiteralBuilder Append(char value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Append(char[] value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Append(decimal value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(byte value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(short value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(int value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(long value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Append(object value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Append(double value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.Double;
            return this;
        }

        public HoconLiteralBuilder Append(string value)
        {
            _builder.Append(value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #endregion

        #region AppendFormat facade

        public HoconLiteralBuilder AppendFormat(IFormatProvider provider, string format, object arg0,
            object arg1, object arg2)
        {
            _builder.AppendFormat(provider, format, arg0, arg1, arg2);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(string format, object arg0)
        {
            _builder.AppendFormat(format, arg0);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(string format, params object[] args)
        {
            _builder.AppendFormat(format, args);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(IFormatProvider provider, string format, object arg0)
        {
            _builder.AppendFormat(provider, format, arg0);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            _builder.AppendFormat(provider, format, args);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(string format, object arg0, object arg1)
        {
            _builder.AppendFormat(format, arg0, arg1);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(IFormatProvider provider, string format, object arg0,
            object arg1)
        {
            _builder.AppendFormat(provider, format, arg0, arg1);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            _builder.AppendFormat(format, arg0, arg1, arg2);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #endregion

#if NS2_1
        #region AppendJoin facade
        public HoconImmutableLiteralBuilder AppendJoin(string separator, params object[] values)
        {
            _builder.AppendJoin(separator, values);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconImmutableLiteralBuilder AppendJoin(string separator, params string[] values)
        {
            _builder.AppendJoin(separator, values);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconImmutableLiteralBuilder AppendJoin(char separator, params object[] values)
        {
            _builder.AppendJoin(separator, values);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconImmutableLiteralBuilder AppendJoin(char separator, params string[] values)
        {
            _builder.AppendJoin(separator, values);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconImmutableLiteralBuilder AppendJoin<T>(char separator, IEnumerable<T> values)
        {
            _builder.AppendJoin(separator, values);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconImmutableLiteralBuilder AppendJoin<T>(string separator, IEnumerable<T> values)
        {
            _builder.AppendJoin(separator, values);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #endregion
#endif

        #region AppendLine facade

        public HoconLiteralBuilder AppendLine()
        {
            _builder.AppendLine();
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder AppendLine(string value)
        {
            _builder.AppendLine(value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #endregion

        public HoconLiteralBuilder Clear()
        {
            _builder.Clear();
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder EnsureCapacity(int capacity)
        {
            _builder.EnsureCapacity(capacity);
            return this;
        }

#if NS2_1
        public bool Equals(ReadOnlySpan<char> span)
        {
            return _builder.Equals(span);
        }
#endif

        public bool Equals(StringBuilder sb)
        {
            return _builder.Equals(sb);
        }

        #region Insert facade

        public HoconLiteralBuilder Insert(int index, string value, int count)
        {
            _builder.Insert(index, value, count);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, ulong value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, uint value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, ushort value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, string value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, float value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Double;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, char[] value, int startIndex, int charCount)
        {
            _builder.Insert(index, value, startIndex, charCount);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, sbyte value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

#if NS2_1
        public HoconImmutableLiteralBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }
#endif

        public HoconLiteralBuilder Insert(int index, long value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, int value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, double value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Double;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, decimal value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Double;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, char[] value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, char value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, byte value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Long;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, bool value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.Bool;
            return this;
        }

        public HoconLiteralBuilder Insert(int index, object value)
        {
            _builder.Insert(index, value);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #endregion

        public HoconLiteralBuilder Remove(int startIndex, int length)
        {
            _builder.Remove(startIndex, length);
            if (_builder.Length == 0)
                LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #region Replace facade

        public HoconLiteralBuilder Replace(char oldChar, char newChar)
        {
            _builder.Replace(oldChar, newChar);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Replace(string oldValue, string newValue)
        {
            _builder.Replace(oldValue, newValue);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            _builder.Replace(oldChar, newChar, startIndex, count);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        public HoconLiteralBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            _builder.Replace(oldValue, newValue, startIndex, count);
            LiteralType = HoconLiteralType.UnquotedString;
            return this;
        }

        #endregion

        public override string ToString()
        {
            return _builder.ToString();
        }

        public string ToString(int startIndex, int length)
        {
            return _builder.ToString(startIndex, length);
        }

        #endregion
    }
}