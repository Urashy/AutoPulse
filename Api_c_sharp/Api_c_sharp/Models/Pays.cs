using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_pays_pay")]
    public class Pays
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("pay_id")]
        public int IdPays { get; set; }

        [Column("pay_libelle")]
        [Required]
        public string Libelle { get; set; } = null!;

        [InverseProperty(nameof(Ville.PaysVilleNav))]
        public virtual ICollection<Ville> Villes { get; set; } = new List<Ville>();
    }
}
