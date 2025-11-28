namespace Api_c_sharp.DTO;

public class CompteUpdateDTO
{
    public int IdCompte { get; set; }
    public string Pseudo { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public DateTime DateNaissance { get; set; }
    public string Biographie { get; set; }
    public int IdTypeCompte { get; set; }
    public string NumeroSiret { get; set; }
    public string RaisonSociale { get; set; }
}





