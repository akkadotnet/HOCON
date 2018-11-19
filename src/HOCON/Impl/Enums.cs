//-----------------------------------------------------------------------
// <copyright file="Enums.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

namespace Hocon
{
    public enum HoconCallbackType
    {
        File,
        Url,
        Resource
    }

    public enum HoconType
    {
        Empty,
        Literal,
        Array,
        Object,
    }

    public enum HoconLiteralType
    {
        Null,
        Whitespace,
        UnquotedString,
        QuotedString,
        TripleQuotedString,
        Bool,
        Long,
        Hex,
        Octal,
        Double,
    }
}
