using System.Text.Json;
using System.Text.Json.Serialization;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Core.JsonConverters;

/// <summary>
/// Custom JSON converter for ARCRating enum to handle hyphenated format (ARC-a, ARC-b, ARC-c, ARC-d)
/// Frontend sends: "ARC-b" (lowercase with hyphen)
/// Backend enum values: ARC_a, ARC_b, ARC_c, ARC_d (underscore)
/// </summary>
public class ARCRatingConverter : JsonConverter<ARCRating>
{
    public override ARCRating Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string for ARCRating, got {reader.TokenType}");
        }

        string value = reader.GetString()!;
        
        // Handle both hyphenated (ARC-b) and underscore (ARC_b) formats, case-insensitive
        return value.Replace("-", "_").ToUpperInvariant() switch
        {
            "ARC_A" => ARCRating.ARC_a,
            "ARC_B" => ARCRating.ARC_b,
            "ARC_C" => ARCRating.ARC_c,
            "ARC_D" => ARCRating.ARC_d,
            _ => throw new JsonException($"Unknown ARCRating value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ARCRating value, JsonSerializerOptions options)
    {
        // Write in hyphenated lowercase format (ARC-a, ARC-b, etc.) for frontend compatibility
        string stringValue = value switch
        {
            ARCRating.ARC_a => "ARC-a",
            ARCRating.ARC_b => "ARC-b",
            ARCRating.ARC_c => "ARC-c",
            ARCRating.ARC_d => "ARC-d",
            _ => throw new JsonException($"Unknown ARCRating value: {value}")
        };

        writer.WriteStringValue(stringValue);
    }
}
