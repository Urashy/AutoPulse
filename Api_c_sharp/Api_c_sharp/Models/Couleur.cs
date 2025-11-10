using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Api_c_sharp.Models
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

        [InverseProperty(nameof(APourCouleur.APourCouleurCouleurNavigation))]
        public virtual ICollection<APourCouleur> Voitures { get; set; } = new List<APourCouleur>();
    }
}
