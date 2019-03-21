//-----------------------------------------------------------------------
// <copyright file="HoconConfigurationSource.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Configuration;

namespace Hocon.Extensions.Configuration
{
    /// <summary>
    /// Represents a HOCON file as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class HoconConfigurationSource : FileConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="HoconConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="HoconConfigurationProvider"/></returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new HoconConfigurationProvider(this);
        }
    }
}
