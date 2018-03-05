using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Yuki.Core.Resolver
{
    class ResolverMappingJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Mapping);
        }

        public override object ReadJson(JsonReader reader, Type objectType, 
            object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            if (jObject["multi-implementation"].Value<bool>() == true)
                return jObject.ToObject<MultiMapping>(serializer);

            if (jObject["multi-implementation"].Value<bool>() == false)
                return jObject.ToObject<SingleMapping>(serializer);

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
