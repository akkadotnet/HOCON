//-----------------------------------------------------------------------
// <copyright file="HoconTokenizer.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Hocon
{
    /// <summary>
    /// This class contains methods used to tokenize a string.
    /// </summary>
    internal abstract class Tokenizer : IHoconLineInfo
    {
        private readonly Stack<int> _indexStack = new Stack<int>();

        private readonly string _text;
        private int _index;

        protected void PushIndex() => _indexStack.Push(Index);
        protected void ResetIndex() => Index = _indexStack.Pop();
        protected void PopIndex() => _indexStack.Pop();

        public int Length => _text.Length;

        public int Index
        {
            get => _index;
            private set
            {
                _index = value;
                if (_index > _text.Length)
                    _index = _text.Length;
            }
        }

        public int LineNumber { get; private set; } = 1;
        public int LinePosition { get; private set; } = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="text">The string that contains the text to tokenize.</param>
        protected Tokenizer(string text)
        {
            _text = text;
        }

        /// <summary>
        /// A value indicating whether the tokenizer has reached the end of the string.
        /// </summary>
        protected bool EoF => Index >= _text.Length;

        /// <summary>
        /// Determines whether the given pattern matches the value at the current
        /// position of the tokenizer.
        /// </summary>
        /// <param name="pattern">The string that contains the characters to match.</param>
        /// <returns><c>true</c> if the pattern matches, otherwise <c>false</c>.</returns>
        protected bool Matches(string pattern)
        {
            if (pattern.Length + Index > _text.Length)
                return false;

            for (var i = 0; i < pattern.Length; ++i)
            {
                if (pattern[i] != _text[Index + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether any of the given patterns match the value at the current
        /// position of the tokenizer.
        /// </summary>
        /// <param name="patterns">The string array that contains the characters to match.</param>
        /// <returns><c>true</c> if any one of the patterns match, otherwise <c>false</c>.</returns>
        protected bool Matches(params string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.Length + Index > _text.Length)
                    continue;

                var match = true;
                for (var i = 0; i < pattern.Length; ++i)
                {
                    if (pattern[i] == _text[Index + i]) continue;
                    match = false;
                    break;
                }
                if(match)
                    return true;
            }
            return false;
        }

        protected bool Matches(char pattern)
        {
            if (EoF)
                return false;

            return _text[Index] == pattern;
        }

        protected bool Matches(params char[] patterns)
        {
            if (EoF)
                return false;

            foreach (var pattern in patterns)
            {
                if (_text[Index] == pattern)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the next character in the tokenizer.
        /// </summary>
        /// <returns>The character at the tokenizer's current position.</returns>
        protected void Take()
        {
            Index++;
            if (EoF) return;
            LinePosition++;
            if (_text[Index] == Utils.NewLine)
            {
                LineNumber++;
                LinePosition = 1;
            }
        }

        /// <summary>
        /// Retrieves a string of the given length from the current position of the tokenizer.
        /// </summary>
        /// <param name="length">The length of the string to return.</param>
        /// <returns>
        /// The string of the given length. If the length exceeds where the
        /// current index is located, then null is returned.
        /// </returns>
        protected void Take(int length)
        {
            if (Index + length > _text.Length)
                return;

            for (var i = 0; i < length; ++i)
            {
                Take();
            }
        }

        protected string TakeWithResult(int length)
        {
            if (Index + length > _text.Length)
                return null;

            var s = _text.Substring(Index, length);
            Index += length;
            return s;
        }

        /// <summary>
        /// Retrieves the next character in the tokenizer without advancing its position.
        /// </summary>
        /// <returns>The character at the tokenizer's current position.</returns>
        protected char Peek => EoF ? (char) 0 : _text[Index];

        protected char PeekAndTake()
        {
            if (EoF)
                return (char)0;
            Take();
            return _text[Index - 1];
        }

        protected void PullWhitespaces()
        {
            while (!EoF && Peek.IsWhitespaceWithNoNewLine())
            {
                Take();
            }
        }
    }


    /// <summary>
    /// This class contains methods used to tokenize HOCON (Human-Optimized Config Object Notation)
    /// configuration strings.
    /// </summary>
    internal sealed class HoconTokenizer : Tokenizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HoconTokenizer"/> class.
        /// </summary>
        /// <param name="text">The string that contains the text to tokenize.</param>
        public HoconTokenizer(string text)
            : base(text) { }

        public HoconTokenizerResult Tokenize()
        {
            var tokens = Tokenize(TokenType.EndOfFile);
            tokens.Add(new Token("", TokenType.EndOfFile, this));
            return tokens;
        }

        private HoconTokenizerResult Tokenize(TokenType closingTokenType)
        {
            var tokens = new HoconTokenizerResult();

            while (!EoF)
            {
                switch (Peek)
                {
                    case '{':
                        Take();
                        tokens.Add(new Token("{", TokenType.StartOfObject, this));
                        tokens.AddRange(Tokenize(TokenType.EndOfObject));
                        continue;

                    case '}':
                        Take();
                        tokens.Add(new Token("}", TokenType.EndOfObject, this));
                        if (closingTokenType != tokens[tokens.Count - 1].Type)
                            throw new HoconTokenizerException(
                                $"Expected {closingTokenType}, found {tokens[tokens.Count - 1].Type} instead.",
                                tokens[tokens.Count - 1]);
                        return tokens;

                    case '[':
                        Take();
                        tokens.Add(new Token("[", TokenType.StartOfArray, this));
                        tokens.AddRange(Tokenize(TokenType.EndOfArray));
                        continue;

                    case ']':
                        Take();
                        tokens.Add(new Token("]", TokenType.EndOfArray, this));
                        if (closingTokenType != tokens[tokens.Count - 1].Type)
                            throw new HoconTokenizerException(
                                $"Expected {closingTokenType}, found {tokens[tokens.Count - 1].Type} instead.",
                                tokens[tokens.Count - 1]);
                        return tokens;

                    case ',':
                        Take();
                        tokens.Add(new Token(",", TokenType.Comma, TokenLiteralType.UnquotedLiteralValue, this));
                        continue;

                    case ':':
                    case '=':
                        var c = PeekAndTake();
                        tokens.Add(new Token(c.ToString(), TokenType.Assign, this));
                        continue;

                    case '+':
                        if (PullPlusEqualAssignment(tokens))
                            continue;
                        break;

                    case '/':
                    case '#':
                        if (PullComment(tokens))
                            continue;
                        break;

                    case '$':
                        if (PullSubstitution(tokens))
                            continue;
                        break;

                    case '\n':
                        Take();
                        tokens.Add(new Token("\n", TokenType.EndOfLine, this));
                        continue;
                    case 'i':
                        if (PullInclude(tokens))
                            continue;
                        break;
                }

                if (PullNonNewLineWhitespace(tokens))
                    continue;
                if (PullLiteral(tokens))
                    continue;

                throw new HoconTokenizerException($"Invalid token at index {Index}", Token.Error(this));
            }

            if(closingTokenType != TokenType.EndOfFile)
                throw new HoconTokenizerException(
                    $"Expected {closingTokenType}, found {TokenType.EndOfFile} instead.", tokens[tokens.Count - 1]);
            return tokens;
        }

        /// <summary>
        /// Retrieves a <see cref="TokenType.PlusEqualAssign"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.PlusEqualAssign"/> token from the tokenizer's current position.</returns>
        private bool PullPlusEqualAssignment(HoconTokenizerResult tokens)
        {
            if (!Matches("+="))
                return false;

            Take(2);
            tokens.Add(new Token("+=", TokenType.PlusEqualAssign, this));
            return true;
        }

        /// <summary>
        /// Retrieves a <see cref="TokenType.Comment"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.EndOfLine"/> token from the tokenizer's last position, discarding the comment.</returns>
        private bool PullComment(HoconTokenizerResult tokens)
        {
            if (!Matches("//", "#"))
                return false;

            var comment = DiscardRestOfLine();

            //tokens.Add(new Token(TokenType.Comment, this, start, Index - start));
            tokens.Add(new Token(comment, TokenType.EndOfLine, this));
            return true;
        }

        private bool PullInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("include"))
                return false;

            PushIndex();
            var includeTokens = new HoconTokenizerResult();

            Take("include".Length);
            includeTokens.Add(new Token("include", TokenType.Include, this));
            PullWhitespaces();

            var parenCount = 0;
            if (PullRequired(includeTokens))
            {
                if (!PullParenthesisStart(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            }

            if (PullFileInclude(includeTokens))
            {
                if (!PullParenthesisStart(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            } else if (PullUrlInclude(includeTokens))
            {
                if (!PullParenthesisStart(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            } else if (PullResourceInclude(includeTokens))
            {
                if (!PullParenthesisStart(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            }

            if (!PullQuotedText(includeTokens))
            {
                ResetIndex();
                return false;
            }

            for (; parenCount > 0; --parenCount)
            {
                if (!PullParenthesisEnd(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
            }

            PopIndex();
            tokens.AddRange(includeTokens);
            return true;
        }

        private bool PullParenthesisStart(HoconTokenizerResult tokens)
        {
            if (Peek != '(')
                return false;

            Take();
            tokens.Add(new Token("(", TokenType.ParenthesisStart, this));
            PullWhitespaces();
            return true;
        }

        private bool PullParenthesisEnd(HoconTokenizerResult tokens)
        {
            if (Peek != ')')
                return false;

            Take();
            tokens.Add(new Token(")", TokenType.ParenthesisEnd, this));
            PullWhitespaces();
            return true;
        }

        private bool PullRequired(HoconTokenizerResult tokens)
        {
            if (!Matches("required"))
                return false;

            Take("required".Length);
            tokens.Add(new Token("required", TokenType.Required, this));
            PullWhitespaces();
            return true;

        }

        private bool PullUrlInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("url"))
                return false;

            Take("url".Length);
            tokens.Add(new Token("url", TokenType.Url, this));
            PullWhitespaces();
            return true;
        }

        private bool PullFileInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("file"))
                return false;

            Take("file".Length);
            tokens.Add(new Token("file", TokenType.File, this));
            PullWhitespaces();
            return true;
        }

        private bool PullResourceInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("classpath"))
                return false;

            Take("classpath".Length);
            tokens.Add(new Token("classpath", TokenType.Classpath, this));
            PullWhitespaces();
            return true;
        }

        private string PullEscapeSequence()
        {
            Take(); //consume "\"
            char escaped = PeekAndTake();
            switch (escaped)
            {
                case '"':
                    return ("\"");
                case '\\':
                    return ("\\");
                case '/':
                    return ("/");
                case 'b':
                    return ("\b");
                case 'f':
                    return ("\f");
                case 'n':
                    return ("\n");
                case 'r':
                    return ("\r");
                case 't':
                    return ("\t");
                case 'u':
                    string hex = $"0x{TakeWithResult(4)}";
                    try
                    {
                        int j = Convert.ToInt32(hex, 16);
                        return ((char) j).ToString();
                    }
                    catch
                    {
                        throw new HoconTokenizerException($"Invalid unicode escape code `{escaped}`", Token.Error(this));
                    }
                default:
                    throw new HoconTokenizerException($"Unknown escape code `{escaped}`", Token.Error(this));
            }
        }

        /// <summary>
        /// Retrieves a <see cref="TokenType.SubstituteRequired"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.SubstituteRequired"/> token from the tokenizer's current position.</returns>
        private bool PullSubstitution(HoconTokenizerResult tokens)
        {
            bool questionMarked = false;
            if (Matches("${?"))
            {
                Take(3);
                questionMarked = true;
            }
            else if (Matches("${"))
            {
                Take(2);
            }
            else
            {
                return false;
            }

            var sb = new StringBuilder();
            while (!EoF && !Matches("}"))
            {
                sb.Append(PeekAndTake());
            }

            if (EoF)
                throw new HoconTokenizerException("Expected end of substitution but found EoF", Token.Error(this));
            Take();

            tokens.Add(Token.Substitution(sb.ToString().TrimWhitespace(), this, questionMarked));
            return true;
        }

        private bool PullNonNewLineWhitespace(HoconTokenizerResult tokens)
        {
            if (!Peek.IsWhitespaceWithNoNewLine())
                return false;

            var sb = new StringBuilder();
            while (Peek.IsWhitespaceWithNoNewLine())
            {
                sb.Append(PeekAndTake());
            }
            tokens.Add(Token.LiteralValue(sb.ToString(), TokenLiteralType.Whitespace, this));
            return true;
        }

        private bool PullLiteral(HoconTokenizerResult tokens)
        {
            switch (Peek)
            {
                case '\'':
                case '"':
                    if (PullTripleQuotedText(tokens) || PullQuotedText(tokens))
                        return true;
                    throw new HoconTokenizerException("Expected closing quote.", Token.Error(this));
                case '-':
                case '+':
                    return PullInfinity(tokens) ||
                           PullNumbers(tokens) ||
                           PullUnquotedText(tokens);
                case '0':
                    return PullHexadecimal(tokens) ||
                           PullOctal(tokens) ||
                           PullNumbers(tokens) ||
                           PullUnquotedText(tokens);
                case '.':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return PullNumbers(tokens) ||
                           PullUnquotedText(tokens);
                case 'I':
                    return PullInfinity(tokens) ||
                           PullUnquotedText(tokens);
                case 'N':
                    return PullNaN(tokens) ||
                           PullUnquotedText(tokens);
                case 't':
                case 'y':
                case 'o':
                case 'f':
                    return PullBool(tokens) ||
                           PullUnquotedText(tokens);
                case 'n':
                    return PullNull(tokens) ||
                           PullBool(tokens) ||
                           PullUnquotedText(tokens);
                default:
                    return PullUnquotedText(tokens);
            }
        }

        private bool PullNull(HoconTokenizerResult tokens)
        {
            if (!Matches("null"))
                return false;

            Take(4);
            tokens.Add(Token.LiteralValue("null", TokenLiteralType.Null, this));
            return true;
        }

        private bool PullInfinity(HoconTokenizerResult tokens)
        {
            if (Matches("-Infinity"))
            {
                Take("-Infinity".Length);
                tokens.Add(Token.LiteralValue("-Infinity", TokenLiteralType.Double, this));
                return true;
            }

            if (Matches("Infinity"))
            {
                Take("Infinity".Length);
                tokens.Add(Token.LiteralValue("Infinity", TokenLiteralType.Double, this));
                return true;
            }

            if (Matches("+Infinity"))
            {
                Take("+Infinity".Length);
                tokens.Add(Token.LiteralValue("Infinity", TokenLiteralType.Double, this));
                return true;
            }

            return false;
        }

        private bool PullNaN(HoconTokenizerResult tokens)
        {
            if (Matches("NaN"))
            {
                Take(3);
                tokens.Add(Token.LiteralValue("NaN", TokenLiteralType.Double, this));
                return true;
            }

            return false;
        }

        private bool PullHexadecimal(HoconTokenizerResult tokens)
        {
            if (!Matches("0x", "0X", "&h", "&H"))
                return false;

            PushIndex();
            var sb = new StringBuilder();
            Take(2);
            sb.Append("0x");

            while (Peek.IsHexadecimal())
            {
                sb.Append(PeekAndTake());
            }
            try
            {
                Convert.ToInt64(sb.ToString(), 16);
            }
            catch
            {
                ResetIndex();
                return false;
            }

            PopIndex();
            tokens.Add(Token.LiteralValue(sb.ToString(), TokenLiteralType.Hex, this));
            return true;
        }

        private bool PullOctal(HoconTokenizerResult tokens)
        {
            PushIndex();
            var sb = new StringBuilder();
            sb.Append(PeekAndTake());

            while (Peek.IsOctal())
            {
                sb.Append(PeekAndTake());
            }
            try
            {
                Convert.ToInt64(sb.ToString(), 8);
            }
            catch
            {
                ResetIndex();
                return false;
            }

            PopIndex();
            tokens.Add(Token.LiteralValue(sb.ToString(), TokenLiteralType.Octal, this));
            return true;
        }

        enum NumberTokenizerState
        {
            Coefficient,
            Significand,
            Exponent
        }

        private bool PullNumbers(HoconTokenizerResult tokens)
        {
            var sb = new StringBuilder();

            // Parse numbers
            var parsing = true;
            Token lastValidToken = null;
            var state = NumberTokenizerState.Coefficient;
            while (parsing)
            {
                switch (state)
                {
                    case NumberTokenizerState.Coefficient:
                        // possible double number without coefficient
                        if (Matches("-.", "+.", "."))
                        {
                            state = NumberTokenizerState.Significand;
                            break;
                        }

                        PushIndex(); // long test index
                        if (Matches('+', '-'))
                            sb.Append(PeekAndTake());

                        // numbers could not start with a 0
                        if (!Peek.IsDigit() || Peek == '0')
                        {
                            ResetIndex(); // reset long test index
                            parsing = false;
                            break;
                        }
                        while (Peek.IsDigit())
                        {
                            sb.Append(PeekAndTake());
                        }
                        if (!long.TryParse(sb.ToString(), out _))
                        {
                            ResetIndex(); // reset long test index
                            parsing = false;
                            break;
                        }
                        PopIndex(); // end long test index

                        lastValidToken = Token.LiteralValue(sb.ToString(), TokenLiteralType.Long, this);
                        state = NumberTokenizerState.Significand;
                        break;

                    case NumberTokenizerState.Significand:
                        // short logic, no significand, but probably have an exponent
                        if (!Matches("-.", "+.", "."))
                        {
                            state = NumberTokenizerState.Exponent;
                            break;
                        }

                        PushIndex(); // validate significand in number test
                        if (Matches('+', '-'))
                            sb.Insert(0, PeekAndTake());

                        sb.Append(PeekAndTake());
                        if (!Peek.IsDigit())
                        {
                            ResetIndex(); // reset validate significand in number test
                            parsing = false;
                            break;
                        }
                        while (Peek.IsDigit())
                        {
                            sb.Append(PeekAndTake());
                        }
                        if (!double.TryParse(sb.ToString(), out _))
                        {
                            ResetIndex(); // reset validate significand in number test
                            parsing = false;
                            break;
                        }
                        PopIndex(); // end validate significand in number test

                        lastValidToken = Token.LiteralValue(sb.ToString(), TokenLiteralType.Double, this);
                        state = NumberTokenizerState.Exponent;
                        break;

                    case NumberTokenizerState.Exponent:
                        // short logic, check if number is a double with exponent
                        if (!Matches('e', 'E'))
                        {
                            parsing = false;
                            break;
                        }

                        PushIndex(); // validate exponent
                        sb.Append(PeekAndTake());

                        // check for signed exponent
                        if (Matches('-', '+'))
                            sb.Append(PeekAndTake());

                        if (!Peek.IsDigit())
                        {
                            ResetIndex(); // reset validate exponent
                            parsing = false;
                            break;
                        }
                        while (Peek.IsDigit())
                        {
                            sb.Append(PeekAndTake());
                        }
                        if (!double.TryParse(sb.ToString(), out _))
                        {
                            ResetIndex(); // reset validate exponent
                            parsing = false;
                            break;
                        }
                        PopIndex(); // end validate exponent

                        lastValidToken = Token.LiteralValue(sb.ToString(), TokenLiteralType.Double, this);
                        parsing = false;
                        break;
                }
            }

            if (lastValidToken == null)
                return false;

            tokens.Add(lastValidToken);
            return true;
        }

        private bool PullBool(HoconTokenizerResult tokens)
        {
            if (Matches("true"))
            {
                Take(4);
                tokens.Add(Token.LiteralValue("true", TokenLiteralType.Bool, this));
                return true;
            }
            if (Matches("yes"))
            {
                Take(3);
                tokens.Add(Token.LiteralValue("yes", TokenLiteralType.Bool, this));
                return true;
            }
            if (Matches("on"))
            {
                Take(2);
                tokens.Add(Token.LiteralValue("on", TokenLiteralType.Bool, this));
                return true;
            }

            if (Matches("false"))
            {
                Take(5);
                tokens.Add(Token.LiteralValue("false", TokenLiteralType.Bool, this));
                return true;
            }
            if (Matches("no"))
            {
                Take(2);
                tokens.Add(Token.LiteralValue("no", TokenLiteralType.Bool, this));
                return true;
            }
            if (Matches("off"))
            {
                Take(3);
                tokens.Add(Token.LiteralValue("off", TokenLiteralType.Bool, this));
                return true;
            }
            return false;
        }

        private bool PullUnquotedText(HoconTokenizerResult tokens)
        {
            if (!IsUnquotedText())
                return false;

            var sb = new StringBuilder();
            while (!EoF && IsUnquotedText())
            {
                sb.Append(PeekAndTake());
            }

            tokens.Add(Token.LiteralValue(sb.ToString(), TokenLiteralType.UnquotedLiteralValue, this));
            return true;
        }

        /// <summary>
        /// Retrieves a quoted <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.</returns>
        private bool PullQuotedText(HoconTokenizerResult tokens)
        {
            if (!Matches('\"', '\''))
                return false;

            var sb = new StringBuilder();
            Take();
            while (!EoF && !Matches('\"', '\''))
            {
                if (Matches("\\"))
                {
                    sb.Append(PullEscapeSequence());
                }
                else
                {
                    sb.Append(PeekAndTake());
                }
            }

            if (EoF)
                throw new HoconTokenizerException($"Expected end of quoted string, found {TokenType.EndOfFile} instead.", Token.Error(this));
            Take();

            tokens.Add(Token.QuotedLiteralValue(sb.ToString(), this));
            return true;
        }

        /// <summary>
        /// Retrieves a triple quoted <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.</returns>
        private bool PullTripleQuotedText(HoconTokenizerResult tokens)
        {
            if (!Matches("\"\"\"", "'''"))
                return false;

            var sb = new StringBuilder();
            Take(3);
            while (!EoF && !Matches("\"\"\"", "'''"))
            {
                if (Matches("\\"))
                {
                    sb.Append(PullEscapeSequence());
                }
                else
                {
                    sb.Append(PeekAndTake());
                }
            }

            if (EoF)
                throw new HoconTokenizerException($"Expected end of triple quoted string, found {TokenType.EndOfFile} instead.", Token.Error(this));
            Take(3);

            tokens.Add(Token.TripleQuotedLiteralValue(sb.ToString(), this));
            return true;
        }

        /// <summary>
        /// Retrieves the current line from where the current token
        /// is located in the string.
        /// </summary>
        /// <returns>The current line from where the current token is located.</returns>
        private string DiscardRestOfLine()
        {
            var sb = new StringBuilder();
            while (!EoF && !Matches(Utils.NewLine))
            {
                sb.Append(PeekAndTake());
            }

            return sb.ToString();
        }

        private bool IsStartOfComment() => Matches("//");

        private bool IsUnquotedText() => !EoF && !Peek.IsHoconWhitespace() && !IsStartOfComment() && !Peek.IsNotInUnquotedText();
    }
}