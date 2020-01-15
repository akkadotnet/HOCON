// -----------------------------------------------------------------------
// <copyright file="HoconToken.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

namespace Hocon
{
    /// <summary>
    ///     This enumeration defines the different types of tokens found within
    ///     a HOCON (Human-Optimized Config Object Notation) configuration string.
    /// </summary>
    internal enum TokenType
    {
        /// <summary>
        ///     This token type represents the end of the configuration string.
        /// </summary>
        EndOfFile,

        /// <summary>
        ///     This token type represents the beginning of an object, <c>{</c> .
        /// </summary>
        StartOfObject,

        /// <summary>
        ///     This token type represents the end of an object, <c>}</c> .
        /// </summary>
        EndOfObject,

        /// <summary>
        ///     This token type represents the beginning of an array, <c>[</c> .
        /// </summary>
        StartOfArray,

        /// <summary>
        ///     This token type represents the end of an array, <c>]</c> .
        /// </summary>
        EndOfArray,

        /// <summary>
        ///     This token type represents the opening parenthesis, <c>(</c> .
        /// </summary>
        ParenthesisStart,

        /// <summary>
        ///     This token type represents the closing parenthesis, <c>)</c> .
        /// </summary>
        ParenthesisEnd,

        /// <summary>
        ///     This token type represents a comment.
        /// </summary>
        Comment,

        /// <summary>
        ///     This token type represents the value portion of a key-value pair.
        /// </summary>
        LiteralValue,

        /// <summary>
        ///     This token type represents the assignment operator, <c>+=</c>.
        /// </summary>
        PlusEqualAssign,

        /// <summary>
        ///     This token type represents the assignment operator, <c>=</c> or <c>:</c> .
        /// </summary>
        Assign,

        /// <summary>
        ///     This token type represents the separator in an array, <c>,</c> .
        /// </summary>
        Comma,

        /// <summary>
        ///     This token type represents the start of a replacement variable, <c>${</c> .
        /// </summary>
        SubstituteRequired,

        /// <summary>
        ///     This token type represents the start of a replacement variable with question mark, <c>${?</c> .
        /// </summary>
        SubstituteOptional,

        /// <summary>
        ///     This token type represents a newline character, <c>\n</c> .
        /// </summary>
        EndOfLine,

        /// <summary>
        ///     This token type represents the include directive.
        /// </summary>
        Include,

        /// <summary>
        ///     This token type represents the required() directive.
        /// </summary>
        Required,

        /// <summary>
        ///     This token type represents the url() directive.
        /// </summary>
        Url,

        /// <summary>
        ///     This token type represents the file() directive.
        /// </summary>
        File,

        /// <summary>
        ///     This token type represents the classpath() directive.
        /// </summary>
        Classpath,

        /// <summary>
        ///     This token type represents a tokenizer error.
        /// </summary>
        Error
    }

    internal enum TokenLiteralType
    {
        None,
        Null,
        Whitespace,
        UnquotedLiteralValue,
        QuotedLiteralValue,
        TripleQuotedLiteralValue,
        Bool,
        Long,
        Double,
        Hex,
        Octal
    }

    /// <summary>
    ///     This class represents a token within a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    /// </summary>
    internal sealed class Token : IHoconLineInfo
    {
        public static readonly Token Empty = new Token();

        //for serialization
        private Token()
        {
        }

        public Token(string value, TokenType type, TokenLiteralType literalType, IHoconLineInfo source)
        {
            Type = type;
            LiteralType = literalType;
            Value = value;

            if (source != null)
            {
                LineNumber = source.LineNumber;
                LinePosition = source.LinePosition - (value?.Length ?? 0);
            }
        }

        public Token(string value, TokenType type, IHoconLineInfo source)
            : this(value, type, TokenLiteralType.None, source)
        {
        }

        /// <summary>
        ///     The value associated with this token. If this token is
        ///     a <see cref="TokenType.LiteralValue" />, then this property
        ///     holds the string literal.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     The type that represents this token.
        /// </summary>
        public TokenType Type { get; }

        public TokenLiteralType LiteralType { get; }

        public int LineNumber { get; }
        public int LinePosition { get; }

        public override string ToString()
        {
            return $"Type:{Type}, Value:{Value ?? "null"}, Num:{LineNumber}, Pos:{LinePosition}";
        }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="type">The type of token to associate with.</param>
        /// <param name="source">The <see cref="IHoconLineInfo"/> of this <see cref="Token"/>, used for exception generation purposes.</param>
        public Token(TokenType type, IHoconLineInfo source) 
            : this(null, type, TokenLiteralType.None, source)
        { }
        */

        /// <summary>
        ///     Creates a substitution token with a given <paramref name="path" />.
        /// </summary>
        /// <param name="path">The path to associate with this token.</param>
        /// <param name="source">
        ///     The <see cref="IHoconLineInfo" /> of this <see cref="Token" />, used for exception generation
        ///     purposes.
        /// </param>
        /// <param name="questionMarked">Designate whether the substitution <see cref="Token" /> was declared as `${?`.</param>
        /// <returns>A substitution token with the given path.</returns>
        public static Token Substitution(string path, IHoconLineInfo source, bool questionMarked)
        {
            return new Token(path, questionMarked ? TokenType.SubstituteOptional : TokenType.SubstituteRequired,
                TokenLiteralType.None, source);
        }

        /// <summary>
        ///     Creates a string literal token with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">The value to associate with this token.</param>
        /// <param name="literalType">The <see cref="TokenLiteralType" /> of this <see cref="Token" />.</param>
        /// <param name="source">
        ///     The <see cref="IHoconLineInfo" /> of this <see cref="Token" />, used for exception generation
        ///     purposes.
        /// </param>
        /// <returns>A string literal token with the given value.</returns>
        public static Token LiteralValue(string value, TokenLiteralType literalType, IHoconLineInfo source)
        {
            return new Token(value, TokenType.LiteralValue, literalType, source);
        }

        public static Token QuotedLiteralValue(string value, IHoconLineInfo source)
        {
            return LiteralValue(value, TokenLiteralType.QuotedLiteralValue, source);
        }

        public static Token TripleQuotedLiteralValue(string value, IHoconLineInfo source)
        {
            return LiteralValue(value, TokenLiteralType.TripleQuotedLiteralValue, source);
        }

        public static Token Include(string path, IHoconLineInfo source)
        {
            return new Token(path, TokenType.Include, TokenLiteralType.None, source);
        }

        public static Token Error(IHoconLineInfo source)
        {
            return new Token("", TokenType.Error, TokenLiteralType.None, source);
        }
    }
}