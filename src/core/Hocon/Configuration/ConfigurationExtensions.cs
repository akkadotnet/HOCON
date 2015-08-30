#if DNXCORE50 || DNX451
using System;
using System.IO;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Configuration.Helper;

namespace Akka.Hocon.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddHoconFile(this IConfigurationBuilder configurationBuilder, string path, bool optional = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is null or empty " + nameof(path));
            }

            var fullPath = ConfigurationHelper.ResolveConfigurationFilePath(configurationBuilder, path);

            if (!optional && !File.Exists(fullPath))
            {
                throw new FileNotFoundException("Path not found: " + fullPath);
            }

            configurationBuilder.Add(new HoconConfigurationSource(fullPath, optional: optional));

            return configurationBuilder;
        }
    }
}
#endif
