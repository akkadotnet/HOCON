// -----------------------------------------------------------------------
// <copyright file="ConfigurationProviderExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.Extensions.Configuration;

namespace Hocon.Extensions.Configuration.Tests
{
    public static class ConfigurationProviderExtensions
    {
        public static string Get(this IConfigurationProvider provider, string key)
        {
            if (!provider.TryGet(key, out var value)) throw new InvalidOperationException("Key not found");

            return value;
        }
    }
}