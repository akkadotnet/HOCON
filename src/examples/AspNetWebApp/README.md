# Hocon.Extension.Configuration Web Application Sample

This is an example on how to use `Hocon.Extension.Configuration` nuget package with an ASP.NET Core 3.1 Web Application project. 
The project file was made in Visual Studio 2019.

The code needed to inject the HOCON configuration file into the Web Application configuration singleton instance 
is located inside the `Program.cs` file.

```C#
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
			config.AddHoconFile(
				"appsettings.conf",
				optional: false,
				reloadOnChange: true)
			.AddHoconFile(
				$"appsettings.{env.EnvironmentName}.conf",
				optional: true,
				reloadOnChange: true);

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
```
