// -----------------------------------------------------------------------
// <copyright file="HoconLiteral.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Hocon
{
    /// <summary>
    ///     This class represents a literal element in a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    ///     <code>
    /// akka {  
    ///   actor {
    ///     provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
    ///   }
    /// }
    /// </code>
    /// </summary>
    public abstract class HoconLiteral : IHoconElement
    {
        protected HoconLiteral(IHoconElement parent, string value)
        {
            Parent = parent;
            Value = value;
        }

        public abstract HoconLiteralType LiteralType { get; }

        /// <summary>
        ///     Gets or sets the value of this element.
        /// </summary>
        public virtual string Value { get; }

        public IHoconElement Parent { get; }
        public abstract HoconType Type { get; }

        /// <summary>
        ///     Retrieves the raw string representation of this element.
        /// </summary>
        /// <returns>The raw value of this element.</returns>
        public virtual string Raw => Value;

        /// <inheritdoc />
        /// <exception cref="T:Hocon.HoconException">
        ///     This element is a string literal. It is not an object.
        ///     Therefore this method will throw an exception.
        /// </exception>
        public HoconObject GetObject()
        {
            throw new HoconException("Hocon literal could not be converted to object.");
        }

        /// <inheritdoc />
        public string GetString()
        {
            return Value;
        }

        /// <summary>
        ///     Retrieves a list of elements associated with this element.
        /// </summary>
        /// <returns>
        ///     A list of elements associated with this element.
        /// </returns>
        /// <exception cref="HoconException">
        ///     This element is a string literal. It is not an array.
        ///     Therefore this method will throw an exception.
        /// </exception>
        public IList<HoconValue> GetArray()
        {
            throw new HoconException("Hocon literal could not be converted to array.");
        }

        /// <inheritdoc />
        public string ToString(int indent, int indentSize)
        {
            return Raw;
        }

        public abstract IHoconElement Clone(IHoconElement newParent);

        public bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && string.Equals(Value, other.GetString());
        }

        /// <summary>
        ///     Returns the string representation of this element.
        /// </summary>
        /// <returns>The value of this element.</returns>
        public override string ToString()
        {
            return Raw;
        }

        internal static HoconLiteral Create(IHoconElement owner, Token token)
        {
            switch (token.LiteralType)
            {
                case TokenLiteralType.Null:
                    return new HoconNull(owner);

                case TokenLiteralType.Bool:
                    return new HoconBool(owner, token.Value);

                case TokenLiteralType.Whitespace:
                    return new HoconWhitespace(owner, token.Value);

                case TokenLiteralType.UnquotedLiteralValue:
                    return new HoconUnquotedString(owner, token.Value);

                case TokenLiteralType.QuotedLiteralValue:
                    return new HoconQuotedString(owner, token.Value);

                case TokenLiteralType.TripleQuotedLiteralValue:
                    return new HoconTripleQuotedString(owner, token.Value);

                case TokenLiteralType.Long:
                    return new HoconLong(owner, token.Value);

                case TokenLiteralType.Double:
                    return new HoconDouble(owner, token.Value);

                case TokenLiteralType.Hex:
                    return new HoconHex(owner, token.Value);

                case TokenLiteralType.Octal:
                    return new HoconOctal(owner, token.Value);

                default:
                    throw new HoconException($"Unknown token literal type: {token.Value}");
            }
        }

        public override bool Equals(object obj)
        {
            // Needs to be cast to IHoconElement because there are cases 
            // where a HoconLiteral can be the same as a HoconValue
            return obj is IHoconElement other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(HoconLiteral left, HoconLiteral right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconLiteral left, HoconLiteral right)
        {
            return !Equals(left, right);
        }
    }

    public sealed class HoconNull : HoconLiteral
    {
        public HoconNull(IHoconElement parent) : base(parent, "null")
        {
        }

        public override HoconType Type => HoconType.String;
        public override HoconLiteralType LiteralType => HoconLiteralType.Null;
        public override string Raw => "null";
        public override string Value => null;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconNull(newParent);
        }
    }

    public sealed class HoconBool : HoconLiteral
    {
        public HoconBool(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.Boolean;
        public override HoconLiteralType LiteralType => HoconLiteralType.Bool;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconBool(newParent, Value);
        }
    }

    public sealed class HoconDouble : HoconLiteral
    {
        public HoconDouble(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.Number;
        public override HoconLiteralType LiteralType => HoconLiteralType.Double;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconDouble(newParent, Value);
        }
    }

    public sealed class HoconLong : HoconLiteral
    {
        public HoconLong(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.Number;
        public override HoconLiteralType LiteralType => HoconLiteralType.Long;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconLong(newParent, Value);
        }
    }

    public sealed class HoconHex : HoconLiteral
    {
        public HoconHex(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.Number;
        public override HoconLiteralType LiteralType => HoconLiteralType.Hex;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconHex(newParent, Value);
        }
    }

    public sealed class HoconOctal : HoconLiteral
    {
        public HoconOctal(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconLiteralType LiteralType => HoconLiteralType.Long;

        public override HoconType Type => HoconType.Number;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconOctal(newParent, Value);
        }
    }

    public sealed class HoconUnquotedString : HoconLiteral
    {
        public HoconUnquotedString(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.String;
        public override HoconLiteralType LiteralType => HoconLiteralType.UnquotedString;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconUnquotedString(newParent, Value);
        }
    }

    public sealed class HoconQuotedString : HoconLiteral
    {
        public HoconQuotedString(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.String;
        public override HoconLiteralType LiteralType => HoconLiteralType.QuotedString;
        public override string Raw => "\"" + Value + "\"";

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconQuotedString(newParent, Value);
        }
    }

    public sealed class HoconTripleQuotedString : HoconLiteral
    {
        public HoconTripleQuotedString(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.String;
        public override HoconLiteralType LiteralType => HoconLiteralType.TripleQuotedString;
        public override string Raw => "\"\"\"" + Value + "\"\"\"";

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconTripleQuotedString(newParent, Value);
        }
    }

    public sealed class HoconWhitespace : HoconLiteral
    {
        public HoconWhitespace(IHoconElement parent, string value) : base(parent, value)
        {
        }

        public override HoconType Type => HoconType.String;
        public override HoconLiteralType LiteralType => HoconLiteralType.Whitespace;

        public override IHoconElement Clone(IHoconElement newParent)
        {
            return new HoconWhitespace(newParent, Value);
        }
    }
}