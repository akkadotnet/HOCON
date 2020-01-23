// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Hocon;

namespace Fallback
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // The "environment-property" property is set through environment variable.
            // On debug mode, it is set inside the debug tab in the project properties.
            var baseHocon = @"
root {
   some-property1 = 123
   some-property2 = 234 
   environment-property = ${?ENV_MY_PROPERTY} 
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

            if (merged.HasPath("root.environment-property"))
            {
                var envVal = merged.GetString("root.environment-property");
                Console.WriteLine("root.environment-property = {0}", envVal);
            }

            Console.ReadKey();
        }
    }
}