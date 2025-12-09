using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_typecompte_tco")]
    public class TypeCompte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("tco_id")]
        public int IdTypeCompte { get; set; }

        [Column("tco_libelle")]
        [Required]
        public string Libelle { get; set; } = null!;

        [Column("tco_cherchable")]
        [Required]
        public bool Cherchable { get; set; }

        [InverseProperty(nameof(Compte.TypeCompteCompteNav))]
        public virtual ICollection<Compte> Comptes { get; set; } = new List<Compte>();
    }
}
