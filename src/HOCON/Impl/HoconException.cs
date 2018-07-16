using System;

namespace Hocon
{
    public class HoconException:Exception
    {
        public HoconException(string message):base(message)
        { }

        public HoconException(string message, Exception innerException):base(message, innerException)
        { }
    }
}
