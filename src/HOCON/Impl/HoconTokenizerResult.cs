using System.Collections.Generic;
using System.Linq;

namespace Hocon
{
    internal sealed class HoconTokenizerResult : List<Token>
    {
        private readonly Stack<int> _indexStack = new Stack<int>();

        public int Index { get; private set; }

        public Token Current => this[Index];

        public void Push() => _indexStack.Push(Index);

        public void Pop() => Index = _indexStack.Pop();

        public void Reset() => Index = 0;

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

        public bool GetNextSignificant(TokenType tokenType)
        {
            while(Next().LiteralType == TokenLiteralType.Whitespace) { }
            return Current.Type == tokenType;
        }

        public bool GetNextSignificant(params TokenType[] tokenTypes)
        {
            while (Next().LiteralType == TokenLiteralType.Whitespace) { }
            return tokenTypes.Any(tokenType => Current.Type == tokenType);
        }

        public bool BackMatch(TokenType token)
        {
            Push();
            Back();
            while (Current.LiteralType == TokenLiteralType.Whitespace)
            {
                Back();
            }
            var current = Current;
            Pop();
            return current.Type == token;
        }

        public bool BackMatch(params TokenType[] tokens)
        {
            Push();
            Back();
            while (Current.LiteralType == TokenLiteralType.Whitespace)
            {
                Back();
            }
            var c = Current;
            if (tokens.Any(token => token == c.Type)) {
                Pop();
                return true;
            }
            Pop();
            return false;
        }

        public bool ForwardMatch(TokenType token)
        {
            Push();
            Next();
            while (Current.LiteralType == TokenLiteralType.Whitespace)
            {
                Next();
            }
            var current = Current;
            Pop();
            return current.Type == token;
        }

        public bool ForwardMatch(params TokenType[] tokens)
        {
            Push();
            while (Next() != null)
            {
                var c = Current;
                if (tokens.All(token => c.Type != token)) continue;
                Pop();
                return true;
            }
            Pop();
            return false;
        }
    }

}
