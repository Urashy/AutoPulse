using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_modele_mod")]
    public class Modele
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mod_id")]
        public int IdModele { get; set; }

        [Column("mod_lib")]
        public string LibelleModele{ get; set; } = null!;

        [Column("mar_id")]
        public int IdMarque { get; set; }

        [ForeignKey(nameof(IdMarque))]
        [InverseProperty(nameof(Marque.Modeles))]
        public virtual Marque? MarqueModeleNavigation { get; set; } = null!;

        [InverseProperty(nameof(Voiture.ModeleVoitureNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();

    }
}
