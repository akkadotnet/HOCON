//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Configuration;
using System;
using System.IO;
using Configuration.Hocon;
namespace ExternalIncludes
{
    class Program
    {
        static void Main(string[] args)
        {


            var hocon = @"
root {
  some-property {
    include ""ExternalHocon.txt"" 
  }
}
";
            //in this example we use a file resolver as the include mechanism
            //but could be replaced with e.g. a resolver for assembly resources
            Func<string, HoconRoot> fileResolver = null;
            fileResolver = fileName =>
            {
                var content = File.ReadAllText(fileName);
                var parsed = Parser.Parse(content, fileResolver);
                return parsed;
            };

            var config = ConfigurationFactory.ParseString(hocon, fileResolver);

            var val1 = config.GetInt("root.some-property.foo");
            var val2 = config.GetInt("root.some-property.bar");
            var val3 = config.GetInt("root.some-property.baz");

            Console.WriteLine("root.some-property.foo: " + val1);
            Console.WriteLine("root.some-property.bar: " + val2);
            Console.WriteLine("root.some-property.baz: " + val3);
            Console.ReadKey();
        }
    }
}
