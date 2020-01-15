// -----------------------------------------------------------------------
// <copyright file="TestConfigStrings.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

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