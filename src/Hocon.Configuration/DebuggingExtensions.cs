// -----------------------------------------------------------------------
// <copyright file="DebuggingExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

namespace Hocon
{
    /// <summary>
    /// Debugging extensions for <see cref="Config"/> objects.
    /// </summary>
    public static class DebuggingExtensions
    {
        /// <summary>
        /// Dumps all of the fallbacks in the order in which they would be resolved.
        /// </summary>
        /// <param name="c">The top-level config.</param>
        /// <returns>A stringified list of fallbacks</returns>
        public static string DumpConfig(this Config c, bool dumpAsFallbacks = true)
        {
            var sb = new StringBuilder();
            if(!dumpAsFallbacks)
            {
                sb.AppendLine(c.Root.ToString(1, 2));
                return sb.ToString();
            }

            var hoconCount = 0;

            void AppendHocon(HoconValue value, int i)
            {
                sb.AppendFormat("HOCON{0}", i)
                    .AppendLine()
                    .Append(value.ToString(1, 2))
                    .AppendLine();
            }

            AppendHocon(c.Value, hoconCount);

            foreach(var fallback in c.Fallbacks)
            {
                // add a header here
                sb.AppendLine().AppendLine("------------");
                AppendHocon(fallback, ++hoconCount);
            }

            return sb.ToString();
        }
    }
}
