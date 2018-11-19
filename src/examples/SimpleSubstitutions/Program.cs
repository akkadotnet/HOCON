//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Hocon;

namespace SimpleSubstitutions
{
    class Program
    {
        static void Main(string[] args)
        {
            var hocon = @"
root {
  some-value = ""Hello""
  simple-string = ${root.some-value} ""Hocon""  
}
";
            var config = Parser.Parse(hocon);
            var val = config.GetString("root.simple-string");
            Console.WriteLine("Hocon says: " + val);
            Console.ReadKey();
        }
    }
}
