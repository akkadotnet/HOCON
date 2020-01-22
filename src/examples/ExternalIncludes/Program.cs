// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Hocon;

namespace ExternalIncludes
{
    internal class Program
    {
        private static string ReadResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static void Main(string[] args)
        {
            var hocon = @"
root {
  some-property {
    include file(""ExternalHocon.txt"") 
  }
}
";

            //in this example we use a multi resolver as the include mechanism
            async Task<string> ConfigResolver(HoconCallbackType type, string fileName)
            {
                switch (type)
                {
                    case HoconCallbackType.Resource:
                        return ReadResource(fileName);
                    case HoconCallbackType.File:
                        return await File.ReadAllTextAsync(fileName);
                    default:
                        return null;
                }
            }

            var config = Parser.Parse(hocon, ConfigResolver);

            var val1 = config.GetInt("root.some-property.foo");
            var val2 = config.GetInt("root.some-property.bar");
            var val3 = config.GetInt("root.some-property.baz");

            Console.WriteLine("Hocon loaded from file:");
            Console.WriteLine("root.some-property.foo: " + val1);
            Console.WriteLine("root.some-property.bar: " + val2);
            Console.WriteLine("root.some-property.baz: " + val3);

            var hocon2 = @"include classpath(""ExternalIncludes.reference.conf"")";
            config = Parser.Parse(hocon2, ConfigResolver);

            Console.WriteLine();
            Console.WriteLine("Hocon loaded from resource path:");
            Console.WriteLine(config.PrettyPrint(2));
            Console.ReadKey();
        }
    }
}