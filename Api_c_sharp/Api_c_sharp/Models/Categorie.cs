using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_categorie_cat")]
    public class Categorie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("cat_id")]
        public int IdCategorie { get; set; }

        [Column("cat_lib")]
        public string LibelleCategorie { get; set; } = null!;

        [InverseProperty(nameof(Voiture.CategorieVoitureNavigation))]
        public virtual ICollection<Voiture> Voitures { get; set; } = new List<Voiture>();
    }
}
