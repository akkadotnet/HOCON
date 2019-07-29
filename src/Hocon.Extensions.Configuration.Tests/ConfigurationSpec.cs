//-----------------------------------------------------------------------
// <copyright file="ConfigurationSpec.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Hocon.Extensions.Configuration.Tests
{
    public class ConfigurationSpec
    {
        [Fact]
        public void ShouldBeAbleToReadHoconFile()
        {
            using (var fileSystem = new DisposableFileSystem())
            {
                fileSystem.CreateFile("reference.conf", ConfigString);
                var filePath = Path.Combine(fileSystem.RootPath, "reference.conf");
                var config = new ConfigurationBuilder().AddHoconFile(filePath, optional: false, reloadOnChange: true).Build();
                Assert.Equal("0.0.1 Akka", config["akka:version"]);
                Assert.Equal("Akka.Actor.LocalActorRefProvider", config["akka:actor:provider"]);
                Assert.Equal("512", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);
            }
        }

        [Fact]
        public void ShouldReloadConfigurationOnFileChange()
        {
            using (var fileSystem = new DisposableFileSystem())
            {
                fileSystem.CreateFile("reference.conf", ConfigString);
                var filePath = Path.Combine(fileSystem.RootPath, "reference.conf");

                var config = new ConfigurationBuilder().AddHoconFile(filePath, optional: false, reloadOnChange: true).Build();
                Assert.Equal("0.0.1 Akka", config["akka:version"]);
                Assert.Equal("Akka.Actor.LocalActorRefProvider", config["akka:actor:provider"]);
                Assert.Equal("512", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);

                fileSystem.CreateFile("reference.conf", ModifiedConfigString);
                // The default reload delay is 250 ms, add 100 ms to give the file monitoring system time to catch up and load the new config.
                Task.Delay(350).Wait();
                Assert.Equal("0.0.2 Akka", config["akka:version"]);
                Assert.Equal("Akka.Actor.RemoteActorRefProvider", config["akka:actor:provider"]);
                Assert.Equal("1024", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);
            }
        }

        private static readonly string ConfigString = ReadResource("Hocon.Extensions.Configuration.Tests.reference.conf");
        private static readonly string ModifiedConfigString = ReadResource("Hocon.Extensions.Configuration.Tests.reference_modified.conf");

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
