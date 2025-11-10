using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_journaux_jou")]
    public class Journaux
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

        [ForeignKey("IdTypeJournaux")]
        [InverseProperty(nameof(TypeJournaux.IdTypeJournaux))]
        public virtual TypeJournaux? TypeCompteCompteNav { get; set; } = null!;
    }
}
