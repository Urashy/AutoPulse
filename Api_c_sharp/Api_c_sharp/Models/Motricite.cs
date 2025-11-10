using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Api_c_sharp.Models
{
    [Table("t_e_motricite_mot")]

    public class Motricite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mot_id")]
        public int IdMotricite { get; set; }

        [Column("mot_lib")]
        public string LibelleMotricite { get; set; } = null!;


        [InverseProperty(nameof(Voiture.MotriciteVoitureNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();
    }
}
