using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_carburant_car")]
    public class Carburant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("car_id")]
        public int IdCarburant{ get; set; }

        [Column("car_lib")]
        public string LibelleCarburant{ get; set; } = null!;

        [InverseProperty(nameof(Voiture.CarburantVoitureNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();
    }
}
