// -----------------------------------------------------------------------
// <copyright file="HoconTokenizerResult.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    internal sealed class HoconTokenizerResult : List<Token>
    {
        private readonly Stack<int> _indexStack = new Stack<int>();

        public int Index { get; private set; }

        public Token Current => this[Index];

        public void PushPosition()
        {
            _indexStack.Push(Index);
        }

        public void PopPosition()
        {
            Index = _indexStack.Pop();
        }

        public void ResetPosition()
        {
            Index = 0;
        }

        public void Insert(Token token)
        {
            Insert(Index, token);
        }

        public void InsertAfter(Token token)
        {
            Insert(Index + 1, token);
        }

        public Token Back()
        {
            Index--;
            if (Index < 0)
                Index = 0;
            return this[Index];
        }

        public Token Next()
        {
            Index++;
            if (Index >= Count)
                Index = Count - 1;
            return this[Index];
        }

        // It is assumed that comments are ignored completely
        public void ToNextSignificant()
        {
            Next();
            while (Current.LiteralType == TokenLiteralType.Whitespace || Current.Type == TokenType.Comment)
                Next();
        }

        public void ToNextSignificantLine()
        {
            Next();
            while (Current.LiteralType == TokenLiteralType.Whitespace
                   || Current.Type == TokenType.Comment
                   || Current.Type == TokenType.EndOfLine)
                Next();
        }

        public bool BackMatch(TokenType token)
        {
            PushPosition();
            Back();
            while (Current.LiteralType == TokenLiteralType.Whitespace) Back();
            var current = Current;
            PopPosition();
            return current.Type == token;
        }

        public bool BackMatch(params TokenType[] tokens)
        {
            PushPosition();
            Back();
            while (Current.LiteralType == TokenLiteralType.Whitespace) Back();
            var c = Current;
            if (tokens.Any(token => token == c.Type))
            {
                PopPosition();
                return true;
            }

            PopPosition();
            return false;
        }

        public bool ForwardMatch(TokenType token)
        {
            PushPosition();
            Next();
            while (Current.LiteralType == TokenLiteralType.Whitespace) Next();
            var current = Current;
            PopPosition();
            return current.Type == token;
        }

        public bool ForwardMatch(params TokenType[] tokens)
        {
            PushPosition();
            while (Next() != null)
            {
                var c = Current;
                if (tokens.All(token => c.Type != token)) continue;
                PopPosition();
                return true;
            }

            PopPosition();
            return false;
        }
    }
}