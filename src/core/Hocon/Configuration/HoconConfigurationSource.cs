#if DNXCORE50 || DNX451
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Configuration.Hocon;
using Microsoft.Framework.Configuration;

namespace Akka.Hocon.Configuration
{
    public class HoconConfigurationSource : ConfigurationSource
    {
        private readonly Stack<string> _context = new Stack<string>(); 
        private string currentPath;

        public HoconConfigurationSource(string path, bool optional = false)
        {
            Path = path;
            Optional = optional;
        }

        /// <summary>
        /// Gets a value that determines if this instance of <see cref="HoconConfigurationSource"/> is optional.
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// The absolute path of the file backing this instance of <see cref="HoconConfigurationSource"/>.
        /// </summary>
        public string Path { get; }

        public override void Load()
        {
            if (!File.Exists(Path))
            {
                if (Optional)
                {
                    Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    throw new FileNotFoundException("Path not found: " + Path);
                }
            }
            else
            {
                var root = Parser.Parse(File.ReadAllText(Path), null);
                VisitHoconObject(root.Value.GetObject());
            }
        }

        private void VisitHoconObject(HoconObject jObject)
        {
            foreach (var property in jObject.Items)
            {
                EnterContext(property.Key);
                VisitHoconValue(property.Value);
                ExitContext();
            }
        }

        private void VisitHoconValue(HoconValue property)
        {
            if (property.IsString())
            {
                VisitString(property.GetString());
            }
            else if (property.IsObject())
            {
                VisitHoconObject(property.GetObject());
            }
            else if (property.IsArray())
            {
                VisitArray(property.GetArray());
            }
        }

        private void VisitArray(IList<HoconValue> array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitHoconValue(array[index]);
                ExitContext();
            }
        }

        private void VisitString(string data)
        {
            var key = currentPath;

            if (Data.ContainsKey(key))
            {
                throw new FormatException("Duplicate key error: " + key);
            }
            Data[key] = data.ToString();
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            currentPath = string.Join(":", _context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            currentPath = string.Join(":", _context.Reverse());
        }
    }
}
#endif