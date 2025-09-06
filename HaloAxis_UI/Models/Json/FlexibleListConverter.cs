using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloAxis_UI.Models.Json
{
    /// <summary>
    /// Allows a list property to deserialize from:
    ///  - [ ... ]
    ///  - { "$values": [ ... ] }  (EF-style)
    ///  - { ...single object... } (wrapped single)
    ///  - null  => empty list
    /// </summary>
    public sealed class FlexibleListConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
                return JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                if (TryGetProp(root, out var values, "$values", "values", "Value", "Items", "items"))
                    return JsonSerializer.Deserialize<List<T>>(values.GetRawText(), options) ?? new();

                var one = JsonSerializer.Deserialize<T>(root.GetRawText(), options);
                return one is null ? new() : new() { one };
            }

            if (reader.TokenType == JsonTokenType.Null) return new();

            throw new JsonException("Expected array or object for list.");
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, options);

        private static bool TryGetProp(JsonElement obj, out JsonElement value, params string[] names)
        {
            foreach (var p in obj.EnumerateObject())
                foreach (var n in names)
                    if (string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase))
                    { value = p.Value; return true; }
            value = default; return false;
        }
    }
}
