using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hocon.Extensions.Configuration.Tests
{
    public class IncludeTest
    {
        [Fact]
        public void ProcessIncludes()
        {
            var hocon = @"{
                servers { include file(""servers.hocon"") }
            }";

            var hoconConfigSource = new HoconConfigurationSource
            {
                FileProvider = TestStreamHelpers.StringToFileProvider(hocon),
                IncludeCallback = (type, value) => Task.FromResult(@"{
    server1 = 10.0.0.1
    server2 = 10.0.0.2
}")
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.Add(hoconConfigSource);
            var config = configurationBuilder.Build();

            Assert.Equal("10.0.0.1", config["servers:server1"]);
            Assert.Equal("10.0.0.2", config["servers:server2"]);
        }
    }
}
