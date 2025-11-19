namespace Api_c_sharp.DTO;

public class CompteCreateDTO
{
    public string Pseudo { get; set; }
    public string MotDePasse { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public DateTime DateNaissance { get; set; }
    public string? Biographie { get; set; }
    public int IdTypeCompte { get; set; }
    public string NumeroSiret { get; set; }
    public string RaisonSociale { get; set; }
}
