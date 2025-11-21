using System.ComponentModel.DataAnnotations;

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

    [StringLength(14, MinimumLength = 14, ErrorMessage = "Le numéro SIRET doit contenir exactement 14 chiffres")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "Le SIRET doit contenir uniquement des chiffres")]
    public string? NumeroSiret { get; set; }
    public string? RaisonSociale { get; set; }
}
