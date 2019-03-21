//-----------------------------------------------------------------------
// <copyright file="ConfigurationProviderExtensions.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Extensions.Configuration;

namespace Hocon.Extensions.Configuration.Tests
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
