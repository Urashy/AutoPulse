namespace AutoPulse.Shared.DTO.Authentification;

public class LoginRequest
{
    public string? Email { get; set; }
    public string? MotDePasse { get; set; }
    public bool RememberMe { get; set; } = false;
}