using System.ComponentModel.DataAnnotations;

namespace AutoPulse.Shared.DTO
{
    public class PlainteDTO
    {
        public int IdPlainte { get; set; }
        [Required(ErrorMessage = "Le contenu de la plainte est obligatoire.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Le contenu doit faire entre 10 et 1000 caractères.")]
        public string Contenu { get; set; }
        public int IdCompte { get; set; }
        public int IdSignalement { get; set; }
        public DateTime DateCreation { get; set; }
    }
}