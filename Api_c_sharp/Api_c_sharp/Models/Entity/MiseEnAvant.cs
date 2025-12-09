using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_miseavant_mav")]
    public class MiseEnAvant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mav_id")]
        public int IdMiseEnAvant { get; set; }

        [Column("mav_libelle")]
        [Required]
        public string LibelleMiseEnAvant { get; set; }

        [Column("mav_prixsemaine")]
        [Required]
        public decimal PrixSemaine { get; set; }

        [InverseProperty(nameof(Annonce.MiseEnAvantAnnonceNav))]
        public virtual ICollection<Annonce> Annonces { get; set; } = new List<Annonce>();
    }
}
