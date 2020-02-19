// -----------------------------------------------------------------------
// <copyright file="HoconConfigurationFileParser.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Hocon.Extensions.Configuration
{
    internal class HoconConfigurationFileParser
    {
        private readonly Stack<string> _context = new Stack<string>();

        private readonly IDictionary<string, string> _data =
            new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string _currentPath;

        public IDictionary<string, string> Parse(Stream input)
        {
            _data.Clear();

            using (var textStream = new StreamReader(input))
            {
                var content = textStream.ReadToEnd();
                var hoconConfig = HoconParser.Parse(content);
                VisitHoconObject(hoconConfig.Value.GetObject());
            }

            return _data;
        }

        private void VisitHoconObject(HoconObject hObject)
        {
            foreach (var field in hObject)
            {
                EnterContext(field.Key);
                VisitHoconField(field.Value);
                ExitContext();
            }
        }

        private void VisitHoconField(HoconField property)
        {
            VisitObject(property.Value);
        }

        private void VisitObject(HoconValue value)
        {
            switch (value.Type)
            {
                case HoconType.Object:
                    VisitHoconObject(value.GetObject());
                    break;

                case HoconType.Array:
                    VisitArray(value.GetArray());
                    break;

                case HoconType.Boolean:
                case HoconType.Number:
                case HoconType.String:
                    VisitPrimitive(value);
                    break;
            }
        }

        private void VisitArray(IList<HoconValue> array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitObject(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(IHoconElement data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key)) throw new FormatException(string.Format(Resources.Error_KeyIsDuplicated, key));

            _data[key] = data.GetString();
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }
}