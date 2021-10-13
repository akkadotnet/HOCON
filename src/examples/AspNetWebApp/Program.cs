using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Hocon.Extensions.Configuration;

namespace AspNetWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                   {
                       // We inject the HOCON configuration file using this function call,
                       // the rest of the code are there to make sure that the final configuration
                       // conforms to the Microsoft standard on loading a full configuration stack.
                       var env = hostingContext.HostingEnvironment;
                       config.AddHoconFile("appsettings.conf", optional: false, reloadOnChange: true)
                           .AddHoconFile($"appsettings.{env.EnvironmentName}.conf", optional: true, reloadOnChange: true);
                       if (env.EnvironmentName == "Development")
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
                });
    }
}
