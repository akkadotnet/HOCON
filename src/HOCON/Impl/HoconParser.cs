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

        public int LineNumber => _reader.LineNumber;
        public int LinePosition => _reader.LinePosition;
        public string Path { get; private set; }
        public HoconTokenizer Tokenizer => _reader;

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
            // Workaround for annoying end of line difference between windows and unix
            if (Environment.NewLine == "\r\n")
            {
                text = text.Replace(Environment.NewLine, "\n");
            }

            _includeCallback = includeCallback;
            _root = new HoconValue(null);
            _reader = new HoconTokenizer(text);
            _reader.PullWhitespaceAndComments();
            ParseObject(_root, true,"");

            if (_reader.BraceCount > 0)
            {
                throw HoconParserException.Create(this, "Curly brace mismatch, missing closing brace.");
            }
            if (_reader.BraceCount < 0)
            {
                throw HoconParserException.Create(this, "Curly brace mismatch, missing opening brace.");
            }

            var c = new Config(new HoconRoot(_root, Enumerable.Empty<HoconSubstitution>()));
            foreach (HoconSubstitution sub in _substitutions)
            {
                HoconValue ownerValue;
                HoconValue res = c.GetValue(sub.Path);
                if (ReferenceEquals(res, HoconValue.Undefined))
                {
                    string envValue = null;
                    try
                    {
                        // Try to pull value from environment
                        envValue = Environment.GetEnvironmentVariable(sub.Path);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (envValue == null)
                    {
                        if (!sub.IsQuestionMark)
                            throw HoconParserException.Create(this, $"Unresolved substitution:{sub.Path}");

                        ownerValue = sub.Owner as HoconValue;
                        ownerValue.Values.Remove(sub);
                        if (ownerValue.IsEmpty)
                        {
                            // If owner value is empty, but it has old values, restore the old values
                            // Else, delete the owner.
                            if (ownerValue.OldValues != null && ownerValue.OldValues.Count > 0)
                            {
                                ownerValue.RestoreOldValues();
                            }
                            else
                            {
                                var ownerParent = ownerValue.Owner;
                                switch (ownerParent)
                                {
                                    case HoconObject obj:
                                        foreach (var item in obj.Items)
                                        {
                                            if (!ReferenceEquals(item.Value, ownerValue)) continue;

                                            obj.Items.Remove(item.Key);
                                            break;
                                        }
                                        break;
                                    case HoconArray arr:
                                        arr.Remove(ownerValue);
                                        break;
                                }
                            }
                        }

                        continue;
                    }

                    res = new HoconValue(sub);
                    res.AppendValue(new HoconLiteral(sub)
                    {
                        Value = envValue
                    });
                }

                ownerValue = sub.Owner as HoconValue;

                sub.ResolvedValue = res;
                if (res.IsArray() || res.IsString())
                {
                    if (sub.Owner.IsObject())
                        throw HoconParserException.Create(this, "Array substitution can not be value concatenated with an object.");

                    ValidateValue(ownerValue, false);
                    continue;
                }

                foreach (var value in ownerValue.Values)
                {
                    if(value.IsArray())
                        throw HoconParserException.Create(this, "Hocon object can not be merged with an array.");

                    if (value is HoconLiteral && !value.IsWhitespace())
                        throw HoconParserException.Create(this, "Hocon object can not be merged with a string.");
                }
            }
            return new HoconRoot(_root, _substitutions);
        }

        private void ParseObject(HoconValue owner, bool root,string currentPath)
        {
            Path = currentPath;
            try
            {
                PushDiagnostics("{");

                if (owner.IsObject())
                {
                    //the value of this KVP is already an object
                }
                else
                {
                    _reader.IsParsingObject = true;
                    owner.AppendValue(new HoconObject(owner));
                    //the value of this KVP is not an object, thus, we should add a new
                    //owner.NewValue(new HoconObject());
                }

                HoconObject currentObject = owner.GetObject();

                while (!_reader.EoF)
                {
                    Token t;
                    try
                    {
                        t = _reader.PullNext();
                    }
                    catch (Exception e)
                    {
                        throw HoconParserException.Create(this, "Failed to parse Hocon key content.", e);
                    }

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
                                throw HoconParserException.Create(this, $"Expected end of object but found EoF {GetDiagnosticsStackTrace()}");
                            }
                            break;
                        case TokenType.Key:
                            HoconValue value = currentObject.GetOrCreateKey(t.Value);
                            var nextPath = currentPath == "" ? t.Value : currentPath + "." + t.Value;
                            Path = nextPath;
                            ParseKeyContent(value, nextPath);
                            Path = currentPath;
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
                    throw HoconParserException.Create(this, "Arrays can only be value concatenated with another array.");
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
                    throw HoconParserException.Create(this, "String can only be value concatenated with another string.");
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
                    Token t;
                    try
                    {
                        t = _reader.PullNext();
                    }
                    catch (Exception e)
                    {
                        throw HoconParserException.Create(this, "Failed to parse Hocon key content.", e);
                    }

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
                throw HoconParserException.Create(this, "End of file reached while trying to read a value");

            _reader.PullWhitespaceAndComments();
            var start = _reader.Index;
            try
            {
                while (_reader.IsValue())
                {
                    Token t;
                    try
                    {
                        t = _reader.PullValue();
                    }
                    catch (Exception e)
                    {
                        throw HoconParserException.Create(this, "Failed to parse Hocon token", e);
                    }

                    switch (t.Type)
                    {
                        case TokenType.EoF:
                            break;
                        case TokenType.LiteralValue:
                            if (owner.IsArray())
                                throw HoconParserException.Create(this, "Literal value can not be value concatenated with an array");

                            if (owner.IsObject())
                            {
                                if(_reader.IsParsingObject)
                                    throw HoconParserException.Create(this, "Literal value can not be value concatenated with an object");

                                //needed to allow for override objects
                                owner.Clear();
                            }

                            var lit = new HoconLiteral(owner)
                            {
                                Value = t.Value
                            };

                            owner.AppendValue(lit);

                            break;
                        case TokenType.ObjectStart:
                            if (owner.IsString() || owner.IsArray())
                                throw HoconParserException.Create(this, "Object can not be merged with an array or string");

                            ParseObject(owner, true, currentPath);
                            break;
                        case TokenType.ArrayStart:
                            if (owner.IsObject() || owner.IsString())
                                throw HoconParserException.Create(this, "Array can not be value concatenated with a string or object");

                            HoconArray arr = ParseArray(currentPath, owner);
                            owner.AppendValue(arr);
                            break;
                        case TokenType.SubstituteWithQuestionMark:
                        case TokenType.Substitute:
                            if (!_reader.IsValidSubstitutionPath(t.Value))
                                throw HoconParserException.Create(this, $"Invalid substitution path: {t.Value}");

                            HoconSubstitution sub = ParseSubstitution(owner, t.Value, t.Type == TokenType.SubstituteWithQuestionMark);
                            _substitutions.Add(sub);
                            owner.AppendValue(sub);
                            break;
                    }

                    if(_reader.IsWhitespaceWithNoNewLine())
                        ParseTrailingWhitespaceWithNoNewLine(owner);

                    /*
                    if (_reader.IsSpaceOrTab())
                    {
                        ParseTrailingWhitespace(owner);
                    }
                    */
                }

                IgnoreComma();
            }
            catch(HoconTokenizerException tokenizerException)
            {
                throw HoconParserException.Create(this, $"{tokenizerException.Message}\n{GetDiagnosticsStackTrace()}", tokenizerException);
            }
            finally
            {
                //no value was found, tokenizer is still at the same position
                if (_reader.Index == start)
                {
                    throw HoconParserException.Create(this, $"Hocon syntax error {_reader.GetHelpTextAtIndex(start)}\n{GetDiagnosticsStackTrace()}");
                }
            }
        }

        private void ParseTrailingWhitespaceWithNoNewLine(HoconValue owner)
        {
            Token ws = _reader.PullNonNewLineWhitespace();
            //single line ws should be included if string concat
            if (ws.Value.Length > 0)
            {
                var wsLit = new HoconLiteral(owner)
                {
                    Value = ws.Value,
                };
                owner.AppendValue(wsLit);
            }
        }

        /*
        private void ParseTrailingWhitespace(HoconValue owner)
        {
            Token ws = _reader.PullSpaceOrTab();
            //single line ws should be included if string concat
            if (ws.Value.Length > 0)
            {
                var wsLit = new HoconLiteral(owner)
                {
                    Value = ws.Value,
                };
                owner.AppendValue(wsLit);
            }
        }
        */

        private static HoconSubstitution ParseSubstitution(HoconValue owner, string value, bool isQuestionMarked)
        {
            return new HoconSubstitution(owner, value, isQuestionMarked);
        }

        /// <summary>
        /// Retrieves the next array token from the tokenizer.
        /// </summary>
        /// <returns>An array of elements retrieved from the token.</returns>
        public HoconArray ParseArray(string currentPath, IHoconElement owner)
        {
            try
            {
                PushDiagnostics("[");

                var arr = new HoconArray(owner);
                while (!_reader.EoF && !_reader.IsArrayEnd())
                {
                    var v = new HoconValue(arr);
                    ParseValue(v, currentPath);
                    arr.Add(v);
                    _reader.PullWhitespaceAndComments();
                }
                try
                {
                    _reader.PullArrayEnd();
                }
                catch (Exception e)
                {
                    throw HoconParserException.Create(this, "Failed to parse Hocon", e);
                }
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

