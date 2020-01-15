// -----------------------------------------------------------------------
// <copyright file="HoconConfigurationProvider.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Hocon.Extensions.Configuration
{
    /// <summary>
    ///     A HOCON file based <see cref="FileConfigurationProvider" />.
    /// </summary>
    public class HoconConfigurationProvider : FileConfigurationProvider
    {
        /// <summary>
        ///     Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public HoconConfigurationProvider(HoconConfigurationSource source) : base(source)
        {
        }

        /// <summary>
        ///     Loads the HOCON data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public override void Load(Stream stream)
        {
            var parser = new HoconConfigurationFileParser();
            try
            {
                Data = parser.Parse(stream);
            }
            catch (HoconParserException e)
            {
                string errorLine = string.Empty;
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var streamReader = new StreamReader(stream))
                    {
                        var fileContent = ReadLines(streamReader);
                        errorLine = RetrieveErrorContext(e, fileContent);
                    }
                }

                throw new FormatException(string.Format(Resources.Error_HOCONParseError, e.LineNumber, errorLine), e);
            }
        }

        private static string RetrieveErrorContext(HoconParserException e, IEnumerable<string> fileContent)
        {
            string errorLine = null;
            if (e.LineNumber >= 2)
            {
                var errorContext = fileContent.Skip(e.LineNumber - 2).Take(2).ToList();
                // Handle situations when the line number reported is out of bounds
                if (errorContext.Count >= 2)
                    errorLine = errorContext[0].Trim() + Environment.NewLine + errorContext[1].Trim();
            }

            if (string.IsNullOrEmpty(errorLine))
            {
                var possibleLineContent = fileContent.Skip(e.LineNumber - 1).FirstOrDefault();
                errorLine = possibleLineContent ?? string.Empty;
            }

            return errorLine;
        }

        private static IEnumerable<string> ReadLines(StreamReader streamReader)
        {
            string line;
            do
            {
                line = streamReader.ReadLine();
                yield return line;
            } while (line != null);
        }
    }
}