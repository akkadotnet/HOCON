//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Akka.Configuration;
using Akka.Configuration.Hocon;

namespace ConfigurationFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.Load();

            string result1 = config.GetString("root.simple-string-1");
            string resulta = config.GetString("root.simple-string-a");
            string resultb = config.GetString("root.simple-string-b");
            string resultc = config.GetString("root.simple-string-c");

            Console.WriteLine("root.simple-string-1: " + result1);
            Console.WriteLine("root.simple-string-a: " + resulta);
            Console.WriteLine("root.simple-string-b: " + resultb);
            Console.WriteLine("root.simple-string-c: " + resultc);

            Console.ReadKey();
        }
    }
}
