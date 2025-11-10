using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_miseavant_mav")]
    public class MiseEnAvant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mav_id")]
        public int IdMiseEnAvant { get; set; }

        [Column("mav_libelle")]
        public string LibellMiseEnAvant { get; set; }

        [Column("mav_prixsemaine")]
        public decimal PrixSemaine { get; set; }

        [InverseProperty(nameof(Annonce.MiseEnAvantNavigation))]
        public virtual ICollection<Annonce> Annonces { get; set; } = new List<Annonce>();
    }
}
