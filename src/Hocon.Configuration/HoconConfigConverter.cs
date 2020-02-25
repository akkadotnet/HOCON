using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hocon
{
    class HoconConfigConverter : JsonConverter<Config>
    {
        public override Config ReadJson(JsonReader reader, Type objectType, Config existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize<JObject>(reader);
            var data = obj["$"].Value<string>().Substring(1);
            return HoconConfigurationFactory.ParseString(data);
        }

        public override void WriteJson(JsonWriter writer, Config value, JsonSerializer serializer)
        {
            var jsonObject = new JObject
            {
                { "$", $"H{value.Root.ToString()}" },
            };
            serializer.Serialize(writer, jsonObject);
        }
    }
}
