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
        public string? DescriptionSignalement { get; set; }

        [Column("sig_datecreation")]
        public DateTime DateCreationSignalement { get; set; }

        [Column("com_id_signalant")]
        public int IdCompteSignalant { get; set; }

        [Column("com_id_signalé")]
        public int IdCompteSignale { get; set; }

        [ForeignKey("IdCompteSignalant")]
        [InverseProperty(nameof(Compte.SignalementsFaits))]
        public virtual Compte CompteSignalantNav { get; set; } = null!;

        [ForeignKey("IdCompteSignale")]
        [InverseProperty(nameof(Compte.SignalementsRecus))]
        public virtual Compte CompteSignaleNav { get; set; } = null!;
    }
}
