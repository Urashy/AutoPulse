using System.Text.Json;
using System.Text.Json.Serialization;
using AutoPulse.Shared.DTO.IA.Result;

namespace BlazorAutoPulse.Service;

public class ResultatAIJsonConverter : JsonConverter<ResultatAI>
{
    public override ResultatAI Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Console.WriteLine("=== Converter appelé ===");
        
        using var doc = JsonDocument.ParseValue(ref reader);
        var rootElement = doc.RootElement;
        
        if (!rootElement.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("La propriété 'type' est manquante");
        }
        
        var type = typeProperty.GetString();
        Console.WriteLine($"Type détecté: {type}");

        // Créer de nouvelles options SANS le converter pour éviter la récursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        newOptions.PropertyNameCaseInsensitive = true;

        return type switch
        {
            "cnn" => JsonSerializer.Deserialize<ResultatCNN>(rootElement.GetRawText(), newOptions)!,
            "ajustement" => JsonSerializer.Deserialize<ResultatAjustement>(rootElement.GetRawText(), newOptions)!,
            "prediction" => JsonSerializer.Deserialize<ResultatPrediction>(rootElement.GetRawText(), newOptions)!,
            _ => throw new JsonException($"Type inconnu : {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ResultatAI value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, (object)value, options);
}