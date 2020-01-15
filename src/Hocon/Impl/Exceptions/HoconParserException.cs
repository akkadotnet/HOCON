// -----------------------------------------------------------------------
// <copyright file="HoconParserException.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;

namespace Hocon
{
    public sealed class HoconParserException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HoconParserException" /> class.
        /// </summary>
        public HoconParserException()
        {
        }

        internal HoconParserException(IHoconLineInfo info)
        {
            LineNumber = info.LineNumber;
            LinePosition = info.LinePosition;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HoconParserException" /> class
        ///     with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public HoconParserException(string message) : base(message)
        {
        }

        internal HoconParserException(IHoconLineInfo info, string message) : base(message)
        {
            LineNumber = info.LineNumber;
            LinePosition = info.LinePosition;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HoconParserException" /> class
        ///     with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or <c>null</c> if no inner
        ///     exception is specified.
        /// </param>
        public HoconParserException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal HoconParserException(IHoconLineInfo info, string message, Exception innerException) : base(message,
            innerException)
        {
            LineNumber = info.LineNumber;
            LinePosition = info.LinePosition;
        }

        public int LineNumber { get; }
        public int LinePosition { get; }

        internal static HoconParserException Create(IHoconLineInfo lineInfo, HoconPath path, string message)
        {
            return Create(lineInfo, path, message, null);
        }

        internal static HoconParserException Create(IHoconLineInfo lineInfo, HoconPath path, string message,
            Exception ex)
        {
            message = FormatMessage(lineInfo, path, message);
            return new HoconParserException(lineInfo, message, ex);
        }

        private static string FormatMessage(IHoconLineInfo lineInfo, HoconPath path, string message)
        {
            var sb = new StringBuilder();

            // don't add a fullstop and space when message ends with a new line
            if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                sb.Append(message.Trim());

                if (!message.EndsWith("."))
                    sb.Append(".");

                sb.Append(" ");
            }

            var addComma = false;
            if (path != null)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "At path '{0}'", path));
                addComma = true;
            }

            if (lineInfo != null)
            {
                sb.Append(addComma ? ", " : "At ");
                sb.Append(string.Format(CultureInfo.InvariantCulture, "line {0}, position {1}", lineInfo.LineNumber,
                    lineInfo.LinePosition));
            }

            sb.Append(".");

            return sb.ToString();
        }
    }
}