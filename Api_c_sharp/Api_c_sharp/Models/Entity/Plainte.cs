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

        [ForeignKey(nameof(IdSignalement))]
        [InverseProperty(nameof(Signalement.Plaintes))]
        public virtual Signalement SignalementPainteNav { get; set; } = null!;
        

    }
}
