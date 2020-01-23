// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Hocon;

namespace HelloHocon
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.Load();
            var val = config.GetString("root.simple-string");
            Console.WriteLine("Hocon says: " + val);
            Console.ReadKey();
        }
    }
}