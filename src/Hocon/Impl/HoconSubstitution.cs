//-----------------------------------------------------------------------
// <copyright file="HoconSubstitution.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Hocon
{
    /// <summary>
    /// This class represents a substitution element in a HOCON (Human-Optimized Config Object Notation)
    /// configuration string.
    /// <code>
    /// akka {  
    ///   defaultInstances = 10
    ///   deployment{
    ///     /user/time{
    ///       nr-of-instances = ${defaultInstances}
    ///     }
    ///   }
    /// }
    /// </code>
    /// </summary>
    public sealed class HoconSubstitution : IHoconElement, IHoconLineInfo
    {
        private HoconValue _resolvedValue;

        /// <summary>
        ///     The Hocon node that owned this substitution node
        /// </summary>
        public IHoconElement Parent { get; private set; }

        public int LineNumber { get; }

        public int LinePosition { get; }

        public bool Required { get; }

        internal HoconField ParentField
        {
            get  {
                var p = Parent;
                while (p != null && !(p is HoconField))
                    p = p.Parent;
                return p as HoconField;
            }
        }

        /// <summary>
        ///     The full path to the value which should substitute this instance.
        /// </summary>
        public HoconPath Path { get; }

        /// <summary>
        ///     The evaluated value from the Path property
        /// </summary>
        public HoconValue ResolvedValue
        {
            get => _resolvedValue;
            internal set
            {
                _resolvedValue = value;
                switch (Parent)
                {
                    case HoconValue v:
                        v.ResolveValue(this);
                        break;
                    case HoconArray a:
                        a.ResolveValue(this);
                        break;
                }
            }
        }

        public HoconType Type => ResolvedValue?.Type ?? HoconType.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HoconSubstitution" /> class.
        /// </summary>
        /// <param name="parent">The <see cref="HoconValue"/> parent of this substitution.</param>
        /// <param name="path">The <see cref="HoconPath"/> that this substitution is pointing to.</param>
        /// <param name="required">Marks wether this substitution uses the ${? notation or not.</param>
        /// /// <param name="lineInfo">The <see cref="IHoconLineInfo"/> of this substitution, used for exception generation purposes.</param>
        internal HoconSubstitution(IHoconElement parent, HoconPath path, IHoconLineInfo lineInfo, bool required)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent), "Hocon substitute parent can not be null.");
            LinePosition = lineInfo.LinePosition;
            LineNumber = lineInfo.LineNumber;
            Required = required;
            Path = path;
        }

        /// <inheritdoc />
        public string GetString() => ResolvedValue?.GetString();

        public string Raw => ResolvedValue?.Raw;

        /// <inheritdoc />
        public List<HoconValue> GetArray() => ResolvedValue?.GetArray() ?? new List<HoconValue>();

        /// <inheritdoc />
        public HoconObject GetObject() => ResolvedValue?.GetObject() ?? new HoconObject(Parent);

        /// <summary>
        /// Returns the string representation of this element.
        /// </summary>
        /// <returns>The value of this element.</returns>
        public override string ToString()
            => ResolvedValue.ToString(0, 2);

        /// <inheritdoc />
        public string ToString(int indent, int indentSize)
            => ResolvedValue.ToString(indent, indentSize);

        // Substitution can not be cloned because it is resolved at the end of the parsing process.
        public IHoconElement Clone(IHoconElement newParent)
        {
            Parent = newParent;
            return this;
        }

        public bool Equals(IHoconElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is HoconSubstitution sub)
                return Path == sub.Path;

            return !(_resolvedValue is null) && _resolvedValue.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is IHoconElement element && Equals(element);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(HoconSubstitution left, HoconSubstitution right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoconSubstitution left, HoconSubstitution right)
        {
            return !Equals(left, right);
        }
    }
}

