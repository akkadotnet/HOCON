//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using System;
using Akka.Hocon.Configuration;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Runtime;

namespace HelloMFHocon
{
    public class Program
    {
        private readonly IApplicationEnvironment applicationEnvironment;

        public Program(IApplicationEnvironment applicationEnvironment)
        {
            this.applicationEnvironment = applicationEnvironment;
        }

        public void Main(string[] args)
        {
            var builder = new ConfigurationBuilder(applicationEnvironment.ApplicationBasePath);
            builder.AddHoconFile("hello.hocon");
            var config = builder.Build();
            var val = config.Get("root:simple-string");
            Console.WriteLine("Hocon says: " + val);
            Console.ReadLine();
        }
    }
}
