// -----------------------------------------------------------------------
// <copyright file="HoconException.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Hocon
{
    public class HoconException : Exception
    {
        public HoconException(string message) : base(message)
        {
        }

        public HoconException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}