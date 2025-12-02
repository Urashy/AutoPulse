using System.ComponentModel.DataAnnotations;

namespace AutoPulse.Shared.DTO;

public class CompteCreateDTO
{
    public string Pseudo { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",ErrorMessage = "Le mot de passe doit contenir au minimum : une majuscule, une minuscule, un chiffre et un caractère spécial.")]
    public string MotDePasse { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }

    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
    [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$",ErrorMessage = "Format d'email invalide.")]
    public string Email { get; set; }
    public DateTime DateNaissance { get; set; }
    public string? Biographie { get; set; }
    public int IdTypeCompte { get; set; }

    [StringLength(14, MinimumLength = 14, ErrorMessage = "Le numéro SIRET doit contenir exactement 14 chiffres")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "Le SIRET doit contenir uniquement des chiffres")]
    public string? NumeroSiret { get; set; }
    public string? RaisonSociale { get; set; }
}
