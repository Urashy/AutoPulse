using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_couleur_cou")]
    public class Couleur
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("cou_id")]
        public int IdCouleur { get; set; }

        [Column("cou_lib")]
        [Required]
        public string LibelleCouleur { get; set; } = null!;

        [Column("cou_codehexa")]
        [Required]
        public string CodeHexaCouleur { get; set; } = null!;

        [InverseProperty(nameof(APourCouleur.APourCouleurCouleurNav))]
        public virtual ICollection<APourCouleur> APourCouleurs { get; set; } = new List<APourCouleur>();
    }
}
