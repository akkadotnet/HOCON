//-----------------------------------------------------------------------
// <copyright file="HoconSubstitution.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    ///       nr-of-instances = $defaultInstances
    ///     }
    ///   }
    /// }
    /// </code>
    /// </summary>
    public class HoconSubstitution : IHoconElement, IMightBeAHoconObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HoconSubstitution"/> class.
        /// </summary>
        protected HoconSubstitution()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HoconSubstitution" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public HoconSubstitution(HoconValue parent, string path)
        {
            Owner = parent ?? throw new ArgumentNullException(nameof(parent), "Hocon substitute parent can not be null.");

            if (path.StartsWith("?"))
            {
                IsQuestionMark = true;
                path = path.Substring(1);
            }

            Debug.WriteLine($"Path = {path}");
            Path = path;
        }

        public bool IsQuestionMark { get; }
        public bool IsResolved => ResolvedValue != null;

        /// <summary>
        ///     The Hocon node that owned this substitution node
        /// </summary>
        public HoconValue Owner { get; }

        /// <summary>
        ///     The full path to the value which should substitute this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     The evaluated value from the Path property
        /// </summary>
        public HoconValue ResolvedValue { get; set; }

        /// <summary>
        /// Determines whether this element is a string and all of its characters are whitespace characters.
        /// </summary>
        /// <returns><c>false</c>.</returns>
        public bool IsWhitespace()
        {
            return false;
        }

        /// <summary>
        /// Determines whether this element is a string.
        /// </summary>
        /// <returns><c>true</c> if this element is a string; otherwise <c>false</c></returns>
        public bool IsString()
        {
            return ResolvedValue != null && ResolvedValue.IsString();
        }

        /// <summary>
        /// Retrieves the string representation of this element.
        /// </summary>
        /// <returns>The string representation of this element.</returns>
        public string GetString()
        {
            return ResolvedValue.GetString();
        }

        /// <summary>
        /// Determines whether this element is an array.
        /// </summary>
        /// <returns><c>true</c> if this element is aan array; otherwise <c>false</c></returns>
        public bool IsArray()
        {
            return ResolvedValue != null && ResolvedValue.IsArray();
        }

        /// <summary>
        /// Retrieves a list of elements associated with this element.
        /// </summary>
        /// <returns>A list of elements associated with this element.</returns>
        public IList<HoconValue> GetArray()
        {
            return ResolvedValue.GetArray();
        }

        /// <summary>
        /// Determines whether this element is a HOCON object.
        /// </summary>
        /// <returns><c>true</c> if this element is a HOCON object; otherwise <c>false</c></returns>
        public bool IsObject()
        {
            return ResolvedValue != null && ResolvedValue.IsObject();
        }

        /// <summary>
        /// Retrieves the HOCON object representation of this element.
        /// </summary>
        /// <returns>The HOCON object representation of this element.</returns>
        public HoconObject GetObject()
        {
            return ResolvedValue.GetObject();
        }
    }
}

