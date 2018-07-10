//-----------------------------------------------------------------------
// <copyright file="HoconArray.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hocon
{
    /// <summary>
    /// This class represents a tuple representing a Hocon field.
    /// Only used during object parsing, so it is intentionally set as internal only
    /// <code>
    /// root {
    ///     items = [
    ///       "1",
    ///       "2"]
    /// }
    /// </code>
    /// </summary>

    internal sealed class HoconField:IHoconElement
    {
        public IHoconElement Parent => throw new NotImplementedException();
        public HoconType Type => throw new NotImplementedException();
        public bool IsObject => throw new NotImplementedException();
        public bool IsLiteral => throw new NotImplementedException();
        public bool IsArray => throw new NotImplementedException();

        public HoconField(string key, HoconValue value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public HoconValue Value { get; }

        public HoconObject GetObject()
            => throw new NotImplementedException();

        public string GetString()
            => throw new NotImplementedException();

        public string Raw
            => throw new NotImplementedException();

        public IList<HoconValue> GetArray()
            => throw new NotImplementedException();

        public IHoconElement Clone(IHoconElement newParent)
            => throw new NotImplementedException();

        public string ToString(int indent, int indentSize)
            => throw new NotImplementedException();
    }
}
