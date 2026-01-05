namespace BlazorAutoPulse.Model;
using AutoPulse.Shared.DTO;

public class LoginResponse
{
    public string Token { get; set; }
    public CompteDetailDTO UserDetails { get; set; }
}