//-----------------------------------------------------------------------
// <copyright file="HoconParser.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/Hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Akka.Configuration.Hocon
{
    /// <summary>
    /// This class contains methods used to parse HOCON (Human-Optimized Config Object Notation)
    /// configuration strings.
    /// </summary>
    public class Parser
    {
        private readonly List<HoconSubstitution> _substitutions = new List<HoconSubstitution>();
        private HoconTokenizer _reader;
        private HoconValue _root;
        private Func<string, HoconRoot> _includeCallback;
        private Stack<string> _diagnosticsStack = new Stack<string>();

        private void PushDiagnostics(string message)
        {
            _diagnosticsStack.Push(message);
        }

        private void PopDiagnostics()
        {
            _diagnosticsStack.Pop();
        }

        public string GetDiagnosticsStackTrace()
        {
            var currentPath = string.Join("", _diagnosticsStack.Reverse());
            return string.Format("Current path: {0}", currentPath);
        }

        /// <summary>
        /// Parses the supplied HOCON configuration string into a root element.
        /// </summary>
        /// <param name="text">The string that contains a HOCON configuration string.</param>
        /// <param name="includeCallback">Callback used to resolve includes</param>
        /// <returns>The root element created from the supplied HOCON configuration string.</returns>
        /// <exception cref="System.Exception">
        /// This exception is thrown when an unresolved substitution is encountered.
        /// It also occurs when the end of the file has been reached while trying
        /// to read a value.
        /// </exception>
        public static HoconRoot Parse(string text,Func<string,HoconRoot> includeCallback)
        {
            return new Parser().ParseText(text,includeCallback);
        }

        private HoconRoot ParseText(string text,Func<string,HoconRoot> includeCallback)
        {
            _includeCallback = includeCallback;
            _root = new HoconValue();
            _reader = new HoconTokenizer(text);
            _reader.PullWhitespaceAndComments();
            ParseObject(_root, true,"");

            var c = new Config(new HoconRoot(_root, Enumerable.Empty<HoconSubstitution>()));
            foreach (HoconSubstitution sub in _substitutions)
            {
                HoconValue res = c.GetValue(sub.Path);
                if (res == null)
                    throw new HoconParserException("Unresolved substitution:" + sub.Path);

                sub.ResolvedValue = res;
            }
            return new HoconRoot(_root, _substitutions);
        }

        private void ParseObject(HoconValue owner, bool root,string currentPath)
        {
            try
            {
                PushDiagnostics("{");

                if (owner.IsObject())
                {
                    //the value of this KVP is already an object
                }
                else
                {
                    //the value of this KVP is not an object, thus, we should add a new
                    owner.NewValue(new HoconObject());
                }

                HoconObject currentObject = owner.GetObject();

                while (!_reader.EoF)
                {
                    Token t = _reader.PullNext();
                    switch (t.Type)
                    {
                        case TokenType.Include:
                            var included = _includeCallback(t.Value);
                            var substitutions = included.Substitutions;
                            foreach (var substitution in substitutions)
                            {
                                //fixup the substitution, add the current path as a prefix to the substitution path
                                substitution.Path = currentPath + "." + substitution.Path;
                            }
                            _substitutions.AddRange(substitutions);
                            var otherObj = included.Value.GetObject();
                            owner.GetObject().Merge(otherObj);

                            break;
                        case TokenType.EoF:
                            break;
                        case TokenType.Key:
                            HoconValue value = currentObject.GetOrCreateKey(t.Value);
                            var nextPath = currentPath == "" ? t.Value : currentPath + "." + t.Value;
                            ParseKeyContent(value, nextPath);
                            if (!root)
                                return;
                            break;

                        case TokenType.ObjectEnd:
                            return;
                    }
                }
            }
            finally
            {
                PopDiagnostics();
            }
        }

        private void ParseKeyContent(HoconValue value,string currentPath)
        {
            try
            {
                var last = currentPath.Split('.').Last();
                PushDiagnostics(string.Format("{0} = ", last));
                while (!_reader.EoF)
                {
                    Token t = _reader.PullNext();
                    switch (t.Type)
                    {
                        case TokenType.Dot:
                            ParseObject(value, false, currentPath);
                            return;
                        case TokenType.Assign:

                            if (!value.IsObject())
                            {
                                //if not an object, then replace the value.
                                //if object. value should be merged
                                value.Clear();
                            }
                            ParseValue(value, currentPath);
                            return;
                        case TokenType.ObjectStart:
                            ParseObject(value, true, currentPath);
                            return;
                    }
                }
            }
            finally
            {
                PopDiagnostics();
            }
        }

        /// <summary>
        /// Retrieves the next value token from the tokenizer and appends it
        /// to the supplied element <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The element to append the next token.</param>
        /// <exception cref="System.Exception">End of file reached while trying to read a value</exception>
        public void ParseValue(HoconValue owner,string currentPath)
        {
            if (_reader.EoF)
                throw new HoconParserException("End of file reached while trying to read a value");

            _reader.PullWhitespaceAndComments();
            var start = _reader.Index;
            try
            {
                while (_reader.IsValue())
                {
                    Token t = _reader.PullValue();

                    switch (t.Type)
                    {
                        case TokenType.EoF:
                            break;
                        case TokenType.LiteralValue:
                            if (owner.IsObject())
                            {
                                //needed to allow for override objects
                                owner.Clear();
                            }
                            var lit = new HoconLiteral
                            {
                                Value = t.Value
                            };
                            owner.AppendValue(lit);

                            break;
                        case TokenType.ObjectStart:
                            ParseObject(owner, true, currentPath);
                            break;
                        case TokenType.ArrayStart:
                            HoconArray arr = ParseArray(currentPath);
                            owner.AppendValue(arr);
                            break;
                        case TokenType.Substitute:
                            HoconSubstitution sub = ParseSubstitution(t.Value);
                            _substitutions.Add(sub);
                            owner.AppendValue(sub);
                            break;
                    }
                    if (_reader.IsSpaceOrTab())
                    {
                        ParseTrailingWhitespace(owner);
                    }
                }

                IgnoreComma();
            }
            catch(HoconTokenizerException tokenizerException)
            {
                throw new HoconParserException(string.Format("{0}\r{1}", tokenizerException.Message, GetDiagnosticsStackTrace()),tokenizerException);
            }
            finally
            {
                //no value was found, tokenizer is still at the same position
                if (_reader.Index == start)
                {
                    throw new HoconParserException(string.Format("Hocon syntax error {0}\r{1}",_reader.GetHelpTextAtIndex(start),GetDiagnosticsStackTrace()));
                }
            }
        }

        private void ParseTrailingWhitespace(HoconValue owner)
        {
            Token ws = _reader.PullSpaceOrTab();
            //single line ws should be included if string concat
            if (ws.Value.Length > 0)
            {
                var wsLit = new HoconLiteral
                {
                    Value = ws.Value,
                };
                owner.AppendValue(wsLit);
            }
        }

        private static HoconSubstitution ParseSubstitution(string value)
        {
            return new HoconSubstitution(value);
        }

        /// <summary>
        /// Retrieves the next array token from the tokenizer.
        /// </summary>
        /// <returns>An array of elements retrieved from the token.</returns>
        public HoconArray ParseArray(string currentPath)
        {
            try
            {
                PushDiagnostics("[");

                var arr = new HoconArray();
                while (!_reader.EoF && !_reader.IsArrayEnd())
                {
                    var v = new HoconValue();
                    ParseValue(v, currentPath);
                    arr.Add(v);
                    _reader.PullWhitespaceAndComments();
                }
                _reader.PullArrayEnd();
                return arr;
            }
            finally
            {
                PopDiagnostics();
            }
        }

        private void IgnoreComma()
        {
            if (_reader.IsComma()) //optional end of value
            {
                _reader.PullComma();
            }
        }
    }
}

