using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_signalement_sig")]
    public class Signalement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("sig_id")]
        public int IdSignalement { get; set; }

        [Column("sig_description")]
        [Required]
        public string? DescriptionSignalement { get; set; }

        [Column("sig_datecreation")]
        [Required]
        [DefaultValue("DateTime.Now")]
        public DateTime DateCreationSignalement { get; set; } = DateTime.Now;

        [Column("com_id_signalant")]
        [Required]
        public int IdCompteSignalant { get; set; }

        [Column("com_idsignale")]
        [Required]
        public int IdCompteSignale { get; set; }

        [Column("tsi_id")]
        [Required]
        public int IdTypeSignalement { get; set; }

        [Column("ets_id")]
        [Required]
        public int IdEtatSignalement { get; set; }

        [ForeignKey(nameof(IdEtatSignalement))]
        [InverseProperty(nameof(EtatSignalement.Signalements))]
        public virtual EtatSignalement EtatSignalementNav { get; set; } = null!;

        [ForeignKey(nameof(IdCompteSignalant))]
        [InverseProperty(nameof(Compte.SignalementsFaits))]
        public virtual Compte CompteSignalantNav { get; set; } = null!;

        [ForeignKey(nameof(IdCompteSignale))]
        [InverseProperty(nameof(Compte.SignalementsRecus))]
        public virtual Compte CompteSignaleNav { get; set; } = null!;

        [ForeignKey(nameof(IdTypeSignalement))]
        [InverseProperty(nameof(TypeSignalement.Signalements))]
        public virtual TypeSignalement TypeSignalementSignalementNav { get; set; } = null!;
    }
}
