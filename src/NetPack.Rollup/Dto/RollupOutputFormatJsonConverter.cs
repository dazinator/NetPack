using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetPack.Rollup
{
    public class RollupOutputFormatJsonConverter : JsonConverter<RollupOutputFormat>
    {
        public override RollupOutputFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            return value?.ToLowerInvariant() switch
            {
                "system" => RollupOutputFormat.System,
                "amd" => RollupOutputFormat.Amd,
                "cjs" => RollupOutputFormat.Cjs,
                "esm" => RollupOutputFormat.Esm,
                "es" => RollupOutputFormat.Esm,
                "iife" => RollupOutputFormat.Iife,
                "umd" => RollupOutputFormat.Umd,
                _ => throw new JsonException($"Unknown RollupOutputFormat value: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, RollupOutputFormat value, JsonSerializerOptions options)
        {
            string stringValue = value switch
            {
                RollupOutputFormat.System => "system",
                RollupOutputFormat.Amd => "amd",
                RollupOutputFormat.Cjs => "cjs",
                RollupOutputFormat.Esm => "esm",
                RollupOutputFormat.Iife => "iife",
                RollupOutputFormat.Umd => "umd",
                _ => throw new JsonException($"Unknown RollupOutputFormat value: {value}")
            };
            writer.WriteStringValue(stringValue);
        }
    }
}
