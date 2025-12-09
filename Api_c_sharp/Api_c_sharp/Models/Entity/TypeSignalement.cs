using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_typesignalement_tsi")]
    public class TypeSignalement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("tsi_id")]
        public int IdTypeSignalement { get; set; }

        [Column("tsi_libelle")]
        [Required]
        public string LibelleTypeSignalement { get; set; } = null!;

        [InverseProperty(nameof(Signalement.TypeSignalementSignalementNav))]
        public virtual ICollection<Signalement> Signalements { get; set; } = new List<Signalement>();
    }
}
