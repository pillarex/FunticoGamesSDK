using System;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels.Converters
{
    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Boolean)
            {
                return (bool)reader.Value;
            }
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value) != 0;
            }
            if (reader.TokenType == JsonToken.String)
            {
                var str = reader.Value?.ToString().Trim().ToLower();
                if (str == "true" || str == "1") return true;
                if (str == "false" || str == "0") return false;
            }

            throw new JsonSerializationException($"Неправильне значення для bool: {reader.Value}");
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}