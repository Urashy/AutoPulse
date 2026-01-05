namespace AutoPulse.Shared.DTO;

public class TokenEmailVerifDTO
{
    public string? Email { get; set; }
    public string? Code { get; set; }
    public string TypeToken { get; set; } = null!;
}