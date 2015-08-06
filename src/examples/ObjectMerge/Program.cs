//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Configuration;
using System;

namespace ObjectMerge
{
    class Program
    {
        static void Main(string[] args)
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
            var config = ConfigurationFactory.ParseString(hocon);
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