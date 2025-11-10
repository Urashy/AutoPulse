using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_journal_jou")]
    public class Journal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("jou_id")]
        public int IdJournal { get; set; }

        [Column("jou_date")]
        [Required]
        public DateTime DateJournal { get; set; } = DateTime.Now;

        [Column("jou_contenu")]
        [Required]
        public string ContenuJournal { get; set; } = string.Empty;

        [Column("tjo_id")]
        [Required]
        public int IdTypeJournal { get; set; }

        [Column("com_id")]
        [Required]
        public int IdCompte { get; set; }

        [ForeignKey("IdCompte")]
        [InverseProperty(nameof(Compte.Journaux))]
        public virtual Compte? CompteJournauxNav { get; set; } = null!;


        [ForeignKey("IdTypeJournaux")]
        [InverseProperty(nameof(TypeJournal.Journaux))]
        public virtual TypeJournal? TypeJournauxJournauxNav { get; set; } = null!;
    }
}
