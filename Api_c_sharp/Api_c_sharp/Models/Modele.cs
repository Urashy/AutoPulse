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

        [Column("mod_nbplace")]
        public int NbPlace { get; set; }

        [Column("mod_nbporte")]
        public int NbPorte { get; set; }

        [Column("mar_id")]
        public int IdMarque { get; set; }

        [ForeignKey("mod_idmarque")]
        [InverseProperty(nameof(Marque.Modeles))]
        public virtual Marque? MarqueModeleNavigation { get; set; } = null!;

    }
}
