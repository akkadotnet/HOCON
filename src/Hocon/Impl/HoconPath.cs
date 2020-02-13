// -----------------------------------------------------------------------
// <copyright file="HoconPath.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Hocon
{
    public sealed class HoconPath : List<string>, IEquatable<HoconPath>
    {
        public static readonly HoconPath Empty;

        static HoconPath()
        {
            Empty = new HoconPath();
        }

        internal HoconPath()
        {
        }

        public HoconPath(IEnumerable<string> path)
        {
            AddRange(path);
        }

        public bool IsEmpty => Count == 0;

        public string Value
        {
            get
            {
                if (IsEmpty)
                    return string.Empty;

                // this is chosen to encode \ in a key
                // anything will do, but if the key contains a lot of this we'll need to do many escapes and impact perf.
                // let's hope this sequence appears rare enough for most situations
                const string encodeBackslashAs = "^";

                List<string> pathSegments = new List<string>();
                foreach (string subKey in this)
                {
                    if (subKey == string.Empty)
                    {
                        pathSegments.Add("\"\"");
                        continue;
                    }

                    bool hasEscapeChar = subKey.Contains('\\');
                    string escapedSubKey = hasEscapeChar
                        ? subKey.Replace(encodeBackslashAs, encodeBackslashAs + "2")
                            .Replace("\\", encodeBackslashAs + "1")
                        : subKey;

                    if (escapedSubKey.Contains(Utils.NewLine))
                    {
                        // escape chars that can't be expressed quote/unquoted (e.g. \n)
                        escapedSubKey = escapedSubKey.Replace(Utils.NewLine.ToString(), "\\n").ToHoconSafe();
                    }
                    else
                    {
                        if (escapedSubKey.Contains('.'))
                            escapedSubKey = escapedSubKey.AddQuotes();
                        else
                            escapedSubKey = escapedSubKey.ToHoconSafe();
                    }

                    pathSegments.Add(hasEscapeChar
                        ? escapedSubKey.Replace(encodeBackslashAs + "1", "\\\\")
                            .Replace(encodeBackslashAs + "2", encodeBackslashAs)
                        : escapedSubKey);
                }

                return string.Join(".", pathSegments);
            }
        }

        public string Key => this[Count - 1];

        public bool Equals(HoconPath other)
        {
            if (other is null) return false;
            if (Count != other.Count) return false;

            for (var i = 0; i < Count; ++i)
                if (this[i] != other[i])
                    return false;
            return true;
        }

        public HoconPath SubPath(int length)
        {
            return new HoconPath(GetRange(0, length));
        }

        public HoconPath SubPath(int index, int count)
        {
            return new HoconPath(GetRange(index, count));
        }

        internal bool IsChildPathOf(HoconPath parentPath)
        {
            if (Count < parentPath.Count) return false;

            for (var i = 0; i < parentPath.Count; ++i)
                if (this[i] != parentPath[i])
                    return false;
            return true;
        }

        public override string ToString()
        {
            return Value;
        }

        internal static HoconPath FromTokens(HoconTokenizerResult tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            var result = new List<string>();
            var sb = new StringBuilder();
            while (tokens.Current.Type == TokenType.LiteralValue)
            {
                switch (tokens.Current.LiteralType)
                {
                    case TokenLiteralType.TripleQuotedLiteralValue:
                        throw HoconParserException.Create(tokens.Current, null,
                            "Triple quoted string could not be used in path expression.");

                    case TokenLiteralType.QuotedLiteralValue:
                        // Normalize quoted keys, remove the quotes if the key doesn't need them.
                        //sb.Append(tokens.Current.Value.NeedQuotes() ? $"\"{tokens.Current.Value}\"" : tokens.Current.Value);
                        sb.Append(tokens.Current.Value);
                        break;

                    default:
                        var split = tokens.Current.Value.Split('.');
                        for (var i = 0; i < split.Length - 1; ++i)
                        {
                            sb.Append(split[i]);
                            result.Add(sb.ToString());
                            sb.Clear();
                        }

                        sb.Append(split[split.Length - 1]);
                        break;
                }

                tokens.Next();
            }

            result.Add(sb.ToString());
            return new HoconPath(result);
        }

        public static bool TryParse(string path, out HoconPath result)
        {
            result = null;
            if (path == null)
                return false;
            try
            {
                result = FromTokens(new HoconTokenizer(path).Tokenize());
                return true;
            } catch {
                return false;
            }
        }

        public static HoconPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return FromTokens(new HoconTokenizer(path).Tokenize());
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is HoconPath other && Equals(other);
        }

        public override int GetHashCode()
        {
            const int modifier = 31;
            var result = 601;
            unchecked
            {
                foreach (var key in this) result = result * modifier + key.GetHashCode();
            }

            return result;
        }

        public static bool operator ==(HoconPath left, HoconPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconPath left, HoconPath right)
        {
            return !Equals(left, right);
        }
    }
}