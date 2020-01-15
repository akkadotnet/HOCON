// -----------------------------------------------------------------------
// <copyright file="IHoconLineInfo.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

namespace Hocon
{
    internal interface IHoconLineInfo
    {
        int LineNumber { get; }
        int LinePosition { get; }
    }
}