using System.ComponentModel.DataAnnotations;

namespace AutoPulse.Shared.DTO
{
    public class PlainteCreateDTO
    {
        [Required(ErrorMessage = "Le contenu de la plainte est obligatoire.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Le contenu doit faire entre 10 et 1000 caractères.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'identifiant du compte est invalide.")]
        public int IdCompte { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'identifiant de l'état est invalide.")]
        public int IdEtat { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'identifiant du signalement est invalide.")]
        public int IdSignalement { get; set; }
    }
}