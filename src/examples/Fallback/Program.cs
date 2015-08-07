//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using System;

namespace Fallback
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseHocon = @"
root {
   some-property1 = 123
   some-property2 = 234  
}
";
            var userHocon = @"
root {
   some-property2 = 456  
   some-property3 = 789
}
";
            var baseConfig = ConfigurationFactory.ParseString(baseHocon);
            var userConfg = ConfigurationFactory.ParseString(userHocon);

            var merged = userConfg.WithFallback(baseConfig);

            var val1 = merged.GetString("root.some-property1");
            var val2 = merged.GetString("root.some-property2");
            var val3 = merged.GetString("root.some-property3");

            Console.WriteLine("root.some-property1 = {0}", val1);
            Console.WriteLine("root.some-property2 = {0}", val2);
            Console.WriteLine("root.some-property3 = {0}", val3);

            Console.ReadKey();
        }
    }
}
