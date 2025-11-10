using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models { 

    [Table("t_e_comptepro_cpr")]
    public class ComptePro : Compte
    {
        [Column("cpr_siret")]
        [Required]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "Le numéro SIRET doit contenir exactement 14 chiffres")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "Le SIRET doit contenir uniquement des chiffres")]
        public string NumeroSiret { get; set; } = null!;

        [Column("cpr_raison_sociale")]
        [Required]
        public string? RaisonSociale { get; set; } 
    }
}
