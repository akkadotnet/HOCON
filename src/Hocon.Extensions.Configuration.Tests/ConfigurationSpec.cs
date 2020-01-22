// -----------------------------------------------------------------------
// <copyright file="ConfigurationSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hocon.Extensions.Configuration.Tests
{
    public class ConfigurationSpec
    {
        private static readonly string ConfigString =
            ReadResource("Hocon.Extensions.Configuration.Tests.reference.conf");

        private static readonly string ModifiedConfigString =
            ReadResource("Hocon.Extensions.Configuration.Tests.reference_modified.conf");

        private static string ReadResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void ShouldBeAbleToReadHoconFile()
        {
            using (var fileSystem = new DisposableFileSystem())
            {
                fileSystem.CreateFile("reference.conf", ConfigString);
                var filePath = Path.Combine(fileSystem.RootPath, "reference.conf");
                var config = new ConfigurationBuilder().AddHoconFile(filePath, false, true).Build();
                Assert.Equal("0.0.1 Akka", config["akka:version"]);
                Assert.Equal("Akka.Actor.LocalActorRefProvider", config["akka:actor:provider"]);
                Assert.Equal("512", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);
            }
        }

        [Fact]
        public async Task ShouldReloadConfigurationOnFileChange()
        {
            using (var fileSystem = new DisposableFileSystem())
            {
                fileSystem.CreateFile("reference.conf", ConfigString);
                var filePath = Path.Combine(fileSystem.RootPath, "reference.conf");

                var config = new ConfigurationBuilder().AddHoconFile(filePath, false, true).Build();
                Assert.Equal("0.0.1 Akka", config["akka:version"]);
                Assert.Equal("Akka.Actor.LocalActorRefProvider", config["akka:actor:provider"]);
                Assert.Equal("512", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);

                fileSystem.CreateFile("reference.conf", ModifiedConfigString);
                // The default reload delay is 250 ms, add 100 ms to give the file monitoring system time to catch up and load the new config.
                await Task.Delay(350);
                var retries = 10;
                while (retries > 0)
                    try
                    {
                        Assert.Equal("0.0.2 Akka", config["akka:version"]);
                        Assert.Equal("Akka.Actor.RemoteActorRefProvider", config["akka:actor:provider"]);
                        Assert.Equal("1024", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);
                        break;
                    }
                    catch
                    {
                        retries--;
                        if (retries == 0) throw;
                        await Task.Delay(50);
                    }
            }
        }
    }
}