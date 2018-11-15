using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hocon.Extensions.Configuration.Tests
{
    public class ConfigurationSpec
    {
        [Fact(Skip = "Problem with CI test runner not running in the proper unit test folder.")]
        public void ShouldBeAbleToReadHoconFile()
        {
            var config = new ConfigurationBuilder().AddHoconFile("reference.conf", optional:false, reloadOnChange:true).Build();
            Assert.Equal("0.0.1 Akka", config["akka:version"]);
            Assert.Equal("Akka.Actor.LocalActorRefProvider", config["akka:actor:provider"]);
            Assert.Equal("512", config["akka:io:tcp:direct-buffer-pool:buffer-size"]);
        }

        [Fact(Skip = "Problem with CI test runner not running in the proper unit test folder.")]
        public void ShouldReloadConfigurationOnFileChange()
        {

        }
    }
}
