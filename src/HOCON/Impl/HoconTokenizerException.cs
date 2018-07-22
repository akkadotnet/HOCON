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
