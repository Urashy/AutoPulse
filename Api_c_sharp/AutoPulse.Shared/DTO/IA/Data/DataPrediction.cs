using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Data;

public class DataPrediction : DataAI
{
    public override string Type => "prediction";

    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("prod_year")]
    public int? ProdYear { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("leather_interior")]
    public string? LeatherInterior { get; set; }

    [JsonPropertyName("fuel_type")]
    public string? FuelType { get; set; }

    [JsonPropertyName("engine_volume")]
    public float? EngineVolume { get; set; }

    [JsonPropertyName("mileage")]
    public string? Mileage { get; set; }

    [JsonPropertyName("cylinders")]
    public float? Cylinders { get; set; }

    [JsonPropertyName("gear_box_type")]
    public string? GearBoxType { get; set; }

    [JsonPropertyName("drive_wheels")]
    public string? DriveWheels { get; set; }

    [JsonPropertyName("doors")]
    public string? Doors { get; set; }

    [JsonPropertyName("wheel")]
    public string? Wheel { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("airbags")]
    public int? Airbags { get; set; }

    [JsonPropertyName("levy")]
    public float? Levy { get; set; }
}