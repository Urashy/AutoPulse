using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_etatsignalementplainte_ets")]
public class EtatSignalementPlainte
{
    [Key]
    [Column("ets_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdEtatSignalement { get; set; }
    
    [Column("eta_lib")]
    [Required]
    public string? LibelleEtatSignalement { get; set; }
    
    [InverseProperty(nameof(Signalement.EtatSignalementNav))]
    public virtual ICollection<Signalement> Signalements { get; set; } = new List<Signalement>();

    [InverseProperty(nameof(Plainte.EtatSignalementPlaintePlainteNav))]
    public virtual ICollection<Plainte> Plaintes { get; set; } = new List<Plainte>();
}