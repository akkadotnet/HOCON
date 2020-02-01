// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Hocon;

namespace SimpleSubstitutions
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hocon = @"
root {
  some-value = ""Hello""
  simple-string = ${root.some-value} ""Hocon""  
}
";
            var config = HoconParser.Parse(hocon);
            var val = config.GetString("root.simple-string");
            Console.WriteLine("Hocon says: " + val);
            Console.ReadKey();
        }
    }
}