using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Configuration.Hocon.Tests
{
    public static class ConfigurationProviderExtensions
    {
        public static string Get(this IConfigurationProvider provider, string key)
        {
            if (!provider.TryGet(key, out var value))
            {
                throw new InvalidOperationException("Key not found");
            }

            return value;
        }
    }
}
