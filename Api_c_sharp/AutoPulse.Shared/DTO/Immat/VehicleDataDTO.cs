namespace AutoPulse.Shared.DTO.Immat;

public class VehicleDataDTO
{
    // Données de base
    public string PlateNumber { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? FuelType { get; set; }
    public string? GearBox { get; set; }
    public string? Category { get; set; }
        
    // Caractéristiques techniques
    public int? Mileage { get; set; }
    public int? Horsepower { get; set; }
    public int? Torque { get; set; }
    public int? Cylinders { get; set; }
    public double? EngineVolume { get; set; }
    public string? DriveWheels { get; set; }
        
    // Détails
    public int? Doors { get; set; }
    public int? Seats { get; set; }
    public string? Color { get; set; }
    public int? Airbags { get; set; }
    public bool? LeatherInterior { get; set; }
    public bool? LeftHandDrive { get; set; }
    public DateTime? FirstRegistration { get; set; }
        
    // État de sélection pour chaque champ (utilisé dans l'UI)
    public Dictionary<string, bool> SelectedFields { get; set; } = new();
}