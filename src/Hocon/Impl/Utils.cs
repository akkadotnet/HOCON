// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;
using System.Linq;

namespace Hocon
{
    internal static class Utils
    {
        /*
         * These are all the characters defined as whitespace by Hocon spec.
         * Hocon spec only recognizes NewLine as an End of Line marker.
         *
         * Unicode space separator (Zs category):
         *   - Space = '\u0020';
         *   - NoBreakSpace = '\u00A0';
         *   - OghamSpaceMark = '\u1680';
         *   - EnQuad = '\u2000';
         *   - EmQuad = '\u2001';
         *   - EnSpace = '\u2002';
         *   - EmSpace = '\u2003';
         *   - ThreePerEmSpace = '\u2004';
         *   - FourPerEmSpace = '\u2005';
         *   - SixPerEmSpace = '\u2006';
         *   - FigureSpace = '\u2007';
         *   - PunctuationSpace = '\u2008';
         *   - ThinSpace = '\u2009';
         *   - HairSpace = '\u200A';
         *   - NarrowNoBreakSpace = '\u202F';
         *   - MediumMathematicalSpace = '\u205F';
         *   - IdeographicSpace = '\u3000';
         * 
         * Unicode line separator(Zl category):
         *   - LineSeparator = '\u2028';
         *
         * Unicode paragraph separator (Zp category):
         *   - ParagraphSeparator = '\u2029';
         *
         * Unicode BOM
         *   - BOM = '\uFEFF';
         *
         * Other Unicode whitespaces
         *   - Tab = '\u0009'; // \t
         *   - NewLine = '\u000A'; // \n
         *   - VerticalTab = '\u000B'; // \v
         *   - FormFeed = '\u000C'; // \f
         *   - CarriageReturn = '\u000D'; // \r
         *   - FileSeparator = '\u001C';
         *   - GroupSeparator = '\u001D';
         *   - RecordSeparator = '\u001E';
         *   - UnitSeparator = '\u001F';
         */

        public const string WhitespaceWithoutNewLine =
            "\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A" +
            "\u202F\u205F\u3000\u2028\u2029\uFEFF\u0009\u000B\u000C\u000D\u001C\u001D\u001E\u001F";

        public const string Whitespaces =
            "\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A" +
            "\u202F\u205F\u3000\u2028\u2029\uFEFF\u0009\u000A\u000B\u000C\u000D\u001C\u001D\u001E\u001F";

        public const string NotInUnquotedText = "$\"{}[]:=,#`^?!@*&\\";
        public const char NewLine = '\u000A';
        public const string Octals = "12345670";
        public const string Digits = "1234567890";
        public const string Hexadecimal = "1234567890abcdefABCDEF";

        public static bool IsSubstitution(this IHoconElement value)
        {
            switch (value)
            {
                case HoconValue v:
                    return v.Any(n => n.IsSubstitution());
                case HoconField f:
                    return f.Value.Any(n => n.IsSubstitution());
                case HoconObject o:
                    foreach (var f in o.Values)
                        if (f.Value.Any(n => n.IsSubstitution()))
                            return true;
                    return false;
                case HoconArray a:
                    foreach (var v in a)
                        if (v.Any(n => n.IsSubstitution()))
                            return true;
                    return false;
                case HoconSubstitution _:
                    return true;
                default:
                    return false;
            }
        }

        #region String extensions

        public static string TrimWhitespace(this string value)
        {
            int index = 0;
            while (Whitespaces.Contains(value[index]) && index < value.Length) index++;

            var trimmed = value.Substring(index);
            index = trimmed.Length - 1;
            while (Whitespaces.Contains(value[index]) && index > 0) index--;
            var res = trimmed.Substring(0, index + 1);
            return res;
        }

        public static bool IsNotInUnquotedText(this char c)
        {
            return NotInUnquotedText.Contains(c);
        }

        public static bool IsHoconWhitespace(this char c)
        {
            return Whitespaces.Contains(c);
        }

        public static bool IsWhitespaceWithNoNewLine(this char c)
        {
            return WhitespaceWithoutNewLine.Contains(c);
        }

        public static bool IsDigit(this char c)
        {
            return Digits.Contains(c);
        }

        public static bool IsOctal(this char c)
        {
            return Octals.Contains(c);
        }

        public static bool IsHexadecimal(this char c)
        {
            return Hexadecimal.Contains(c);
        }

        public static string ToHoconSafe(this string s)
        {
            return s.NeedTripleQuotes() ? $"\"\"\"{s}\"\"\"" : s.NeedQuotes() ? s.AddQuotes() : s;
        }

        public static HoconPath ToHoconPath(this string path)
        {
            return HoconPath.Parse(path);
        }

        public static bool Contains(this string s, char c)
        {
            return s.IndexOf(c) != -1;
        }

        #endregion

        #region Token extensions

        [DebuggerStepThrough]
        public static bool IsSignificant(this Token token)
        {
            return token.Type != TokenType.Comment && token.LiteralType != TokenLiteralType.Whitespace;
        }

        [DebuggerStepThrough]
        public static bool IsNonSignificant(this Token token)
        {
            return !token.IsSignificant();
        }

        #endregion
    }

    public static class StringUtil
    {
        public static bool NeedQuotes(this string s)
        {
            foreach (var c in s)
                if (Enumerable.Contains(Utils.NotInUnquotedText, c))
                    return true;
            return false;
        }

        public static bool NeedTripleQuotes(this string s)
        {
            return s.NeedQuotes() && s.Contains(Utils.NewLine);
        }

        public static string AddQuotes(this string s)
        {
            if (s.Contains('"'))
                return "\"" + s.Replace("\"", "\\\"") + "\"";

            return "\"" + s + "\"";
        }
    }
}