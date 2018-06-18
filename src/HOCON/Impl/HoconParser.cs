//-----------------------------------------------------------------------
// <copyright file="HoconParser.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/Hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hocon
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
            return $"Current path: {currentPath}";
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
                if (res == HoconValue.Undefined)
                {
                    if (sub.IsQuestionMark)
                    {
                        string envValue = null;
                        try
                        {
                            // Try to pull value from environment
                            envValue = Environment.GetEnvironmentVariable(sub.Path);
                        }
                        catch {}

                        if (envValue == null)
                        {
                            sub.Owner.Values.Remove(sub);
                            continue;
                        }
                        else
                        {
                            res = new HoconValue();
                            res.AppendValue(new HoconLiteral
                            {
                                Value = envValue
                            });
                        }
                    }
                    else
                        throw new HoconParserException($"Unresolved substitution:{sub.Path}");
                }

                sub.ResolvedValue = res;

                //var owner = sub.Owner.GetObject();
                if (res.IsArray() || res.IsString())
                {
                    if (sub.Owner.IsObject())
                        throw new HoconParserException("Array substitution can not be value concatenated with an object.");

                    ValidateValue(sub.Owner, false);
                    continue;
                }

                foreach (var value in sub.Owner.Values)
                {
                    if(value.IsArray())
                        throw new HoconParserException("Hocon object can not be merged with an array.");

                    if (value is HoconLiteral && !value.IsWhitespace())
                        throw new HoconParserException("Hocon object can not be merged with a string.");
                }

                /*
                var otherObject = res.GetObject();
                if (owner != null && owner != otherObject)
                {
                    sub.Owner.Values.Remove(sub);
                    owner.Merge(otherObject);
                }
                */
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

                    owner.AppendValue(new HoconObject());
                    //the value of this KVP is not an object, thus, we should add a new
                    //owner.NewValue(new HoconObject());
                }

                HoconObject currentObject = owner.GetObject();

                while (!_reader.EoF)
                {
                    Token t = _reader.PullNext();
                    switch (t.Type)
                    {
                        case TokenType.Include:
                            var included = _includeCallback(t.Value);
                            foreach (var substitution in included.Substitutions)
                            {
                                //fixup the substitution, add the current path as a prefix to the substitution path
                                substitution.Path = currentPath + "." + substitution.Path;
                            }
                            _substitutions.AddRange(included.Substitutions);
                            var otherObj = included.Value.GetObject();
                            owner.GetObject().Merge(otherObj);

                            break;
                        case TokenType.EoF:
                            if (!string.IsNullOrEmpty(currentPath))
                            {
                                throw new HoconParserException($"Expected end of object but found EoF {GetDiagnosticsStackTrace()}");
                            }
                            break;
                        case TokenType.Key:
                            HoconValue value = currentObject.GetOrCreateKey(t.Value);
                            var nextPath = currentPath == "" ? t.Value : currentPath + "." + t.Value;
                            ParseKeyContent(value, nextPath);
                            ValidateValue(value, true);
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

        private enum ItemType
        {
            None,
            String,
            Array,
            Object
        }

        private void ValidateValue(HoconValue hoconValue, bool ignoreSubstitution)
        {
            var type = ItemType.None;
            foreach (var value in hoconValue.Values)
            {
                if(value is HoconSubstitution && ignoreSubstitution)
                    continue;

                if (value.IsArray())
                {
                    switch (type)
                    {
                        case ItemType.None:
                        case ItemType.Array:
                            type = ItemType.Array;
                            continue;
                    }
                    throw new HoconParserException("Arrays can only be value concatenated with another array.");
                }
                if (value.IsString())
                {
                    if (value is HoconLiteral literal && literal.IsWhitespace())
                        continue;

                    switch (type)
                    {
                        case ItemType.None:
                        case ItemType.String:
                            type = ItemType.String;
                            continue;
                    }
                    throw new HoconParserException("String can only be value concatenated with another string.");
                }
            }
        }

        private void ParseKeyContent(HoconValue value,string currentPath)
        {
            try
            {
                var last = currentPath.Split('.').Last();
                PushDiagnostics($"{last} = ");
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
        public void ParseValue(HoconValue owner, string currentPath)
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
                            if (owner.IsArray())
                                throw new HoconParserException("Literal value can not be value concatenated with an array");

                            if (owner.IsObject())
                            {
                                var ownerObject = owner.GetObject();
                                if(!ownerObject.IsOutOfScope)
                                    throw new HoconParserException("Literal value can not be value concatenated with an object");

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
                            if (owner.IsString() || owner.IsArray())
                                throw new HoconParserException("Object can not be merged with an array or string");

                            ParseObject(owner, true, currentPath);
                            break;
                        case TokenType.ArrayStart:
                            if (owner.IsObject() || owner.IsString())
                                throw new HoconParserException("Array can not be value concatenated with a string or object");

                            HoconArray arr = ParseArray(currentPath);
                            owner.AppendValue(arr);
                            break;
                        case TokenType.Substitute:
                            HoconSubstitution sub = ParseSubstitution(owner, t.Value);
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
                throw new HoconParserException($"{tokenizerException.Message}\n{GetDiagnosticsStackTrace()}",tokenizerException);
            }
            finally
            {
                //no value was found, tokenizer is still at the same position
                if (_reader.Index == start)
                {
                    throw new HoconParserException($"Hocon syntax error {_reader.GetHelpTextAtIndex(start)}\n{GetDiagnosticsStackTrace()}");
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

        private static HoconSubstitution ParseSubstitution(HoconValue owner, string value)
        {
            return new HoconSubstitution(owner, value);
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

