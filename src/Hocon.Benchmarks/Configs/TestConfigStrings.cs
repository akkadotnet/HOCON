#region copyright
// -----------------------------------------------------------------------
//  <copyright file="TestConfigStrings.cs" company="Hocon Project">
//      Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//      Copyright (C) 2018-2018 Akka.NET project <https://github.com/akkadotnet/hocon>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System.IO;
using System.Reflection;

namespace Hocon.Benchmarks.Configs
{
    public static class TestConfigStrings
    {
        public static readonly string AkkaConfigString = ReadResource("Hocon.Benchmarks.reference.conf");

        private static string ReadResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}