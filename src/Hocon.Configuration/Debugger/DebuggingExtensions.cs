// -----------------------------------------------------------------------
// <copyright file="DebuggingExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Text;


namespace Hocon.Debugger
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
        public static string DumpConfig(this Config c)
        {
            var sb = new StringBuilder();
            var hoconCount = 0;

            void AppendHocon(Config config, int i)
            {
                sb.AppendFormat("HOCON{0}", i)
                    .AppendLine()
                    .Append(config.PrettyPrint(2))
                    .AppendLine();
            }

            AppendHocon(c, hoconCount);

            var current = c;
            while (current.Fallback != null)
            {
                // add a header here
                sb.AppendLine().AppendLine("------------");
                current = current.Fallback;
                AppendHocon(current, ++hoconCount);
            }

            return sb.ToString();
        }
    }
}
