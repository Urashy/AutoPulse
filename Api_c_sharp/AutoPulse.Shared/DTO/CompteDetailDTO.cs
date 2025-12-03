namespace AutoPulse.Shared.DTO;

public class CompteDetailDTO
{
    public int IdCompte { get; set; }
    public string Pseudo { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateDerniereConnexion { get; set; }
    public DateTime DateNaissance { get; set; }
    public string Biographie { get; set; }
    public string TypeCompte { get; set; }
    public int IdTypeCompte { get; set; }
    public string NumeroSiret { get; set; }
    public string RaisonSociale { get; set; }
    public List<AdresseDTO> Adresses { get; set; } = new List<AdresseDTO>();
    public int idImage { get; set; }
}