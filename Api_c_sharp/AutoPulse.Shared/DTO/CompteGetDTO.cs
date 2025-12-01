namespace AutoPulse.Shared.DTO;

public class CompteGetDTO
{
    public int IdCompte { get; set; }
    public string Pseudo { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string TypeCompte { get; set; }
    public DateTime DateInscription { get; set; }
}