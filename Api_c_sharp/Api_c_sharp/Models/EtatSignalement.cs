using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_etatsignalement_ets")]
public class EtatSignalement
{
    [Key]
    [Column("ets_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdEtatSignalement { get; set; }
    
    [Required]
    [Column("eta_lib")]
    public string LibelleEtatSignalement { get; set; }
    
    [InverseProperty(nameof(Signalement.EtatSignalementNav))]
    public virtual ICollection<Signalement> Signalements { get; set; }
}