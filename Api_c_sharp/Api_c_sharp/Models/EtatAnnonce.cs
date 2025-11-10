using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_etatannonce_eta")]
public class EtatAnnonce
{
    [Key]
    [Column("eta_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdEtatAnnonce { get; set; }
    
    [Required]
    [Column("eta_lib")]
    public string LibelleEtatAnnonce { get; set; }
    
    [InverseProperty(nameof(Annonce.EtatAnnonceNavigation))]
    public virtual ICollection<Annonce> Annonces { get; set; }
}