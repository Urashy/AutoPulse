using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_plainte_pla")]
    public class Plainte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("pla_id")]
        public int IdPlainte { get; set; }

        [Required]
        [Column("pla_description")]
        public string Description { get; set; } = null!;

        [Required]
        [Column("pla_date_creation")]
        public DateTime DateCreation { get; set; }

        [Required]
        [Column("sig_id")] 
        public int IdSignalement { get; set; }

        [Required]
        [Column("com_id")]
        public int IdCompte { get; set; }

        [Required]
        [Column("ets_id")]
        public int IdEtat { get; set; }

        [ForeignKey(nameof(IdCompte))]
        [InverseProperty(nameof(Compte.Plaintes))]
        public virtual Compte ComptePlainteNav { get; set; } = null!;

        [ForeignKey(nameof(IdSignalement))]
        [InverseProperty(nameof(Signalement.Plaintes))]
        public virtual Signalement SignalementPlainteNav { get; set; } = null!;

        [ForeignKey(nameof(IdEtat))]
        [InverseProperty(nameof (EtatSignalementPlainte.Plaintes))]
        public virtual EtatSignalementPlainte EtatSignalementPlaintePlainteNav { get; set; } = null!;
        

    }
}
