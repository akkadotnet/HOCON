// -----------------------------------------------------------------------
// <copyright file="Enums.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

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
        Array,
        Object,
        Number,
        Boolean,
        String
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
        Double
    }
}