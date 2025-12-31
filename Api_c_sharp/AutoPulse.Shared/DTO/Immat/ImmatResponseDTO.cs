namespace AutoPulse.Shared.DTO.Immat;

public class ImmatResponseDTO
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public VehicleDataDTO? VehicleData { get; set; }
}