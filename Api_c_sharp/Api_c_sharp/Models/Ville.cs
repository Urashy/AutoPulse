using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_ville_vil")]
    public class Ville
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("vil_id")]
        public int IdVille { get; set; }
        [Column("vil_libelle")]
        public string Libelle { get; set; } = null!;

        [Column("pay_id")]
        public int IdPays { get; set; }

        [Column("vil_codepostal")]
        public string CodePostal { get; set; } = null!;

        [ForeignKey("pay_id")]
        [InverseProperty(nameof(Pays.Villes))]
        public virtual Pays PaysVilleNav  { get; set; } = null!;

        [InverseProperty(nameof(Adresse.VilleAdresseNav))]
        public virtual ICollection<Adresse> Adresses { get; set; } = new List<Adresse>();
    }
}
