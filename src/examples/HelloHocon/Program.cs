//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using System;

namespace HelloHocon
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.Load();
            var val = config.GetString("root.simple-string");
            Console.WriteLine("Hocon says: " + val);
            Console.ReadKey();
        }
    }
}
