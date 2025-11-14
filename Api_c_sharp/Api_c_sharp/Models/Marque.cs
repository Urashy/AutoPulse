using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_marque_mar")]
    public class Marque
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mar_id")]
        public int IdMarque { get; set; }

        [Column("mar_lib")]
        [Required]
        public string LibelleMarque { get; set; } = null!;

        [InverseProperty(nameof(Voiture.MarqueVoitureNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();

        [InverseProperty(nameof(Modele.MarqueModeleNavigation))]
        public virtual ICollection<Modele> Modeles { get; set; } = new List<Modele>();
    }
}
