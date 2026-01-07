using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_etatcommande_etc")]
    public class EtatCommande
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("etc_id")]
        public int IdEtatCommande { get; set; }

        [Column("etc_libelle")]
        [Required]
        public string Libelle { get; set; } = null!;

        [InverseProperty(nameof(Commande.EtatCommandeCommandeNav))]
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();
    }
}
