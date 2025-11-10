using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_journeau_jou")]
    public class Journal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("jou_id")]
        public int IdJournaux { get; set; }

        [Column("jou_date")]
        public DateTime DateJournaux { get; set; }

        [Column("jou_libelle")]
        public string LibelleJournaux { get; set; } = null!;

        [Column("jou_contenu")]
        public string ContenuJournaux { get; set; } = string.Empty;

        [Column("tjo_id")]
        public int IdTypeJournaux { get; set; }

        [Column("com_id")]
        public int IdCompte { get; set; }

        [ForeignKey("IdCompte")]
        [InverseProperty(nameof(Compte.Journaux))]
        public virtual Compte? CompteJournauxNav { get; set; } = null!;


        [ForeignKey("IdTypeJournaux")]
        [InverseProperty(nameof(TypeJournal.Journaux))]
        public virtual TypeJournal? TypeJournauxJournauxNav { get; set; } = null!;
    }
}
