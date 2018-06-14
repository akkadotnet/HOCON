using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocon.Tests
{
    public class Whitespace
    {
        // These are all the characters defined as whitespace by Hocon spec 
        // Unicode space separator (Zs category)
        private const char Space = '\u0020';
        private const char NoBreakSpace = '\u00A0';
        private const char OghamSpaceMark = '\u1680';
        private const char EnQuad = '\u2000';
        private const char EmQuad = '\u2001';
        private const char EnSpace = '\u2002';
        private const char EmSpace = '\u2003';
        private const char ThreePerEmSpace = '\u2004';
        private const char FourPerEmSpace = '\u2005';
        private const char SixPerEmSpace = '\u2006';
        private const char FigureSpace = '\u2007';
        private const char PunctuationSpace = '\u2008';
        private const char ThinSpace = '\u2009';
        private const char HairSpace = '\u200A';
        private const char NarrowNoBreakSpace = '\u202F';
        private const char MediumMathematicalSpace = '\u205F';
        private const char IdeographicSpace = '\u3000';

        // Unicode line separator(Zl category)
        private const char LineSeparator = '\u2028';

        // Unicode paragraph separator (Zp category)
        private const char ParagraphSeparator = '\u2029';

        // Unicode BOM
        private const char BOM = '\uFEFF';

        // Other Unicode whitespaces
        private const char Tab = '\u0009';              // \t
        private const char NewLine = '\u000A';          // \n
        private const char VerticalTab = '\u000B';      // \v
        private const char FormFeed = '\u000C';         // \f
        private const char CarriageReturn = '\u000D';   // \r
        private const char FileSeparator = '\u001C';
        private const char GroupSeparator = '\u001D';
        private const char RecordSeparator = '\u001E';
        private const char UnitSeparator = '\u001F';

        private readonly string Whitespaces = new string(new [] {
            Space, NoBreakSpace, OghamSpaceMark, EnQuad, EmQuad,
            EnSpace, EmSpace, ThreePerEmSpace, FourPerEmSpace, SixPerEmSpace,
            FigureSpace, PunctuationSpace, ThinSpace, HairSpace, NarrowNoBreakSpace,
            MediumMathematicalSpace, IdeographicSpace,

            // Unicode line separator(Zl category)
            LineSeparator,

            // Unicode paragraph separator (Zp category)
            ParagraphSeparator,

            // Unicode BOM
            BOM,

            // Other Unicode whitespaces
            Tab, VerticalTab, FormFeed, CarriageReturn,
            FileSeparator, GroupSeparator, RecordSeparator, UnitSeparator,
        });

        // TODO: Figure out what needs to be tested for whitespaces, probably string.Trim() for unquoted strings
    }
}
