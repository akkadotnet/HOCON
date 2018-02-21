using System;

namespace Hocon
{
    public class HoconParserException : Exception
    {
        public HoconParserException(string message) : base(message)
        {
        }

        public HoconParserException(string message,Exception innerException) : base(message,innerException)
        {
        }
    }
}
