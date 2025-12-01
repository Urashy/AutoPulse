namespace Api_c_sharp.DTO;

public class ReinitialiseMdpDTO
{
    public int IdCompte { get; set; }
    public string Email { get; set; }
    public string? Code { get; set; }
}