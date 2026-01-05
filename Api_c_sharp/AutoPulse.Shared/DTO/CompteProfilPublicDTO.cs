namespace AutoPulse.Shared.DTO;

public class CompteProfilPublicDTO
{
    public int IdCompte { get; set; }
    public string? Pseudo { get; set; }
    public string? Biographie { get; set; }
    public DateTime DateInscription { get; set; }
    public string? TypeCompte { get; set; }
    public int IdTypeCompte { get; set; }
    public string? ImageProfil { get; set; }
    public int NombreAnnonces { get; set; }
    public double NoteMoyenne { get; set; }
    public int NombreAvis { get; set; }    
    public string? RaisonSociale { get; set; }

}