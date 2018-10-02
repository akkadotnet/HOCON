//-----------------------------------------------------------------------
// <copyright file="Enums.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
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
