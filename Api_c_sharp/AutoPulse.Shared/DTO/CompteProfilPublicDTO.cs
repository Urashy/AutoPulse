namespace AutoPulse.Shared.DTO;

public class CompteProfilPublicDTO
{
    public int IdCompte { get; set; }
    public string Pseudo { get; set; }
    public string Biographie { get; set; }
    public DateTime DateInscription { get; set; }
    public string TypeCompte { get; set; }
    public string ImageProfil { get; set; }
    public int NombreAnnonces { get; set; }
    public decimal NoteMoyenne { get; set; }
    public int NombreAvis { get; set; }
}