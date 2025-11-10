using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_voiture_voi")]
    public class Voiture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("idvoiture")]
        public int IdVoiture{ get; set; }


        [ForeignKey("IdMarque")]
        [InverseProperty(nameof(Marque.Voitures))]
        public virtual Marque? MarqueVoitureNavigation { get; set; } = null!;



    }
}
