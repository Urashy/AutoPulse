using System.ComponentModel.DataAnnotations;

namespace AutoPulse.Shared.DTO
{
    public class PlainteCreateDTO
    {
        [Required(ErrorMessage = "Le contenu de la plainte est obligatoire.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Le contenu doit faire entre 10 et 1000 caractères.")]
        public string Contenu { get; set; }
    }
}