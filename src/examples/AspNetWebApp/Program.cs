using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Hocon.Extensions.Configuration;

namespace AspNetWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // We inject the HOCON configuration file using this function call,
                    // the rest of the code are there to make sure that the final configuration
                    // conforms to the Microsoft standard on loading a full configuration stack.
                    var env = hostingContext.HostingEnvironment;
                    config.AddHoconFile("appsettings.conf", optional: false, reloadOnChange: true)
                        .AddHoconFile($"appsettings.{env.EnvironmentName}.conf", optional: true, reloadOnChange: true);

                    if (env.IsDevelopment())
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        if (appAssembly != null)
                        {
                            config.AddUserSecrets(appAssembly, optional: true);
                        }
                    }

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                });
    }
}
