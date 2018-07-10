using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hocon
{
    public class HoconPath:List<string>
    {
        public bool IsEmpty => Count == 0;
        public string Value => string.Join(".", this);
        public string Last => this[Count - 1];

        public HoconPath() { }

        public HoconPath(IEnumerable<string> path)
        {
            AddRange(path);
        }

        public override string ToString()
            => Value;

        internal static HoconPath FromTokens(HoconTokenizerResult tokens)
        {
            if(tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            var result = new List<string>();
            var sb = new StringBuilder();
            while (tokens.Current.Type == TokenType.LiteralValue)
            {
                switch (tokens.Current.LiteralType)
                {
                    case TokenLiteralType.TripleQuotedLiteralValue:
                        throw HoconParserException.Create(tokens.Current, null, "Triple quoted string could not be used in path expression.");

                    case TokenLiteralType.QuotedLiteralValue:
                        // Normalize quoted keys, remove the quotes if the key doesn't need them.
                        //sb.Append(tokens.Current.Value.NeedQuotes() ? $"\"{tokens.Current.Value}\"" : tokens.Current.Value);
                        sb.Append(tokens.Current.Value);
                        break;

                    default:
                        var split = tokens.Current.Value.Split('.');
                        for(var i = 0; i < split.Length-1; ++i)
                        {
                            sb.Append(split[i]);
                            result.Add(sb.ToString());
                            sb.Clear();
                        }
                        sb.Append(split[split.Length-1]);
                        break;
                }
                tokens.Next();
            }
            result.Add(sb.ToString());
            return new HoconPath(result);
        }

        public static HoconPath Parse(string path)
        {
            if(path == null)
                throw new ArgumentNullException(nameof(path));

            try
            {
                return FromTokens(new HoconTokenizer(path).Tokenize());
            }
            catch (HoconTokenizerException e)
            {
                throw HoconParserException.Create(e, null, "Failed to tokenize path", e);
            }
            
        }
    }
}
