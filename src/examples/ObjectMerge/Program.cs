// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Hocon;

namespace ObjectMerge
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hocon = @"
root {
  some-object.property1 = 123
  some-object.property2 = 456
  some-object {
     property3 = 789
  }
}
";
            var config = HoconParser.Parse(hocon);
            var val1 = config.GetString("root.some-object.property1");
            var val2 = config.GetString("root.some-object.property2");
            var val3 = config.GetString("root.some-object.property3");
            Console.WriteLine("root.some-object.property1 = {0}", val1);
            Console.WriteLine("root.some-object.property2 = {0}", val2);
            Console.WriteLine("root.some-object.property3 = {0}", val3);
            Console.ReadKey();
        }
    }
}