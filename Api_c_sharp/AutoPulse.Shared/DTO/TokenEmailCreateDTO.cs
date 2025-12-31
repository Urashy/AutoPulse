namespace AutoPulse.Shared.DTO;

public class TokenEmailCreateDTO
{
    public int IdCompte { get; set; }
    public string Email { get; set; } = null!;
    public string TypeToken { get; set; } = null!;
}