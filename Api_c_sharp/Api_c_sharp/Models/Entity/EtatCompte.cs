using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_etat_compte_etc")]
    public class EtatCompte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("etc_id")]
        public int IdEtatCompte { get; set; }

        [Required]
        [Column("etc_libelle")]
        public string Libelle { get; set; } = null!;

        [InverseProperty(nameof(Compte.EtatCompteNav))]
        public virtual ICollection<Compte> Comptes { get; set; } = new List<Compte>();
    }
}
