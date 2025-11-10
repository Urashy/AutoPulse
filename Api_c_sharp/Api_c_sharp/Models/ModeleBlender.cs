using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_modeleblender_mob")]
    public class ModeleBlender
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mob_id")]
        public int IdModeleBlender { get; set; }

        [Column("mob_lien")]
        [Required]
        public string Lien { get; set; }

        [InverseProperty(nameof(Voiture.ModeleBlenderNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; }

    }
}
