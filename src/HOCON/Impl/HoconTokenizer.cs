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

        private readonly ReadOnlyMemory<char> _text;
        private int _index;

        protected void PushIndex() => _indexStack.Push(Index);
        protected void ResetIndex() => Index = _indexStack.Pop();
        protected void PopIndex() => _indexStack.Pop();

        private bool _dirty = true;
        private char _peek;

        protected char Peek
        {
            get
            {
                if (!_dirty) return _peek;

                _dirty = false;
                _peek = EoF ? (char) 0 : _text.Span[Index];
                return _peek;
            }
        }

        public int Length => _text.Length;

        public int Index
        {
            get => _index;
            private set
            {
                _index = value;
                if (_index > _text.Length)
                    _index = _text.Length;
                _dirty = true;
            }
        }

        public int LineNumber { get; private set; } = 1;
        public int LinePosition { get; private set; } = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="text">The string that contains the text to tokenize.</param>
        protected Tokenizer(ReadOnlyMemory<char> text)
        {
            _text = text;
            Index = 0;
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
                if (pattern[i] != _text.Span[Index + i])
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
                    if (pattern[i] == _text.Span[Index + i]) continue;
                    match = false;
                    break;
                }
                if(match)
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
            if (_text.Span[Index] == Utils.NewLine)
            {
                LineNumber++;
                LinePosition = 1;
            }
        }

        protected ReadOnlyMemory<char> Slice(int length)
        {
            var ret = _text.Slice(_index, length);
            Index += length;
            return ret;
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

            var s = new string(_text.Span.Slice(Index, length).ToArray());
            Index += length;
            return s;
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
        public HoconTokenizer(ReadOnlyMemory<char> text)
            : base(text) { }

        public HoconTokenizer(string text)
            : base(text.AsMemory()) { }

        public HoconTokenizerResult Tokenize()
        {
            var tokens = Tokenize(TokenType.EndOfFile);
            tokens.Add(new Token(new ReadOnlyMemory<char>(), TokenType.EndOfFile, this));
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
                        tokens.Add(new Token(Slice(1), TokenType.StartOfObject, this));
                        tokens.AddRange(Tokenize(TokenType.EndOfObject));
                        continue;

                    case '}':
                        tokens.Add(new Token(Slice(1), TokenType.EndOfObject, this));
                        if (closingTokenType != tokens[tokens.Count - 1].Type)
                            throw new HoconTokenizerException(
                                $"Expected {closingTokenType}, found {tokens[tokens.Count - 1].Type} instead.",
                                tokens[tokens.Count - 1]);
                        return tokens;

                    case '[':
                        tokens.Add(new Token(Slice(1), TokenType.StartOfArray, this));
                        tokens.AddRange(Tokenize(TokenType.EndOfArray));
                        continue;

                    case ']':
                        tokens.Add(new Token(Slice(1), TokenType.EndOfArray, this));
                        if (closingTokenType != tokens[tokens.Count - 1].Type)
                            throw new HoconTokenizerException(
                                $"Expected {closingTokenType}, found {tokens[tokens.Count - 1].Type} instead.",
                                tokens[tokens.Count - 1]);
                        return tokens;

                    case ',':
                        tokens.Add(new Token(Slice(1), TokenType.Comma, TokenLiteralType.UnquotedLiteralValue, this));
                        continue;

                    case ':':
                    case '=':
                        tokens.Add(new Token(Slice(1), TokenType.Assign, this));
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
                        tokens.Add(new Token(Slice(1), TokenType.EndOfLine, this));
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

            tokens.Add(new Token(Slice(2), TokenType.PlusEqualAssign, this));
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

            includeTokens.Add(new Token(Slice("include".Length), TokenType.Include, this));
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

            tokens.Add(new Token(Slice(1), TokenType.ParenthesisStart, this));
            PullWhitespaces();
            return true;
        }

        private bool PullParenthesisEnd(HoconTokenizerResult tokens)
        {
            if (Peek != ')')
                return false;

            tokens.Add(new Token(Slice(1), TokenType.ParenthesisEnd, this));
            PullWhitespaces();
            return true;
        }

        private bool PullRequired(HoconTokenizerResult tokens)
        {
            if (!Matches("required"))
                return false;

            tokens.Add(new Token(Slice("required".Length), TokenType.Required, this));
            PullWhitespaces();
            return true;

        }

        private bool PullUrlInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("url"))
                return false;

            tokens.Add(new Token(Slice("url".Length), TokenType.Url, this));
            PullWhitespaces();
            return true;
        }

        private bool PullFileInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("file"))
                return false;

            tokens.Add(new Token(Slice("file".Length), TokenType.File, this));
            PullWhitespaces();
            return true;
        }

        private bool PullResourceInclude(HoconTokenizerResult tokens)
        {
            if (!Matches("classpath"))
                return false;

            tokens.Add(new Token(Slice("classpath".Length), TokenType.Classpath, this));
            PullWhitespaces();
            return true;
        }

        private string PullEscapeSequence()
        {
            Take(); //consume "\"
            char escaped = Peek;
            Take();
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

            PushIndex();
            while (!EoF && Peek != '}')
            {
                Take();
            }

            if (EoF)
            {
                ResetIndex();
                throw new HoconTokenizerException("Expected end of substitution but found EoF instead.", Token.Error(this));
            }

            var end = Index;
            ResetIndex();
            tokens.Add(Token.Substitution(Slice(end - Index), this, questionMarked));
            Take();

            return true;
        }

        private bool PullNonNewLineWhitespace(HoconTokenizerResult tokens)
        {
            if (!Peek.IsWhitespaceWithNoNewLine())
                return false;

            PushIndex();
            while (Peek.IsWhitespaceWithNoNewLine())
            {
                Take();
            }
            var end = Index;
            ResetIndex();
            tokens.Add(Token.LiteralValue(Slice(end - Index), TokenLiteralType.Whitespace, this));
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
                case 'n':
                    return PullNull(tokens) ||
                           PullUnquotedText(tokens);
                default:
                    return PullUnquotedText(tokens);
            }
        }

        private bool PullNull(HoconTokenizerResult tokens)
        {
            if (!Matches("null"))
                return false;

            tokens.Add(Token.LiteralValue(Slice("null".Length), TokenLiteralType.Null, this));
            return true;
        }

        private bool PullUnquotedText(HoconTokenizerResult tokens)
        {
            if (!IsUnquotedText())
                return false;

            PushIndex();
            while (!EoF && IsUnquotedText())
            {
                Take();
            }
            var end = Index;
            ResetIndex();
            tokens.Add(Token.LiteralValue(Slice(end - Index), TokenLiteralType.UnquotedLiteralValue, this));
            return true;
        }

        /// <summary>
        /// Retrieves a quoted <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.</returns>
        private bool PullQuotedText(HoconTokenizerResult tokens)
        {
            if (Peek !=  '\"' && Peek != '\'')
                return false;

            Take();
            PushIndex();
            while (!EoF && Peek != '\"' && Peek != '\'')
            {
                Take();
            }

            if (EoF)
            {
                ResetIndex();
                throw new HoconTokenizerException($"Expected end of quoted string, found {TokenType.EndOfFile} instead.", Token.Error(this));
            }
            var end = Index;
            ResetIndex();
            tokens.Add(Token.QuotedLiteralValue(Slice(end-Index), this));
            Take();
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

            Take(3);
            PushIndex();
            while (!EoF && !Matches("\"\"\"", "'''"))
            {
                Take();
            }

            if (EoF)
            {
                ResetIndex();
                throw new HoconTokenizerException($"Expected end of triple quoted string, found {TokenType.EndOfFile} instead.", Token.Error(this));
            }
            var end = Index;
            ResetIndex();
            tokens.Add(Token.TripleQuotedLiteralValue(Slice(end - Index), this));
            Take(3);
            return true;
        }

        /// <summary>
        /// Retrieves the current line from where the current token
        /// is located in the string.
        /// </summary>
        /// <returns>The current line from where the current token is located.</returns>
        private ReadOnlyMemory<char> DiscardRestOfLine()
        {
            PushIndex();
            while (!EoF && Peek != Utils.NewLine)
            {
                Take();
            }
            var end = Index;
            ResetIndex();
            return Slice(end - Index);
        }

        private bool IsStartOfComment() => Matches("//");

        private bool IsUnquotedText() => !EoF && !Peek.IsHoconWhitespace() && !IsStartOfComment() && !Peek.IsNotInUnquotedText();
    }
}