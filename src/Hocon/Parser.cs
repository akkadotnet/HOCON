//-----------------------------------------------------------------------
// <copyright file="Parser.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hocon
{
    public delegate Task<string> HoconIncludeCallbackAsync(HoconCallbackType callbackType, string value);

    /// <summary>
    /// This class contains methods used to parse HOCON (Human-Optimized Config Object Notation)
    /// configuration strings.
    /// </summary>
    public sealed class Parser
    {
        private readonly List<HoconSubstitution> _substitutions = new List<HoconSubstitution>();
        private HoconIncludeCallbackAsync _includeCallback = (type, value) => Task.FromResult("{}");

        private HoconTokenizerResult _tokens;
        private HoconValue _root;

        private HoconPath Path { get; } = new HoconPath();

        /// <summary>
        /// Parses the supplied HOCON configuration string into a root element.
        /// </summary>
        /// <param name="text">The string that contains a HOCON configuration string.</param>
        /// <param name="includeCallback">Callback used to resolve includes</param>
        /// <returns>The root element created from the supplied HOCON configuration string.</returns>
        /// <exception cref="HoconParserException">
        /// This exception is thrown when an unresolved substitution is encountered.
        /// It also occurs when any error is encountered while tokenizing or parsing the configuration string.
        /// </exception>
        public static HoconRoot Parse(string text, HoconIncludeCallbackAsync includeCallback = null)
        {
            return new Parser().ParseText(text, true, includeCallback);
        }

        private HoconRoot ParseText(string text, bool resolveSubstitutions, HoconIncludeCallbackAsync includeCallback)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new HoconParserException(
                    $"Parameter {nameof(text)} is null or empty.\n" +
                    "If you want to create an empty Hocon HoconRoot, use \"{}\" or just use \"new HoconRoot();\" instead.");

            if (includeCallback != null)
                _includeCallback = includeCallback;

            try
            {
                _tokens = new HoconTokenizer(text).Tokenize();
                _root = new HoconValue(null);
                ParseTokens();
                if(resolveSubstitutions)
                    ResolveSubstitutions();
            }
            catch (HoconTokenizerException e)
            {
                throw HoconParserException.Create(e, null, $"Error while tokenizing Hocon: {e.Message}", e);
            }
            catch (HoconException e)
            {
                throw HoconParserException.Create(_tokens.Current, Path, e.Message, e);
            }

            return new HoconRoot(_root, _substitutions);
        }

        private void ResolveSubstitutions()
        {
            var removedSubstitution = new List<HoconSubstitution>();
            foreach (var sub in _substitutions)
            {
                if (sub.Removed)
                {
                    removedSubstitution.Add(sub);
                    continue;
                }

                // Retrieve value
                HoconValue res;
                try
                {
                    res = ResolveSubstitution(sub);
                }
                catch(HoconException e)
                {
                    throw HoconParserException.Create(sub, sub.Path, $"Invalid substitution declaration. {e.Message}.", e);
                }

                if (res != null)
                {
                    sub.ResolvedValue = res;
                    continue;
                }

                // Try to pull value from environment
                string envValue = null;
                try
                {
                    envValue = Environment.GetEnvironmentVariable(sub.Path.Value);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (envValue != null)
                {
                    // undefined value resolved to an environment variable
                    res = new HoconValue(sub.Parent.Parent);
                    if (envValue.NeedQuotes())
                        res.Add(new HoconQuotedString(sub.Parent.Parent, envValue));
                    else
                        res.Add(new HoconUnquotedString(sub.Parent.Parent, envValue));

                    sub.ResolvedValue = res;
                    continue;
                }

                // ${ throws exception if it is not resolved
                if (sub.Required)
                    throw HoconParserException.Create(sub, sub.Path, $"Unresolved substitution: {sub.Path}");

                sub.ResolvedValue = new HoconEmptyValue(sub.Parent.Parent);
            }

            foreach (var sub in removedSubstitution)
            {
                _substitutions.Remove(sub);
            }
        }

        private HoconValue ResolveSubstitution(HoconSubstitution sub)
        {
            var subField = sub.ParentField;

            // first case, this substitution is a direct self-reference
            if (sub.Path == subField.Path)
            {
                var parent = sub.Parent;
                while (parent is HoconValue)
                    parent = parent.Parent;

                // Fail case
                if (parent is HoconArray)
                    throw new HoconException("Self-referencing substitution may not be declared within an array.");

                // try to resolve substitution by looking backward in the field assignment stack
                return subField.OlderValueThan(sub);
            }

            // second case, the substitution references a field child in the past
            if (sub.Path.IsChildPathOf(subField.Path))
            {
                var olderValue = subField.OlderValueThan(sub);
                if (olderValue.Type == HoconType.Object)
                {
                    var difLength = sub.Path.Count - subField.Path.Count;
                    var deltaPath = sub.Path.SubPath(sub.Path.Count - difLength, difLength);

                    var olderObject = olderValue.GetObject();
                    if (olderObject.TryGetValue(deltaPath, out var innerValue))
                    {
                        return innerValue.Type == HoconType.Object ? innerValue : null;
                    }
                }
            }

            // Detect invalid parent-referencing substitution
            if (subField.Path.IsChildPathOf(sub.Path))
                throw new HoconException("Substitution may not reference one of its direct parents.");

            // Detect invalid cyclic reference loop
            if (IsValueCyclic(subField, sub))
                throw new HoconException("A cyclic substitution loop is detected in the Hocon file.");

            // third case, regular substitution
            _root.GetObject().TryGetValue(sub.Path, out var field);
            return field?.Clone(field.Parent) as HoconValue;
        }

        private bool IsValueCyclic(HoconField field, HoconSubstitution sub)
        {
            var pendingValues = new Stack<HoconValue>();
            var visitedFields = new List<HoconField> { field };
            var pendingSubs = new Stack<HoconSubstitution>();
            pendingSubs.Push(sub);

            while (pendingSubs.Count > 0)
            {
                var currentSub = pendingSubs.Pop();
                if (!_root.GetObject().TryGetField(currentSub.Path, out var currentField))
                    continue;

                if (visitedFields.Contains(currentField))
                    return true;

                visitedFields.Add(currentField);
                pendingValues.Push(currentField.Value);
                while (pendingValues.Count > 0)
                {
                    var currentValue = pendingValues.Pop();

                    foreach (var value in currentValue)
                    {
                        switch (value)
                        {
                            case HoconLiteral _:
                                break;

                            case HoconObject o:
                                foreach (var f in o.Values)
                                {
                                    if (visitedFields.Contains(f))
                                        return true;

                                    visitedFields.Add(f);
                                    pendingValues.Push(f.Value);
                                }
                                break;

                            case HoconArray a:
                                foreach (var item in a.GetArray())
                                {
                                    pendingValues.Push(item);
                                }
                                break;

                            case HoconSubstitution s:
                                pendingSubs.Push(s);
                                break;
                        }
                    }
                }
            }
            return false;
        }

        private void ParseTokens()
        {
            if (_tokens.Current.IsNonSignificant() || _tokens.Current.Type == TokenType.EndOfLine)
                _tokens.ToNextSignificantLine();

            // Array Hocon files are handled differently than normal Hocon file.
            // Array Hocon files HAVE to start with an angle bracket and can only contain ONE array
            if (_tokens.Current.Type == TokenType.StartOfArray)
            {
                _root.Add(ParseArray(_root));

                // Sanity check
                if (_tokens.Current.IsNonSignificant() || _tokens.Current.Type == TokenType.EndOfLine)
                    _tokens.ToNextSignificantLine();

                if (_tokens.Current.Type != TokenType.EndOfFile)
                    throw HoconParserException.Create(_tokens.Current, null, 
                        $"Illegal token type: {_tokens.Current.Type}. Hocon array file can only contain one array.", null);

                return;
            }

            if (_tokens.Current.Type != TokenType.StartOfObject)
            {
                // This is a "headless" Hocon file, we'll normalize the file
                // by inserting the proper open and close curly brackets
                _tokens.Insert(new Token("{", TokenType.StartOfObject, null));
                _tokens.Insert(_tokens.Count-1, new Token("}", TokenType.EndOfObject, null));
            }

            ParseObject(ref _root);
        }

        private HoconValue ParseInclude()
        {
            // Sanity check
            if (_tokens.Current.Type != TokenType.Include)
                throw HoconParserException.Create(_tokens.Current, Path,
                    $"Internal parser error, ParseInclude() is called on an invalid token: `{_tokens.Current.Type}`");

            var parenthesisCount = 0;
            var required = false;
            var callbackType = HoconCallbackType.File;
            string fileName = null;
            var includeToken = _tokens.Current;

            var expectedTokens = new List<TokenType>(new[]
            {
                TokenType.Required,
                TokenType.Url,
                TokenType.File,
                TokenType.Classpath,
                TokenType.LiteralValue,
                TokenType.ParenthesisEnd,
                TokenType.EndOfLine
            });

            var parsing = true;
            while (parsing)
            {
                _tokens.ToNextSignificant();
                var currentType = _tokens.Current.Type;
                if (expectedTokens.All(t => t != currentType))
                    throw HoconParserException.Create(_tokens.Current, Path,
                        $"Invalid token in include: `{currentType}`", null);

                switch (currentType)
                {
                    case TokenType.ParenthesisEnd:
                        if (parenthesisCount <= 0)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                "Unexpected closing parenthesis.", null);

                        parenthesisCount--;
                        parsing = parenthesisCount > 0;
                        break;

                    case TokenType.Required:
                        _tokens.ToNextSignificant();
                        // The next token after the "required" keyword have to be an open paren
                        if(_tokens.Current.Type != TokenType.ParenthesisStart)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Expected {TokenType.ParenthesisStart}, found `{_tokens.Current.Type}` instead.");

                        parenthesisCount++;
                        required = true;
                        expectedTokens.Remove(TokenType.Required);
                        break;

                    case TokenType.Url:
                        _tokens.ToNextSignificant();
                        // The next token after the "url" keyword have to be an open paren
                        if (_tokens.Current.Type != TokenType.ParenthesisStart)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Expected {TokenType.ParenthesisStart}, found `{_tokens.Current.Type}` instead.");

                        parenthesisCount++;
                        callbackType = HoconCallbackType.Url;
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.Classpath);
                        break;

                    case TokenType.File:
                        _tokens.ToNextSignificant();
                        // The next token after the "file" keyword have to be an open paren
                        if (_tokens.Current.Type != TokenType.ParenthesisStart)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Expected {TokenType.ParenthesisStart}, found `{_tokens.Current.Type}` instead.");

                        parenthesisCount++;
                        callbackType = HoconCallbackType.File;
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.Classpath);
                        break;

                    case TokenType.Classpath:
                        _tokens.ToNextSignificant();
                        // The next token after the "classpath" keyword have to be an open paren
                        if (_tokens.Current.Type != TokenType.ParenthesisStart)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Expected {TokenType.ParenthesisStart}, found `{_tokens.Current.Type}` instead.");

                        parenthesisCount++;
                        callbackType = HoconCallbackType.Resource;
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.Classpath);
                        break;

                    case TokenType.LiteralValue:
                        if(_tokens.Current.IsNonSignificant())
                            _tokens.ToNextSignificant();

                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if (_tokens.Current.LiteralType != TokenLiteralType.QuotedLiteralValue)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Invalid literal type for declaring file name. Expected {TokenLiteralType.QuotedLiteralValue}, " +
                                $"found `{_tokens.Current.LiteralType}` instead.");

                        fileName = _tokens.Current.Value;
                        expectedTokens.Remove(TokenType.LiteralValue);
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.Classpath);

                        parsing = parenthesisCount > 0;
                        break;
                    default:
                        throw HoconParserException.Create(_tokens.Current, Path,
                            $"Unexpected token `{_tokens.Current.Type}`.");
                }
            }

            if (parenthesisCount > 0)
                throw HoconParserException.Create(_tokens.Current, Path,
                    $"Expected {TokenType.ParenthesisEnd}, found `{_tokens.Current.Type}`");

            if (fileName == null)
                throw HoconParserException.Create(_tokens.Current, Path,
                    "Include does not contain any quoted file name value.");

            // Consume the last token
            _tokens.ToNextSignificant();

            var includeHocon = _includeCallback(callbackType, fileName).ConfigureAwait(false).GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(includeHocon))
            {
                if(required)
                    throw HoconParserException.Create(includeToken, Path,
                        "Invalid Hocon include. Include was declared as required but include callback returned a null or empty string.");
                return new HoconEmptyValue(null);
            }

            var includeRoot = new Parser().ParseText(includeHocon, false, _includeCallback);
            /*
            if (owner != null && owner.Type != HoconType.Empty && owner.Type != includeRoot.Value.Type)
                throw HoconParserException.Create(includeToken, Path,
                    "Invalid Hocon include. Hocon config substitution type must be the same as the field it's merged into. " +
                    $"Expected type: `{owner.Type}`, type returned by include callback: `{includeRoot.Value.Type}`");
            */

            // fixup the substitution, add the current path as a prefix to the substitution path
            foreach (var substitution in includeRoot.Substitutions)
            {
                substitution.Path.InsertRange(0, Path);
            }
            _substitutions.AddRange(includeRoot.Substitutions);

            // re-parent the value returned by the callback to the owner of the include declaration
            return includeRoot.Value;
        }

        // The owner in this context can be either an object or an array.
        private void ParseObject(ref HoconValue owner)
        {
            if (owner == null)
                throw HoconParserException.Create(_tokens.Current, Path,
                    "ParseObject called with null parent.");

            // Sanity check
            if (_tokens.Current.Type != TokenType.StartOfObject)
                throw HoconParserException.Create(_tokens.Current, Path,
                    $"Failed to parse Hocon object. Expected `{TokenType.StartOfObject}`, found `{_tokens.Current.Type}` instead.");

            // Consume curly bracket
            _tokens.ToNextSignificant();

            var hoconObject = owner.GetObject();
            if (hoconObject == null)
            {
                hoconObject = new HoconObject(owner);
                owner.Add(hoconObject);
            }

            var valueWasParsed = false;
            var parsing = true;
            while (parsing)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon object. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}`, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        owner.ReParent(ParseInclude());
                        valueWasParsed = true;
                        break;

                    case TokenType.LiteralValue:
                        if (_tokens.Current.IsNonSignificant())
                            _tokens.ToNextSignificant();

                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon object. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}`, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        ParseField(hoconObject);
                        valueWasParsed = true;
                        break;

                    case TokenType.Comment:
                    case TokenType.EndOfLine:
                        valueWasParsed = false;
                        _tokens.ToNextSignificantLine();
                        break;

                    case TokenType.Comma:
                        if (!valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon object. Expected `{TokenType.Assign}` or `{TokenType.StartOfObject}`, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        valueWasParsed = false;
                        _tokens.ToNextSignificant();
                        break;

                    case TokenType.EndOfObject:
                        valueWasParsed = false;
                        parsing = false;
                        break;

                    case TokenType.EndOfFile:
                        throw HoconParserException.Create(_tokens.Current, Path,
                            $"Failed to parse Hocon object. Expected {TokenType.EndOfObject} but found {_tokens.Current.Type} instead.");

                    default:
                        throw HoconParserException.Create(_tokens.Current, Path,
                            $"Failed to parse Hocon object. Unexpected token `{_tokens.Current.Type}`.");
                }
            }

            // Consume the closing curly bracket.
            _tokens.ToNextSignificant();
        }

        // parse path value
        private HoconPath ParseKey()
        {
            // sanity check
            if (_tokens.Current.IsNonSignificant() || _tokens.Current.Type != TokenType.LiteralValue)
                throw HoconParserException.Create(_tokens.Current, Path,
                    $"Internal parser error, ParseKey() is called on an invalid token. Should be `{TokenType.LiteralValue}`, found `{_tokens.Current.Type}` instead.");

            var keyTokens = new HoconTokenizerResult();
            while (_tokens.Current.Type == TokenType.LiteralValue)
            {
                keyTokens.Add(_tokens.Current);
                _tokens.Next();
            }

            // TODO: this is janky, fix this
            keyTokens.Reverse();
            while (keyTokens.Count > 0 && keyTokens[0].LiteralType == TokenLiteralType.Whitespace)
            {
                keyTokens.RemoveAt(0);
            }
            keyTokens.Reverse();

            keyTokens.Add(new Token("", TokenType.EndOfFile, null));

            return HoconPath.FromTokens(keyTokens);
        }

        private void ParseField(HoconObject owner)
        {
            // sanity check
            if(_tokens.Current.IsNonSignificant() || _tokens.Current.Type != TokenType.LiteralValue)
                throw HoconParserException.Create(_tokens.Current, Path,
                    $"Failed to parse Hocon field. Expected start of field {TokenType.LiteralValue}, " +
                    $"found {_tokens.Current.Type} instead.");

            var relativePath = ParseKey();

            if(_tokens.Current.Type == TokenType.EndOfLine)
                _tokens.ToNextSignificantLine();

            // sanity check
            if (_tokens.Current.Type != TokenType.Assign 
                && _tokens.Current.Type != TokenType.StartOfObject
                && _tokens.Current.Type != TokenType.PlusEqualAssign)
                throw HoconParserException.Create(_tokens.Current, Path,
                    $"Failed to parse Hocon field. Expected {TokenType.Assign}, {TokenType.StartOfObject} " +
                    $"or {TokenType.PlusEqualAssign}, found {_tokens.Current.Type} instead.");

            // sanity check
            if (relativePath == null || relativePath.Count == 0)
                throw HoconParserException.Create(_tokens.Current, Path,
                    "Failed to parse Hocon field. Null or empty path");

            Path.AddRange(relativePath);

            var currentField = owner.TraversePath(relativePath);
            currentField.SetValue(ParseValue(currentField));

            Path.RemoveRange(Path.Count - relativePath.Count, relativePath.Count);
        }

        private HoconValue GetHoconValueFromParentElement(IHoconElement parentElement, TokenType type)
        {
            if(parentElement is HoconArray arr)
                return new HoconValue(arr);

            if(!(parentElement is HoconField hf))
                throw HoconParserException.Create(_tokens.Current, Path,
                    "Invalid parent element for HoconValue. Valid objects are HoconArray and HoconField.");

            var fieldValue = hf.Value;
            if (fieldValue == null)
                return new HoconValue(hf);

            switch (type)
            {
                case TokenType.LiteralValue:
                    return new HoconValue(hf);
                case TokenType.StartOfObject when fieldValue.Type == HoconType.Object:
                    return fieldValue;
                case TokenType.StartOfArray when fieldValue.Type == HoconType.Array:
                    return fieldValue;
                case TokenType.SubstituteOptional:
                case TokenType.SubstituteRequired:
                case TokenType.Include:
                default:
                    return new HoconValue(hf);
            }
        }

        /// <summary>
        /// Retrieves the next value token from the tokenizer and appends it
        /// to the supplied element <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The element to append the next token.</param>
        /// <exception cref="System.Exception">End of file reached while trying to read a value</exception>
        private HoconValue ParseValue(IHoconElement owner)
        {
            // value is lazy initialized because we don't know what kind of value we're parsing
            HoconValue value = null;
            var parsing = true;
            while (parsing)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        var includeToken = _tokens.Current;
                        var includeValue = ParseInclude();
                        switch (includeValue.Type)
                        {
                            case HoconType.Empty:
                                value = new HoconEmptyValue(owner);
                                break;
                            case HoconType.Object:
                                value = GetHoconValueFromParentElement(owner, TokenType.StartOfObject);
                                value.ReParent(includeValue);
                                break;
                            case HoconType.Array:
                                value = GetHoconValueFromParentElement(owner, TokenType.StartOfArray);
                                value.ReParent(includeValue);
                                break;
                            default:
                                throw HoconParserException.Create(includeToken, Path,
                                    "Include could never contain a literal type.");
                        }
                        break;

                    case TokenType.LiteralValue:
                        // Consume leading whitespaces.
                        if (_tokens.Current.IsNonSignificant())
                            _tokens.ToNextSignificant();

                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if(value == null)
                            value = GetHoconValueFromParentElement(owner, _tokens.Current.Type);

                        while (_tokens.Current.Type == TokenType.LiteralValue)
                        {
                            value.Add(HoconLiteral.Create(value, _tokens.Current));
                            _tokens.Next();
                        }
                        break;

                    case TokenType.StartOfObject:
                        if (value == null)
                            value = GetHoconValueFromParentElement(owner, _tokens.Current.Type);

                        ParseObject(ref value);
                        break;

                    case TokenType.StartOfArray:
                        if (value == null)
                            value = GetHoconValueFromParentElement(owner, _tokens.Current.Type);
                        value.Add(ParseArray(value));
                        break;

                    case TokenType.SubstituteOptional:
                    case TokenType.SubstituteRequired:
                        if (value == null)
                            value = new HoconValue(owner);

                        var pointerPath = HoconPath.Parse(_tokens.Current.Value);
                        var sub = new HoconSubstitution(value, pointerPath, _tokens.Current,
                            _tokens.Current.Type == TokenType.SubstituteRequired);
                        _substitutions.Add(sub);
                        _tokens.Next();
                        value.Add(sub);
                        break;

                    case TokenType.PlusEqualAssign:
                        if (value == null)
                            value = new HoconValue(owner);

                        var subAssign = new HoconSubstitution(value, new HoconPath(Path), _tokens.Current, false);
                        _substitutions.Add(subAssign);
                        value.Add(subAssign);
                        value.Add(ParsePlusEqualAssignArray(value));
                        parsing = false;
                        break;

                    case TokenType.EndOfObject:
                    case TokenType.EndOfArray:
                        parsing = false;
                        break;

                    case TokenType.Comment:
                        _tokens.ToNextSignificant();
                        break;

                    case TokenType.EndOfLine:
                        parsing = false;
                        break;

                    case TokenType.EndOfFile:
                    case TokenType.Comma:
                        parsing = false;
                        break;

                    case TokenType.Assign:
                        // Special case to support end of line after assign
                        _tokens.ToNextSignificantLine();
                        break;

                    default:
                        throw HoconParserException.Create(_tokens.Current, Path,
                            $"Failed to parse Hocon value. Unexpected token: `{_tokens.Current.Type}`");
                }
            }

            if (value == null)
                value = new HoconEmptyValue(owner);

            // trim trailing whitespace if result is a literal
            if (value.Type == HoconType.Literal)
            {
                if (value[value.Count - 1] is HoconWhitespace)
                    value.RemoveAt(value.Count - 1);
            }
            return value;
        }

        private HoconArray ParsePlusEqualAssignArray(IHoconElement owner)
        {
            // sanity check
            if (_tokens.Current.Type != TokenType.PlusEqualAssign)
                throw HoconParserException.Create(_tokens.Current, Path,
                    "Failed to parse Hocon field with += operator. " +
                    $"Expected {TokenType.PlusEqualAssign}, found {_tokens.Current.Type} instead.");

            var currentArray = new HoconArray(owner);

            // consume += operator token
            _tokens.ToNextSignificant();

            switch (_tokens.Current.Type)
            {
                case TokenType.Include:
                    var includeToken = _tokens.Current;
                    var includeValue = ParseInclude();
                    if (includeValue.Type == HoconType.Empty)
                        break;

                    if(currentArray.Type != HoconType.Empty && currentArray.Type != includeValue.Type)
                        throw HoconParserException.Create(includeToken, Path,
                            "Invalid Hocon include. Hocon config substitution type must be the same as the field it's merged into. " +
                            $"Expected type: `{currentArray.Type}`, type returned by include callback: `{includeValue.Type}`");

                    currentArray.Add((HoconValue)includeValue.Clone(currentArray));
                    break;

                case TokenType.StartOfArray:
                    // Array inside of arrays are parsed as values because it can be value concatenated with another array.
                    currentArray.Add(ParseValue(currentArray));
                    break;

                case TokenType.StartOfObject:
                    currentArray.Add(ParseValue(currentArray));
                    break;

                case TokenType.LiteralValue:
                    if (_tokens.Current.IsNonSignificant())
                        _tokens.ToNextSignificant();

                    if (_tokens.Current.Type != TokenType.LiteralValue)
                        break;

                    currentArray.Add(ParseValue(currentArray));
                    break;

                case TokenType.SubstituteOptional:
                case TokenType.SubstituteRequired:
                    currentArray.Add(ParseValue(currentArray));
                    break;

                default:
                    throw HoconParserException.Create(_tokens.Current, Path,
                        $"Failed to parse Hocon array. Expected {TokenType.EndOfArray} but found {_tokens.Current.Type} instead.");
            }

            return currentArray;
        }

        /// <summary>
        /// Retrieves the next array token from the tokenizer.
        /// </summary>
        /// <returns>An array of elements retrieved from the token.</returns>
        private HoconArray ParseArray(IHoconElement owner)
        {
            // sanity check
            if (_tokens.Current.Type != TokenType.StartOfArray)
                throw HoconParserException.Create(_tokens.Current, Path,
                    "Failed to parse Hocon array. " +
                    $"Expected {TokenType.StartOfArray}, found {_tokens.Current.Type} instead.");

            var currentArray = new HoconArray(owner);

            // consume start of array token
            _tokens.ToNextSignificant();

            var valueWasParsed = false;
            var parsing = true;
            while (parsing)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon array. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        currentArray.Add(ParseValue(currentArray));
                        valueWasParsed = true;
                        break;

                    case TokenType.LiteralValue:
                        if (_tokens.Current.IsNonSignificant())
                            _tokens.ToNextSignificant();

                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon array. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        currentArray.Add(ParseValue(currentArray));
                        valueWasParsed = true;
                        break;

                    case TokenType.StartOfObject:
                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon array. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        currentArray.Add(ParseValue(currentArray));
                        valueWasParsed = true;
                        break;

                    case TokenType.StartOfArray:
                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon array. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        // Array inside of arrays are parsed as values because it can be value concatenated with another array.
                        currentArray.Add(ParseValue(currentArray));
                        valueWasParsed = true;
                        break;

                    case TokenType.SubstituteOptional:
                    case TokenType.SubstituteRequired:
                        if (valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon array. Expected `{TokenType.Comma}` or `{TokenType.EndOfLine}, " +
                                $"found `{_tokens.Current.Type}` instead.");

                        currentArray.Add(ParseValue(currentArray));
                        valueWasParsed = true;
                        break;

                    case TokenType.EndOfArray:
                        valueWasParsed = false;
                        parsing = false;
                        break;

                    case TokenType.Comment:
                    case TokenType.EndOfLine:
                        valueWasParsed = false;
                        _tokens.ToNextSignificantLine();
                        break;

                    case TokenType.Comma:
                        if (!valueWasParsed)
                            throw HoconParserException.Create(_tokens.Current, Path,
                                $"Failed to parse Hocon array. Expected a valid value, found `{_tokens.Current.Type}` instead.");

                        valueWasParsed = false;
                        _tokens.ToNextSignificant();
                        break;

                    default:
                        throw HoconParserException.Create(_tokens.Current, Path,
                            $"Failed to parse Hocon array. Expected {TokenType.EndOfArray} but found {_tokens.Current.Type} instead.");
                }
            }

            // Consume end of array token
            _tokens.Next();
            return currentArray;
        }
    }
}