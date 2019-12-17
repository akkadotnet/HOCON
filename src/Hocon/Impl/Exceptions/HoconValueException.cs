using System;

namespace Hocon
{
    public class HoconValueException : HoconException
    {
        /// <inheritdoc />
        public HoconValueException(string message, string failPath, Exception innerException) : base(message, innerException)
        {
            FailPath = failPath;
        }
        
        public string FailPath { get; }
    }
}