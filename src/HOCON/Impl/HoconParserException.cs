using System;
using System.Globalization;

namespace Hocon
{
    public class HoconParserException : Exception
    {
        /// <summary>
        /// Gets the line number indicating where the error occurred.
        /// </summary>
        /// <value>The line number indicating where the error occurred.</value>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the line position indicating where the error occurred.
        /// </summary>
        /// <value>The line position indicating where the error occurred.</value>
        public int LinePosition { get; }

        /// <summary>
        /// Gets the path to the Hocon where the error occurred.
        /// </summary>
        /// <value>The path to the Hocon where the error occurred.</value>
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconParserException"/> class.
        /// </summary>
        public HoconParserException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconParserException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public HoconParserException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconParserException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public HoconParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconParserException"/> class
        /// with a specified error message, Hocon path, line number, line position, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path to the Hocon where the error occurred.</param>
        /// <param name="lineNumber">The line number indicating where the error occurred.</param>
        /// <param name="linePosition">The line position indicating where the error occurred.</param>
        public HoconParserException(string message, string path, int lineNumber, int linePosition)
            : base(message)
        {
            Path = path;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoconParserException"/> class
        /// with a specified error message, Hocon path, line number, line position, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path to the Hocon where the error occurred.</param>
        /// <param name="lineNumber">The line number indicating where the error occurred.</param>
        /// <param name="linePosition">The line position indicating where the error occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public HoconParserException(string message, string path, int lineNumber, int linePosition, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        internal static HoconParserException Create(Parser reader, string message)
        {
            return Create(reader, message, null);
        }

        internal static HoconParserException Create(Parser reader, string message, Exception ex)
        {
            return Create(reader, reader.Path, message, ex);
        }

        internal static HoconParserException Create(Parser lineInfo, string path, string message, Exception ex)
        {
            message = FormatMessage(lineInfo, path, message);

            int lineNumber;
            int linePosition;
            if (lineInfo != null)
            {
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }
            else
            {
                lineNumber = 0;
                linePosition = 0;
            }

            return new HoconParserException(message, path, lineNumber, linePosition, ex);
        }

        private static string FormatMessage(Parser lineInfo, string path, string message)
        {
            // don't add a fullstop and space when message ends with a new line
            if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                message = message.Trim();

                if (!message.EndsWith("."))
                {
                    message += ".";
                }

                message += " ";
            }

            message += string.Format(CultureInfo.InvariantCulture, "Path '{0}'", path);

            if (lineInfo != null)
            {
                message += string.Format(CultureInfo.InvariantCulture, ", line {0}, position {1}", lineInfo.LineNumber, lineInfo.LinePosition);
            }

            message += ".";

            return message;
        }
    }
}
