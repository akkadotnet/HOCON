//-----------------------------------------------------------------------
// <copyright file="Whitespace.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class Whitespace
    {
        // These are all the characters defined as whitespace by Hocon spec 
        // Unicode space separator (Zs category)
        public static readonly char Space = '\u0020';
        public static readonly char NoBreakSpace = '\u00A0';
        public static readonly char OghamSpaceMark = '\u1680';
        public static readonly char EnQuad = '\u2000';
        public static readonly char EmQuad = '\u2001';
        public static readonly char EnSpace = '\u2002';
        public static readonly char EmSpace = '\u2003';
        public static readonly char ThreePerEmSpace = '\u2004';
        public static readonly char FourPerEmSpace = '\u2005';
        public static readonly char SixPerEmSpace = '\u2006';
        public static readonly char FigureSpace = '\u2007';
        public static readonly char PunctuationSpace = '\u2008';
        public static readonly char ThinSpace = '\u2009';
        public static readonly char HairSpace = '\u200A';
        public static readonly char NarrowNoBreakSpace = '\u202F';
        public static readonly char MediumMathematicalSpace = '\u205F';
        public static readonly char IdeographicSpace = '\u3000';

        // Unicode line separator(Zl category)
        public static readonly char LineSeparator = '\u2028';

        // Unicode paragraph separator (Zp category)
        public static readonly char ParagraphSeparator = '\u2029';

        // Unicode BOM
        public static readonly char BOM = '\uFEFF';

        // Other Unicode whitespaces
        public static readonly char Tab = '\u0009';              // \t
        public static readonly char NewLine = '\u000A';          // \n
        public static readonly char VerticalTab = '\u000B';      // \v
        public static readonly char FormFeed = '\u000C';         // \f
        public static readonly char CarriageReturn = '\u000D';   // \r
        public static readonly char FileSeparator = '\u001C';
        public static readonly char GroupSeparator = '\u001D';
        public static readonly char RecordSeparator = '\u001E';
        public static readonly char UnitSeparator = '\u001F';

        public static readonly string Whitespaces = new string(new [] {
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
