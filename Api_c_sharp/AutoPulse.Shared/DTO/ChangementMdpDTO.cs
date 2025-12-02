using System.ComponentModel.DataAnnotations;

namespace AutoPulse.Shared.DTO;

public class ChangementMdpDTO
{
    public int? IdCompte { get; set; }
    public string? Email { get; set; }
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caract�res.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",ErrorMessage = "Le mot de passe doit contenir au minimum : une majuscule, une minuscule, un chiffre et un caract�re sp�cial.")]
    public string MotDePasse { get; set; }
}