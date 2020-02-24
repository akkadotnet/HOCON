using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hocon
{
    class HoconConfigConverter : JsonConverter<Config>
    {
        public override Config ReadJson(JsonReader reader, Type objectType, Config existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var hocon = (string)reader.Value;
            return HoconConfigurationFactory.ParseString(hocon);
        }

        public override void WriteJson(JsonWriter writer, Config value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Root.ToString());
        }
    }
}
