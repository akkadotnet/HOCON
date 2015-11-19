//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using System;
using System.Configuration;
using ExternalIncludes;

namespace HelloHocon
{
    class Program
    {
        static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka") ?? new AkkaConfigurationSection();
            var config = section.AkkaConfig;
            var val = config.GetString("root.simple-string");
            Console.WriteLine("Hocon says: " + val);
            Console.ReadKey();
        }
    }
}
