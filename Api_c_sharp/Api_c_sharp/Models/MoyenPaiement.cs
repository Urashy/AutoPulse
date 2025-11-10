using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_moyenpaiement_mop")]
public class MoyenPaiement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("mop_id")]
    public int IdMoyenPaiement { get; set; }
    
    [Column("mop_typepaiement")]
    [Required]
    public string TypePaiement { get; set; }

    [InverseProperty(nameof(Commande.MoyenPaiementNavigation))]
    public virtual Commande CommandeNavigation { get; set; }
}