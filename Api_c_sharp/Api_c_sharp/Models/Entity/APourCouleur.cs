using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_j_apourcouleur_apc")]

    public class APourCouleur
    {
        [Key]
        [Column("cou_id")]
        public int IdCouleur { get; set; }

        [Key]
        [Column("voi_id")]
        public int IdVoiture { get; set; }

        [ForeignKey("voi_idcouleur")]
        [InverseProperty(nameof(Voiture.APourCouleurs))]
        public virtual Voiture? APourCouleurVoitureNav { get; set; } = null!;

        [ForeignKey("voi_idcouleur")]
        [InverseProperty(nameof(Couleur.APourCouleurs))]
        public virtual Couleur? APourCouleurCouleurNav { get; set; } = null!;
    }
}
