// -----------------------------------------------------------------------
// <copyright file="HoconSubstitution.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Hocon
{
    /// <summary>
    ///     This class represents a substitution element in a HOCON (Human-Optimized Config Object Notation)
    ///     configuration string.
    ///     <code>
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
        ///     Initializes a new instance of the <see cref="HoconSubstitution" /> class.
        /// </summary>
        /// <param name="parent">The <see cref="HoconValue" /> parent of this substitution.</param>
        /// <param name="path">The <see cref="HoconPath" /> that this substitution is pointing to.</param>
        /// <param name="required">Marks wether this substitution uses the ${? notation or not.</param>
        /// ///
        /// <param name="lineInfo">The <see cref="IHoconLineInfo" /> of this substitution, used for exception generation purposes.</param>
        internal HoconSubstitution(IHoconElement parent, HoconPath path, IHoconLineInfo lineInfo, bool required)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent), "HoconSubstitution parent can not be null.");

            if (!(parent is HoconValue))
                throw new HoconException("HoconSubstitution parent must be HoconValue.");

            Parent = parent;
            LinePosition = lineInfo.LinePosition;
            LineNumber = lineInfo.LineNumber;
            Required = required;
            Path = path;
            
            _parentsToResolveFor.Add(Parent as HoconValue);
        }

        public bool Required { get; }

        internal bool Removed { get; set; }
        
        internal HoconField ParentField
        {
            get
            {
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
                _parentsToResolveFor.ForEach(p => p.ResolveValue(this));
            }
        }

        /// <summary>
        ///     The Hocon node that owned this substitution node
        /// </summary>
        public IHoconElement Parent { get; }
        
        private readonly List<HoconValue> _parentsToResolveFor = new List<HoconValue>();

        public HoconType Type => ResolvedValue?.Type ?? HoconType.Empty;

        /// <inheritdoc />
        public string GetString()
        {
            return ResolvedValue?.GetString();
        }

        public string Raw => ResolvedValue?.Raw;

        /// <inheritdoc />
        public IList<HoconValue> GetArray()
        {
            return ResolvedValue?.GetArray();
        }

        /// <inheritdoc />
        public HoconObject GetObject()
        {
            return ResolvedValue?.GetObject();
        }

        /// <inheritdoc />
        public string ToString(int indent, int indentSize)
        {
            return ResolvedValue.ToString(indent, indentSize);
        }

        /// <summary>
        /// Returns substitution, that is safe to use as a copy (it's immutable anyway)
        /// </summary>
        /// <remarks>
        /// Reference to this substitution instance is already stored in parser, and it's value will be resolved
        /// at the end of parsing process.
        /// Now, one more parent is referencing this substitution as a child - so we need to register this
        /// parent here, and all parents will be notified about subsitution resolution via <see cref="ResolvedValue"/>
        /// setter.
        /// </remarks>
        public IHoconElement Clone(IHoconElement newParent)
        {
            if (newParent == null)
                throw new ArgumentNullException(nameof(newParent), "HoconSubstitution parent can not be null.");

            if (!(newParent is HoconValue))
                throw new HoconException("HoconSubstitution parent must be HoconValue.");
            
            _parentsToResolveFor.Add((HoconValue) newParent);
            
            // No copy required, substitutions are never changing state (except setting resolved value)
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

        public int LineNumber { get; }

        public int LinePosition { get; }

        /// <summary>
        ///     Returns the string representation of this element.
        /// </summary>
        /// <returns>The value of this element.</returns>
        public override string ToString()
        {
            return ResolvedValue.ToString(0, 2);
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