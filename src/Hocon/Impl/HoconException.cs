//-----------------------------------------------------------------------
// <copyright file="HoconException.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

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
