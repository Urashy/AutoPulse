using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_boitedevitesse_boi")]
    public class BoiteDeVitesse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("boi_id")]
        public int IdBoiteDeVitesse { get; set; }

        [Column("boi_lib")]
        [Required]
        public string LibelleBoite{ get; set; } = null!;

        [InverseProperty(nameof(Voiture.BoiteVoitureNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();
    }
}
