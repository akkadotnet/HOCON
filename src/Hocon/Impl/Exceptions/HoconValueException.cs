// -----------------------------------------------------------------------
// <copyright file="HoconValueException.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Hocon
{
    public class HoconValueException : HoconException
    {
        /// <inheritdoc />
        public HoconValueException(string message, string failPath, Exception innerException) : base(message,
            innerException)
        {
            FailPath = failPath;
        }

        /// <summary>
        ///     Gets HOCON path of the value which caused failure
        /// </summary>
        public string FailPath { get; }
    }
}