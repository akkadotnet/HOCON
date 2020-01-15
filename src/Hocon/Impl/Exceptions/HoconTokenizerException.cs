// -----------------------------------------------------------------------
// <copyright file="HoconTokenizerException.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Hocon
{
    public sealed class HoconTokenizerException : Exception, IHoconLineInfo
    {
        internal HoconTokenizerException(string message, Token token) : base(message)
        {
            LineNumber = token.LineNumber;
            LinePosition = token.LinePosition;
            Value = token.Value;
        }

        public string Value { get; }
        public int LineNumber { get; }
        public int LinePosition { get; }
    }
}