using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon
{
    public static class StringUtil
    {
        // These are all the characters defined as whitespace by Hocon spec 
        // Unicode space separator (Zs category)
        public const char Space = '\u0020';

        public const char NoBreakSpace = '\u00A0';
        public const char OghamSpaceMark = '\u1680';
        public const char EnQuad = '\u2000';
        public const char EmQuad = '\u2001';
        public const char EnSpace = '\u2002';
        public const char EmSpace = '\u2003';
        public const char ThreePerEmSpace = '\u2004';
        public const char FourPerEmSpace = '\u2005';
        public const char SixPerEmSpace = '\u2006';
        public const char FigureSpace = '\u2007';
        public const char PunctuationSpace = '\u2008';
        public const char ThinSpace = '\u2009';
        public const char HairSpace = '\u200A';
        public const char NarrowNoBreakSpace = '\u202F';
        public const char MediumMathematicalSpace = '\u205F';
        public const char IdeographicSpace = '\u3000';

        // Unicode line separator(Zl category)
        public const char LineSeparator = '\u2028';

        // Unicode paragraph separator (Zp category)
        public const char ParagraphSeparator = '\u2029';

        // Unicode BOM
        public const char BOM = '\uFEFF';

        // Other Unicode whitespaces
        public const char Tab = '\u0009'; // \t

        public const char NewLine = '\u000A'; // \n
        public const char VerticalTab = '\u000B'; // \v
        public const char FormFeed = '\u000C'; // \f
        public const char CarriageReturn = '\u000D'; // \r
        public const char FileSeparator = '\u001C';
        public const char GroupSeparator = '\u001D';
        public const char RecordSeparator = '\u001E';
        public const char UnitSeparator = '\u001F';

        public const string WhitespaceWithoutNewLine =
            "\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A" +
            "\u202F\u205F\u3000\u2028\u2029\uFEFF\u0009\u000B\u000C\u000D\u001C\u001D\u001E\u001F";

        public const string Whitespaces =
            "\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A" +
            "\u202F\u205F\u3000\u2028\u2029\uFEFF\u0009\u000A\u000B\u000C\u000D\u001C\u001D\u001E\u001F";

        public static string TrimWhitespace(this string value)
        {
            int index = 0;
            while (Whitespaces.Contains(value[index]))
            {
                index++;
            }

            var trimmed = value.Substring(index);
            index = trimmed.Length - 1;
            while (Whitespaces.Contains(value[index]))
            {
                index--;
            }
            var res = trimmed.Substring(0, index+1);
            return res;
        }

        public static bool IsWhitespace(char c)
        {
            return Whitespaces.Contains(c);
        }
    }
}
