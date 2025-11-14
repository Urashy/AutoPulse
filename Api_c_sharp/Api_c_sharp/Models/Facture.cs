using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_facture_fac")]
public class Facture
{
    [Key]
    [Column("fac_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdFacture { get; set; }
    
    [Required]
    [Column("cmd_id")]
    public int IdCommande { get; set; }
    
    [ForeignKey(nameof(IdCommande))]
    [InverseProperty(nameof(Commande.Factures))]
    public virtual Commande CommandeFactureNav { get; set; }
}