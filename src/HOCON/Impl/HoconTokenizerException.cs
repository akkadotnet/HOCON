//-----------------------------------------------------------------------
// <copyright file="HoconTokenizerException.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Hocon
{
    public sealed class HoconTokenizerException : Exception, IHoconLineInfo
    {
        public int LineNumber { get; }
        public int LinePosition { get; }
        public string Value { get; }

        internal HoconTokenizerException(string message, Token token) : base(message)
        {
            LineNumber = token.LineNumber;
            LinePosition = token.LinePosition;
            Value = token.Value;
        }

    }
}
