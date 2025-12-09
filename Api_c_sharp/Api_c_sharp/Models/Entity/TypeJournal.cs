using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_typejournaux_tjo")]
    public class TypeJournal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("tjo_id")]
        public int IdTypeJournaux { get; set; }

        [Column("tjo_libelle")]
        [Required]
        public string LibelleTypeJournaux { get; set; } = null!;

        [InverseProperty(nameof(Journal.TypeJournauxJournauxNav))]
        public virtual ICollection<Journal> Journaux { get; set; } = new List<Journal>();
    }
}
