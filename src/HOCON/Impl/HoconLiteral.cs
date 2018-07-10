//-----------------------------------------------------------------------
// <copyright file="HoconLiteral.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hocon
{
    /// <summary>
    /// This class represents a literal element in a HOCON (Human-Optimized Config Object Notation)
    /// configuration string.
    /// <code>
    /// akka {  
    ///   actor {
    ///     provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
    ///   }
    /// }
    /// </code>
    /// </summary>
    public abstract class HoconLiteral : IHoconElement
    {
        public IHoconElement Parent { get; }
        public HoconType Type => HoconType.Literal;
        public abstract HoconLiteralType LiteralType { get; }

        /// <summary>
        /// Gets or sets the value of this element.
        /// </summary>
        public virtual string Value { get; }

        /// <summary>
        /// Retrieves the raw string representation of this element.
        /// </summary>
        /// <returns>The raw value of this element.</returns>
        public virtual string Raw => Value;

        protected HoconLiteral(IHoconElement parent, string value)
        {
            Parent = parent;
            Value = value;
        }

        /// <inheritdoc />
        /// <exception cref="T:Hocon.HoconException">
        /// This element is a string literal. It is not an object.
        /// Therefore this method will throw an exception.
        /// </exception>
        public HoconObject GetObject()
            => throw new HoconException("Hocon literal could not be converted to object.");

        /// <inheritdoc />
        public string GetString() => Value;

        /// <summary>
        /// Retrieves a list of elements associated with this element.
        /// </summary>
        /// <returns>
        /// A list of elements associated with this element.
        /// </returns>
        /// <exception cref="HoconException">
        /// This element is a string literal. It is not an array.
        /// Therefore this method will throw an exception.
        /// </exception>
        public IList<HoconValue> GetArray()
            => throw new HoconException("Hocon literal could not be converted to array.");

        /// <summary>
        /// Returns the string representation of this element.
        /// </summary>
        /// <returns>The value of this element.</returns>
        public override string ToString() => Raw;

        /// <inheritdoc />
        public string ToString(int indent, int indentSize)
            => Raw;

        public abstract IHoconElement Clone(IHoconElement newParent);

        internal static HoconLiteral Create(IHoconElement owner, Token token)
        {
            switch (token.LiteralType)
            {
                case TokenLiteralType.Null:
                    return (new HoconNull(owner));

                case TokenLiteralType.Bool:
                    return (new HoconBool(owner, token.Value));

                case TokenLiteralType.Whitespace:
                    return (new HoconWhitespace(owner, token.Value));

                case TokenLiteralType.UnquotedLiteralValue:
                    return (new HoconUnquotedString(owner, token.Value));

                case TokenLiteralType.QuotedLiteralValue:
                    return (new HoconQuotedString(owner, token.Value));

                case TokenLiteralType.TripleQuotedLiteralValue:
                    return (new HoconTripleQuotedString(owner, token.Value));

                case TokenLiteralType.Long:
                    return (new HoconLong(owner, token.Value));

                case TokenLiteralType.Double:
                    return (new HoconDouble(owner, token.Value));

                default:
                    throw new HoconException($"Unknown token literal type: {token.Value}");
            }

        }
    }

    public class HoconNull : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.Null;
        public override string Raw => "null";
        public override string Value => null;

        public HoconNull(IHoconElement parent) : base(parent, "null") { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconNull(newParent);
    }

    public class HoconBool : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.Bool;

        public HoconBool(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconBool(newParent, Value);
    }

    public class HoconDouble : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.Double;

        public HoconDouble(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconDouble(newParent, Value);
    }

    public class HoconLong : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.Long;

        public HoconLong(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconLong(newParent, Value);
    }

    public class HoconUnquotedString : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.UnquotedString;

        public HoconUnquotedString(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconUnquotedString(newParent, Value);
    }

    public class HoconQuotedString : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.QuotedString;
        public override string Raw => "\"" + Value + "\"";

        public HoconQuotedString(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconQuotedString(newParent, Value);
    }

    public class HoconTripleQuotedString : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.TripleQuotedString;
        public override string Raw => "\"\"\"" + Value + "\"\"\"";

        public HoconTripleQuotedString(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconTripleQuotedString(newParent, Value);
    }

    public class HoconWhitespace : HoconLiteral
    {
        public override HoconLiteralType LiteralType => HoconLiteralType.Whitespace;

        public HoconWhitespace(IHoconElement parent, string value) : base(parent, value) { }

        public override IHoconElement Clone(IHoconElement newParent)
            => new HoconWhitespace(newParent, Value);
    }
}

